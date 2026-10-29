using System.Diagnostics.CodeAnalysis;

namespace ProjectBuilder.Models
{
    [ExcludeFromCodeCoverage]
    public class UserRoleModel
    {
        public int EntityId { get; set; }
        public long UserId { get; set; }
        public int RoleId { get; set; }
    }
}
