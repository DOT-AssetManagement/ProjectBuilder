using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_identity_UserClaims")]
    public class UserClaimEntity : IEntity<int>
    {
        public UserClaimEntity(string name, string value)
        {
            Name = name;
            Value = value;
        }

        [Column("UserClaimId")]
        public int EntityId { get; set; }
        [Column("UserId")]
        public long UserId { get; set; }
        public UserEntity User { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
