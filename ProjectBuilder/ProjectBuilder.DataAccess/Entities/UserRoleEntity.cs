using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_identity_UserRoles")]
    public class UserRoleEntity : IEntity<int>
    {
        [Column("UserRoleId")]
        public int EntityId { get; set; }
        [Column("UserId")]
        public long UserId { get; set; }
        public UserEntity User { get; set; }
        [Column("RoleId")]
        public int RoleId { get; set; }

        public UserRoleEntity(int roleId)
        {
            RoleId = roleId;
        }

        public RoleEntity Role { get; set; }
    }
}
