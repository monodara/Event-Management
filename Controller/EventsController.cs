using System.Security.Claims;
using AutoMapper;
using EventManagementApi.Database;
using EventManagementApi.DTO;
using EventManagementApi.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;


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
        private readonly IMapper _mapper;

        public EventsController(CosmosClient cosmosClient, IConfiguration configuration, ApplicationDbContext context, IMapper mapper)
        {
            _cosmosClient = cosmosClient;
            _configuration = configuration;
            _container = _cosmosClient.GetContainer(_configuration["CosmosDb:DatabaseName"], "Events");
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
        // [Authorize(Policy = "EventProvider")]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] EventCreateDto eventCreateDto)
        {
            var userRoles = HttpContext.User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();

            Console.WriteLine("Current user roles: " + string.Join(", ", userRoles));
            Console.WriteLine(HttpContext.User.Identity.IsAuthenticated);
            var eventToCreate = _mapper.Map<EventCreateDto, Event>(eventCreateDto);
            await _dbContext.Events.AddAsync(eventToCreate);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateEvent), new { id = eventToCreate.Id }, eventToCreate);
        }

        // Accessible by all authenticated users
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var foundEventById = await _dbContext.Events.FirstOrDefaultAsync(u => u.Id == id);
            return Ok(foundEventById);
        }

        // Accessible by Event Providers
        [HttpPut("{id}")]
        [Authorize(Policy = "EventProvider")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] EventUpdateDto updatedEvent)
        {
            var foundEvent = await GetEventById(id);
            _dbContext.Update(_mapper.Map<EventUpdateDto, Event>(updatedEvent));
            await _dbContext.SaveChangesAsync();
            return Ok(foundEvent);
        }

        // Accessible by Admins
        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            return null;
        }

        // User can register for an event
        [HttpPost("{id}/register")]
        [Authorize(Policy = "User")]
        public async Task<IActionResult> RegisterForEvent(string id)
        {
            return null;
        }

        // User can unregister from an event
        [HttpDelete("{id}/unregister")]
        [Authorize(Policy = "User")]
        public async Task<IActionResult> UnregisterFromEvent(string id)
        {
            return null;
        }
    }
}
