using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using EventManagementApi.Database;
using EventManagementApi.DTO;
using EventManagementApi.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;


namespace EventManagementApi.Controllers
{
    [Route("api/v1/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly Container _registrationContainer;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _queueName;
        private readonly IMapper _mapper;

        public EventsController(
            CosmosClient cosmosClient, 
            IConfiguration configuration, 
            BlobServiceClient blobServiceClient, 
            ApplicationDbContext context, 
            ServiceBusClient serviceBusClient,
            IMapper mapper)
        {
            _cosmosClient = cosmosClient;
            _configuration = configuration;
            _container = _cosmosClient.GetContainer(_configuration["CosmosDb:DatabaseName"], "Events");
            _blobServiceClient = _blobServiceClient = blobServiceClient;
            _serviceBusClient = serviceBusClient;
            _queueName = _configuration["ServiceBus:QueueName"];
            _registrationContainer = _cosmosClient.GetContainer(_configuration["CosmosDb:DatabaseName"], "EventRegistrations");
            _dbContext = context;
            _mapper = mapper;
        }

        // Accessible by all authenticated users
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var eventList = await _dbContext.Events.ToListAsync();
            return Ok(eventList);
        }

        // Accessible by Event Providers
        [HttpPost]
        [Authorize(Policy = "EventProvider")]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] EventCreateDto eventCreateDto)
        {
            var eventToCreate = _mapper.Map<EventCreateDto, Event>(eventCreateDto);
            await _dbContext.Events.AddAsync(eventToCreate);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateEvent), new { id = eventToCreate.Id }, eventToCreate);
        }

        // Accessible by all authenticated users
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var foundEventById = await _dbContext.Events.Include(e => e.Organizer).FirstOrDefaultAsync(u => u.Id == id);
            var eventReadDto = _mapper.Map<Event, EventReadDto>(foundEventById);
            return Ok(eventReadDto);
        }

        // Accessible by Event Providers
        [HttpPut("{id}")]
        [Authorize(Policy = "EventProvider")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] EventUpdateDto updatedEvent)
        {
            var foundEvent = await GetEventById(id);
            _dbContext.Events.Update(_mapper.Map<EventUpdateDto, Event>(updatedEvent));
            await _dbContext.SaveChangesAsync();
            return Ok(foundEvent);
        }

        // Accessible by Admins
        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var foundEvent = await _dbContext.Events.FirstOrDefaultAsync(u => u.Id == id);
            _dbContext.Events.Remove(foundEvent);
            await _dbContext.SaveChangesAsync();
            return Ok(true);
        }

        [HttpPost("{id}/upload")]
        [Authorize(Policy = "EventProvider")]
        public async Task<IActionResult> UploadEventFile(Guid id, IFormFile file)
        {
            // Check if event exists
            var eventEntity = await _dbContext.Events.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound(new { Message = "Event not found." });
            }

            // Check if file is provided
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "No file provided." });
            }

            // Upload file to Azure Blob Storage
            var containerClient = _blobServiceClient.GetBlobContainerClient(_configuration["BlobStorage:EventMaterialContainer"]);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            // Update Event with the new file URI
            var fileUri = blobClient.Uri.ToString();
            eventEntity.DocumentUris.Add(fileUri);

            _dbContext.Events.Update(eventEntity);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "File uploaded successfully.", FilePath = blobClient.Uri.ToString() });
        }


        // User can register for an event
        [HttpPost("{id}/register")]
        [Authorize(Policy = "User")]
        public async Task<IActionResult> RegisterForEvent(string id)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var foundEventById = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id.ToString() == id);
            // Create the event registration record
            var eventRegistration = new EventRegistration
            {
                UserId = userId,
                EventId = id
            };
            // Serialize the registration object to JSON
            var messageBody = JsonSerializer.Serialize(eventRegistration);
            var message = new ServiceBusMessage(messageBody)
                                    {
                                        // Add a property of maximum registerer
                                        ApplicationProperties =
                                            {
                                                ["MaxReg"] = foundEventById.MaxReg
                                            },
                                        SessionId = Guid.NewGuid().ToString(),
            };

            // Create a sender client
            // ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName);
            // Send message
            try
            {
                // Create a sender client
                ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName);

                // Send message
                await sender.SendMessageAsync(message);

                // Log message sent
                Console.WriteLine("Registration has been sent out");
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error sending message: {ex.Message}");
                return StatusCode(500, new { Message = "Error sending message to Service Bus" });
            }

            // Save registration to CosmosDB 

            // Save registration to PostgreSQL
            // await _dbContext.EventRegistrations.AddAsync(eventRegistration);
            // await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(RegisterForEvent), new { Message = "Thank you for registering this event!" }, eventRegistration);
        }

        // User can unregister from an event
        [HttpDelete("{id}/unregister")]
        [Authorize(Policy = "User")]
        public async Task<IActionResult> UnregisterFromEvent(string id)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var foundRegistration = await _dbContext.EventRegistrations
                                             .Where(er => er.UserId == userId && er.EventId == id)
                                             .FirstOrDefaultAsync();

            if (foundRegistration == null)
            {
                return NotFound(new { Message = "Registration not found." });
            }

            _dbContext.EventRegistrations.Remove(foundRegistration);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "You have unregistered from this event." });
        }
    }
}
