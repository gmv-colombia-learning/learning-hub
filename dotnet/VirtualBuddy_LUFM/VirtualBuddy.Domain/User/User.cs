namespace VirtualBuddy.Domain.User
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? NickName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User(Guid id, string email, string? nickName, DateTime createdAt, DateTime? updatedAt)
        {
            Id = id;
            Email = email;
            NickName = nickName;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
    }
}
