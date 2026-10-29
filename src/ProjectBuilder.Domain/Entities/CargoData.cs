using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    // tbl_lib_CargoData — library treatment attribute values (read-only here).
    [Table("tbl_lib_CargoData")]
    public class CargoData : Entity
    {
        public Guid ImportSessionId { get; set; }
        public Guid LibraryId { get; set; }
        public string AssetType { get; set; }
        public Guid ImportTimeGeneratedGuid { get; set; }
        public int AttributeNo { get; set; }
        // Nullable in the DB — a bare `string` would make EF read it with an unguarded
        // reader.GetString() and throw SqlNullValueException on the first NULL row.
        public string? TextValue { get; set; }
        public double? NumericValue { get; set; }

        public override object?[] GetKeys() => new object[] { ImportSessionId, LibraryId, AssetType, ImportTimeGeneratedGuid, AttributeNo };
    }
}
