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
        
    }

    public class EventUpdateDto{
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Date { get; set; }
    }
    public class EventReadDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Date { get; set; }
        public ApplicationUser Organizer { get; set; }
    }
}