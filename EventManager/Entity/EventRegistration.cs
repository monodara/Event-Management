using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagementApi.Entity
{
    public class EventRegistration
    {
        public Guid EventId { get; set; }
        [ForeignKey("EventId")]
        public Event Event { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}