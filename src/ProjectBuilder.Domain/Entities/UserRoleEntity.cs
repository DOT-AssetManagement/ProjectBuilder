using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_identity_UserRoles")]
    public class UserRoleEntity : Entity<int>
    {
        [Column("UserRoleId")]
        public int Id { get; set; }
        [Column("UserId")]
        public long UserId { get; set; }

        [ForeignKey("UserId")]
        public UserEntity User { get; set; }

        [Column("RoleId")]
        public int RoleId { get; set; }

        public UserRoleEntity(int roleId)
        {
            RoleId = roleId;
        }

        public RoleEntity Role { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
