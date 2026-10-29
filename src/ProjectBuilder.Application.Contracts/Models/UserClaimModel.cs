using System.Diagnostics.CodeAnalysis;

namespace ProjectBuilder.Models
{
    [ExcludeFromCodeCoverage]
    public class UserClaimModel
    {
        public int EntityId { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
