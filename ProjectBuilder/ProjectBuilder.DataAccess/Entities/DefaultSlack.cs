using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_pb_DefaultSlackPeriodsByAssetClass")]
    public class DefaultSlack : IEntity<string>
    {
        [Column("AssetType")]
        public string EntityId { get; set; }
        public int MoveBefore { get; set; }
        public int MoveAfter { get; set; }
    }
}
