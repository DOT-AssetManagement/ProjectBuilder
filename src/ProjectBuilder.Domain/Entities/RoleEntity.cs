using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_identity_Role")]
    public class RoleEntity : Entity<int>
    {
        public RoleEntity(string name)
        {
            Name = name;
        }

        [Column("RoleId")]
        public int Id { get; set; }
        public string Name { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
