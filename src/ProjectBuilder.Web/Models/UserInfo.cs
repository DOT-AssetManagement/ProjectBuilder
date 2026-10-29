namespace ProjectBuilder.Web.Models
{
    public class UserInfo
    {
        public string Name { get; set; }

        public bool HasAdminAccess { get; set; }

        // Below is for SimulationPowerUser
        public bool HasSimulationAccess { get; set; }

        public string Email { get; set; }
    }
}
