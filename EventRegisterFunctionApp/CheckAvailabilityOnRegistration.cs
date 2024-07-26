using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CheckAvailability.Function
{
    public class CheckAvailabilityOnRegistration
    {
        private readonly ILogger<CheckAvailabilityOnRegistration> _logger;
        private static CosmosClient _cosmosClient;
        private static Container _container;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _responseQueueName;

        public CheckAvailabilityOnRegistration(ILogger<CheckAvailabilityOnRegistration> logger)
        {
            _logger = logger;
            // Initialize CosmosClient and Container
            var cosmosDbConnectionString = Environment.GetEnvironmentVariable("COSMOSDB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(cosmosDbConnectionString))
            {
                throw new InvalidOperationException("COSMOSDB_CONNECTION_STRING environment variable is not set.");
            }
            var serviceBusConnectionString = Environment.GetEnvironmentVariable("eventregistration_SERVICEBUS");
            if (string.IsNullOrEmpty(serviceBusConnectionString))
            {
                throw new InvalidOperationException("Service Bus Connection String environment variable is not set.");
            }

            _responseQueueName = Environment.GetEnvironmentVariable("RESPONSE_QUEUE_NAME");
            if (string.IsNullOrEmpty(_responseQueueName))
            {
                throw new InvalidOperationException("Response Queue environment variable is not set.");
            }

            _cosmosClient = new CosmosClient(cosmosDbConnectionString);
            _container = _cosmosClient.GetContainer("JunctionJaguar", "EventRegistration");
            _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
        }

        [Function(nameof(CheckAvailabilityOnRegistration))]
        public async Task Run(
            [ServiceBusTrigger("registrationprocess", Connection = "eventregistration_SERVICEBUS", IsSessionsEnabled = true)]
            ServiceBusReceivedMessage message)
        {
            if (message == null)
            {
                _logger.LogError("Received message is null.");
                return;
            }

            string messageBody = message.Body.ToString();
            var eventRegistration = JsonSerializer.Deserialize<EventRegistration>(messageBody);
            if (eventRegistration == null)
            {
                _logger.LogError("Failed to deserialize message body.");
                return;
            }
            var eventId = eventRegistration.EventId;
            var userId = eventRegistration.UserId;
            if (message.ApplicationProperties.TryGetValue("MaxReg", out object maxRegObject) && maxRegObject is int maxReg)
            {
                _logger.LogInformation("Processing event with ID: {eventId} and MaxReg: {maxReg}", eventId, maxReg);

                // Query CosmosDB to find records with the specified eventId
                var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.EventId = @eventId")
                                .WithParameter("@eventId", eventId);

                var iterator = _container.GetItemQueryIterator<int>(query);
                int count = 0;

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    count = response.FirstOrDefault();
                }

                _logger.LogInformation("Total records for eventId {eventId}: {count}", eventId, count);


                // Check if the number of records is less than Max registerer
                var result = count < maxReg;

                if (result)
                {
                    // Save the registration to CosmosDB
                    await _container.CreateItemAsync(eventRegistration);
                    _logger.LogInformation("Registration stored in CosmosDB for UserId: {userId}, EventId: {eventId}", eventRegistration.UserId, eventRegistration.EventId);
                }
                else
                {
                    _logger.LogInformation("Registration limit reached for EventId: {eventId}", eventId);
                }

                // Send the result to the response queue
                var responseMessage = new ServiceBusMessage(JsonSerializer.Serialize(
                    new RegistrationResult{
                        UserId = userId,
                        EventId = eventId,
                        Result = result
                }))
                {
                    SessionId = message.SessionId
                };
                ServiceBusSender sender = _serviceBusClient.CreateSender(_responseQueueName);
                await sender.SendMessageAsync(responseMessage);
            }
            else
            {
                _logger.LogError("MaxReg (The maximum number of registered participants) not found in ApplicationProperties.");
            }
        }
    }

    public class EventRegistration
    {
        public string UserId { get; set; }
        public string EventId { get; set; }
    }

    public class RegistrationResult
    {
        public string UserId { get; set; }
        public string EventId { get; set; }
        public bool Result { get; set; }
    }
}
