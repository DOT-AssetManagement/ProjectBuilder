using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_identity_Roles")]
    public class RoleEntity : IEntity<int>
    {
        public RoleEntity(string name)
        {
            Name = name;
        }

        [Column("RoleId")]
        public int EntityId { get; set; }
        public string Name { get; set; }
    }
}
