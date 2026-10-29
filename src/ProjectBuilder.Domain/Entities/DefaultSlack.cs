using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_pb_DefaultSlackPeriodsByAssetClass")]
    public class DefaultSlack : Entity<string>
    {
        [Column("AssetType")]
        public string Id { get; set; }
        public int MoveBefore { get; set; }
        public int MoveAfter { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
