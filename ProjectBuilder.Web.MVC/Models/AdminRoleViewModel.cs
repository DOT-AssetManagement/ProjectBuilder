using ProjectBuilder.Core;

namespace ProjectBuilder.Web.MVC.Models
{
    public class AdminRoleViewModel
    {
        public List<RoleModel> Roles { get; set; } = new List<RoleModel>();
        public int RoleId { get; set; }
        public string Name { get; set; }
    }
}
