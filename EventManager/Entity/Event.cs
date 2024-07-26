namespace EventManagementApi.Entity
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Date { get; set; }
        public string OrganizerId { get; set; }
        public ApplicationUser Organizer { get; set; }
        public List<string> DocumentUris { get; set; } = new List<string>();
        public int MaxReg { get; set;}
        public bool IsOpenForReg { get; set;} = true;
    }
}