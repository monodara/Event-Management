using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Npgsql; 
using System.Net.Mail;

namespace RegisterResultNotiFunctionApp
{
    public class SendEmailOnRegisterResult
    {
        private readonly ILogger<SendEmailOnRegisterResult> _logger;

        public SendEmailOnRegisterResult(ILogger<SendEmailOnRegisterResult> logger)
        {
            _logger = logger;
        }

        [Function(nameof(SendEmailOnRegisterResult))]
        public async Task Run(
            [ServiceBusTrigger("availability-response-queue", Connection = "eventregistration_SERVICEBUS")]
            ServiceBusReceivedMessage message)
        {
            _logger.LogInformation("Triggering response function....");
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);


            var messageBody = message.Body.ToString();
            var registrationResult = JsonSerializer.Deserialize<RegistrationResult>(messageBody);

            if (registrationResult == null)
            {
                _logger.LogError("Failed to deserialize message body.");
                return;
            }
            string connectionString = Environment.GetEnvironmentVariable("PostgreSqlConnectionString");
            var eventInfo = await GetEventInfoAsync(registrationResult.EventId, connectionString);
            var userInfo = await GetUserInfoAsync(registrationResult.UserId, connectionString);

        }
        private async Task<EventInfo> GetEventInfoAsync(string eventId, string connectionString)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT name, location, date FROM events WHERE id = @EventId";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("EventId", eventId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new EventInfo { Name = reader.GetString(0), Location = reader.GetString(1), Date = reader.GetString(2) };
            }

            return null;
        }

        private async Task<UserInfo> GetUserInfoAsync(string userId, string connectionString)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT email FROM users WHERE id = @UserId";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("UserId", userId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserInfo { Email = reader.GetString(0) };
            }

            return null;
        }
        private async Task SendEmailAsync(string emailAddress, string eventName, string registrationResult)
        {
            string smtpPassword = Environment.GetEnvironmentVariable("SmtpPassword");
            using var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential("monodara.lu@gmail.com", smtpPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("monodara.lu@gmail.com"),
                Subject = "Event Registration Result",
                Body = $"Your registration for event '{eventName}' has been {registrationResult}.",
                IsBodyHtml = false,
            };
            mailMessage.To.Add(emailAddress);

            await client.SendMailAsync(mailMessage);
        }
    }

    public class RegistrationResult
    {
        public string EventId { get; set; }
        public string UserId { get; set; }
        public string Result { get; set; }
    }
    public class EventInfo
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Date { get; set; }
    }


    public class UserInfo
    {
        public string Email { get; set; }
    }
}
