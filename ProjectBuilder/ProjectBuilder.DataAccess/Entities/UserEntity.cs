using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_identity_Users")]
    public class UserEntity : IEntity<long>
    {
        [Column("UserId")]
        public long EntityId { get; set; }
        public Guid B2CUserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsMapActive { get; set; }
        public ICollection<UserRoleEntity> UserRoles { get; set; }
        public ICollection<UserClaimEntity> UserClaims { get; set; }


        public void AddRole(int roleId)
        {
            if (UserRoles == null)
            {
                UserRoles = new List<UserRoleEntity>();
            }

            UserRoles.Add(new UserRoleEntity(roleId));
        }

        public void AddClaim(string name, string value)
        {
            if (UserClaims == null)
            {
                UserClaims = new List<UserClaimEntity>();
            }

            UserClaims.Add(new UserClaimEntity(name, value));
        }

    }


}
