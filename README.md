# Event Management System API

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![Azure](https://img.shields.io/badge/Azure-Enabled-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Enabled-blue)

## Project Description

The Event Management System is a web application built with ASP.NET Core, Entity Framework Core, and PostgreSQL. It uses ASP.NET Core Identity for user authentication and authorization, and integrates with Azure services for storage and monitoring.

## Features

1. **User registration and authentication** with ASP.NET Core Identity.
2. **Role-based authorization** (Admin, EventProvider, User).
3. **CRUD operations** for events.
4. **Event registration with FIFO processing** using Azure Service Bus and Azure Functions.
5. **Storage of event metadata and user interactions** in Cosmos DB for NoSQL. (_Optional_)
6. **Storage of images and documents** in Azure Blob Storage.
7. **Caching of event data** with Redis. (_Optional_)
8. **Monitoring and diagnostics** with Azure Application Insights.

## Database Structure

- **Cosmos DB for PostgreSQL**
  - **Users:** Stores user information including authentication details.
  - **Events:** Stores details of each event.
  - **EventRegistrations:** Tracks which users have registered for which events.
- **Cosmos DB for NoSQL**
  - **EventMetadata:** Stores additional metadata for events (tags, type, ). When users search for events, we can use metadata.
  - **UserInteractions:** Stores user interactions related to events.
  - Example:
      ````
        public async Task<IEnumerable<EventMetadata>> SearchEventsByMetadataAsync(string[] tags, string type, string category)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE ARRAY_CONTAINS(@tags, c.tags) AND c.type = @type AND c.category = @category")
                .WithParameter("@tags", tags)
                .WithParameter("@type", type)
                .WithParameter("@category", category);

            var iterator = _eventMetadataContainer.GetItemQueryIterator<EventMetadata>(query);
            var results = new List<EventMetadata>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<IEnumerable<Event>> GetMostViewedEventsAsync()
        {
            var query = new QueryDefinition("SELECT c.eventId, COUNT(c.id) as views FROM c WHERE c.interactionType = 'view' GROUP BY c.eventId ORDER BY views DESC");

            var iterator = _userInteractionsContainer.GetItemQueryIterator<UserInteraction>(query);
            var results = new List<UserInteraction>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            var eventIds = results.Select(r => r.EventId).Distinct();
            var events = new List<Event>();

            foreach (var eventId in eventIds)
            {
                var eventResponse = await _eventsContainer.ReadItemAsync<Event>(eventId, new PartitionKey(eventId));
                events.Add(eventResponse.Resource);
            }

            return events;
        }
    ````
- **Azure Blob Storage**
  - **EventImages:** Stores images related to events.
  - **UserProfiles:** Stores user profile pictures.
  - **EventDocuments:** Stores documents related to events.

## Workflow

1. **User accesses the Event Management System web app and signs in.**
2. **Browser pulls static resources from Azure CDN.**
3. **User searches for events by metadata.** The web app checks Redis for cached search results.
4. **If cache miss,** the web app queries Cosmos DB for event data and stores the results in Redis.
5. **Web app retrieves event details from Redis** if available, otherwise from Cosmos DB, and updates the cache.
6. **Pulls event-related images and documents from Azure Blob Storage.**
7. **User registers/unregisters for an event.** Registration information is placed in an Azure Service Bus queue with sessions enabled.
8. **Azure Functions processes the registration/unregistration** from the Service Bus queue, ensuring FIFO order, use transaction to modify event's properties.
9. **Azure Functions updates the registration status in Cosmos DB** and may trigger other necessary actions such as sending confirmation emails.
10. **Application Insights monitors and diagnoses issues** in the application.