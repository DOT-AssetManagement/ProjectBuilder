using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_identity_UserClaims")]
    public class UserClaimEntity : Entity<int>
    {
        public UserClaimEntity(string name, string value)
        {
            Name = name;
            Value = value;
        }

        [Column("UserClaimId")]
        public int Id { get; set; }
        [Column("UserId")]
        public long UserId { get; set; }
        public UserEntity User { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
