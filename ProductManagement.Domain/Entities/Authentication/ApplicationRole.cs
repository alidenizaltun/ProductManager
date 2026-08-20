using Microsoft.AspNetCore.Identity;

namespace ProductManagement.Domain.Entities
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
    }
}
