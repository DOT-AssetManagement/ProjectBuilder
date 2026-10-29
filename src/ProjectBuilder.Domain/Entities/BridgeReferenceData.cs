using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_pb_BridgeReferenceDataFromPennshare")]
    public class BridgeReferenceData : Entity<string>
    {
        [Column("BRKEY")]
        public string Id { get; set; }
        [Column("DISTRICT")]
        public int? District { get; set; }
        [Column("County_Code")]
        public int? CountyCode { get; set; }
        [Column("BRIDGE_ID")]
        public string BridgeId { get; set; }
        public int? Route { get; set; }
        public int? Segment { get; set; }
        [Column("Offset")]
        public int? OffSet { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
