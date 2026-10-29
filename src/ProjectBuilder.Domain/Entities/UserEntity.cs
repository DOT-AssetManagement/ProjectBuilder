using ProjectBuilder.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_identity_Users")]
    public class UserEntity : Entity<long>
    {
        [Column("UserId")]
        public long Id { get; set; }
        public Guid B2CUserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public ICollection<UserRoleEntity> UserRoles { get; set; }
        public ICollection<UserClaimEntity> UserClaims { get; set; }
       // public List<RoleEntity> Roles { get; set; }


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
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }

    }


}
