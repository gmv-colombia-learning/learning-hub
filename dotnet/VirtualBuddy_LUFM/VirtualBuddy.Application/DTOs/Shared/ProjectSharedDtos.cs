namespace VirtualBuddy.Application.DTOs.Shared
{
    public class TechnologyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class ProjectMemberDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }
}
