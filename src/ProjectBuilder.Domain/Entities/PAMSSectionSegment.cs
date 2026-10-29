using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_pams_MaintainableAssetsSegmentation")]
    public class PAMSSectionSegment : Entity<string>
    {
        // AssetId is NVARCHAR in the live table, not a GUID — fixed to match (see live-DB check).
        [Column("AssetId")]
        public string Id { get; set; }
        public string NetworkId { get; set; }
        public byte District { get; set; }
        [Column("Cnty")]
        public byte CountyId { get; set; }
        // Route/FromSection/ToSection are SMALLINT in the live table, not int — confirmed
        // by a runtime InvalidCastException (GetInt32 on a smallint column) once a query
        // actually materialized these columns instead of projecting around them (same
        // root cause as the Direction/bit fix above).
        public short Route { get; set; }
        public string? Asset { get; set; }
        [Column("Direction")]
        public bool Direction { get; set; }
        public short FromSection { get; set; }
        public short ToSection { get; set; }
        public int? Area { get; set; }
        public bool Interstate { get; set; }
        public byte? Lanes { get; set; }
        public byte? Width { get; set; }
        public int? Length { get; set; }
        public string? SurfaceName { get; set; }
        public double? Risk { get; set; }
        // Live DB: 100% NULL today — every row is auto-populated, none have been through a manual edit.
        public string? PopulatedBy { get; set; }
        public DateTime? PopulatedAt { get; set; }
        public Guid? ImportSessionId { get; set; }
        public bool? IsMaster { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
