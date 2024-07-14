using EventManagementApi.Database;
using EventManagementApi.DTO;
using EventManagementApi.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
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

        public EventsController(CosmosClient cosmosClient, IConfiguration configuration, ApplicationDbContext context)
        {
            _cosmosClient = cosmosClient;
            _configuration = configuration;
            _container = _cosmosClient.GetContainer(_configuration["CosmosDb:DatabaseName"], "Events");
            _registrationContainer = _cosmosClient.GetContainer(_configuration["CosmosDb:DatabaseName"], "EventRegistrations");
            _dbContext = context;
        }

        // Accessible by all authenticated users
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            return null;
        }

        // Accessible by Event Providers
        [HttpPost]
        // [Authorize(Policy = "EventProvider")]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] EventCreateDto eventCreateDto)
        {
    

            // var eventToCreate = eventCreateDto.ToEvent();
            // _dbContext.Events.Add(eventToCreate);
            // await _dbContext.SaveChangesAsync();
            // return CreatedAtAction(nameof(CreateEvent), new { id = eventToCreate.Id }, CreateEvent);
            return null;
        }

        // Accessible by all authenticated users
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(string id)
        {
            return null;
        }

        // Accessible by Event Providers
        [HttpPut("{id}")]
        [Authorize(Policy = "EventProvider")]
        public async Task<IActionResult> UpdateEvent(string id, [FromBody] Event updatedEvent)
        {
            return null;
        }

        // Accessible by Admins
        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteEvent(string id)
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
