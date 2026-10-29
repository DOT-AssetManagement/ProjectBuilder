using System;

namespace ProjectBuilder.Entities
{
    // CargoAttributeOptions DB view — keyless. Distinct attribute values per library/asset/attribute,
    // used as the fallback value list for Analysis Method criteria dropdowns.
    public class CargoAttributeOption
    {
        public string AttributeValue { get; set; }
        public Guid LibraryId { get; set; }
        public string AssetType { get; set; }
        public int AttributeNo { get; set; }
        public string AttributeName { get; set; }
    }
}
