using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagementApi.Entity;

namespace EventManagementApi.DTO
{
    public class EventCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Date { get; set; }
        public string OrganizerId { get; set; }

        public Event ToEvent()
        {
            var eventToCreate = new Event();
            eventToCreate.Name = Name;
            eventToCreate.Description = Description;
            eventToCreate.Location = Location;
            eventToCreate.Date = Date;
            eventToCreate.OrganizerId = OrganizerId;
            return eventToCreate;
        }
    }
}