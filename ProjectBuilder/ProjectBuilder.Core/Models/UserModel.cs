using System;
using System.Diagnostics.CodeAnalysis;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class UserModel
    {
        public long EntityId { get; set; }
        public Guid B2CUserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsMapActive { get; set; }
        public string RoleName { get; set; }

    }
}
