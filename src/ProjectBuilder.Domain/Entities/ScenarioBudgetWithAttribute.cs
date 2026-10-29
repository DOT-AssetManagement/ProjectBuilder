using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    // tbl_pb_ExtendedScenarioBudgetWithAttributes — budget expanded by criteria attributes.
    [Table("tbl_pb_ExtendedScenarioBudgetWithAttributes")]
    public class ScenarioBudgetWithAttribute : Entity
    {
        [Column("ScenId")]
        public int ScenId { get; set; }
        public int YearWork { get; set; }
        public string District { get; set; }
        public string IsInterstate { get; set; }
        public string AssetType { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public decimal? Budget { get; set; }

        public override object?[] GetKeys() => new object[] { ScenId, YearWork, District, IsInterstate, AssetType, AttributeName, AttributeValue };
    }
}
