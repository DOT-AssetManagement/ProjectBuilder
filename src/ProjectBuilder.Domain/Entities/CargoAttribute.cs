using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    // tbl_lib_CargoAttributes — library treatment attribute definitions (read-only here).
    [Table("tbl_lib_CargoAttributes")]
    public class CargoAttribute : Entity
    {
        public Guid LibraryId { get; set; }
        public int AttributeNo { get; set; }
        public string AssetType { get; set; }
        public string AttributeName { get; set; }
        public string AttributeType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

        public override object?[] GetKeys() => new object[] { LibraryId, AttributeNo, AssetType };
    }
}
