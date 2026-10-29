using System.Diagnostics.CodeAnalysis;

namespace ProjectBuilder.Models
{
    [ExcludeFromCodeCoverage]
    public class RoleModel
    {
        public int EntityId { get; set; }
        public string Name { get; set; }

        public bool IsDeleteable { get; set; }
        public RoleModel()
        { 
        }
        public RoleModel(string name)
        {
            Name = name;
        }
    }
}
