using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;

namespace ProjectBuilder.Web.MVC.Models
{
    public class AdminUsersViewModel
    {
        public List<UserModel> Users { get; set; } = new List<UserModel>();
        public List<RoleModel> Roles { get; set; } = new List<RoleModel>();
        public long EntityId { get; set; }
        public Guid B2CUserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsMapActive { get; set; }
        public string RoleName { get; set; }
        [BindProperty(Name = "updatedrole")]
        public string UpdatedRole { get; set; }

        public bool UserIsInRole(string role)
        {
            return RoleName != null && RoleName.Contains(role);
        }
    }
}
