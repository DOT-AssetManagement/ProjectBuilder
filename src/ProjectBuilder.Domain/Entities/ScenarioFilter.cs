using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    // tbl_pb_ScenarioFilters — Analysis Method criteria (one row per condition).
    [Table("tbl_pb_ScenarioFilters")]
    public class ScenarioFilter : Entity
    {
        [Column("ScenId")]
        public int ScenId { get; set; }
        public int FilterNo { get; set; }
        public string? AssetType { get; set; }
        public string? AttributeName { get; set; }
        public string? Operation { get; set; }
        public string? TextValue { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

        public override object?[] GetKeys() => new object[] { ScenId, FilterNo, AssetType, AttributeName, Operation, TextValue };
    }
}
