using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBuilder.Entities
{
    public class OpenIddictScopeEntity
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? Descriptions { get; set; }
        public string? DisplayName { get; set; }
        public string? DisplayNames { get; set; }
        public string? Name { get; set; }
        public string? Properties { get; set; }
        public string? Resources { get; set; }
        public string ExtraProperties { get; set; }
        public string ConcurrencyStamp { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public Guid? LastModifierId { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeleterId { get; set; }
        public DateTime? DeletionTime { get; set; }
    }
}
