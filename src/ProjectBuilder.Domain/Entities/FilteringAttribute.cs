using System;

namespace ProjectBuilder.Entities
{
    // FilteringAttributes DB view — keyless. The set of filterable attributes per library
    // (asset type + attribute name + attribute type) used by Analysis Method and Budget criteria.
    public class FilteringAttribute
    {
        public string? AssetType { get; set; }
        public string? AttributeName { get; set; }
        public string? AttributeType { get; set; }
        public Guid LibraryId { get; set; }
    }
}
