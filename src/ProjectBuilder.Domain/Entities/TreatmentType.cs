using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    public class TreatmentType : Entity<string>
    {
        [Column("Treatment")]
        public string Id { get; set; }
        [Column("MoveEarlierBufferOverride")]
        public int? MoveEarlier { get; set; }
        [Column("MoveLaterBufferOverride")]
        public int? MoveLater { get; set; }
        public string AssetType { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
