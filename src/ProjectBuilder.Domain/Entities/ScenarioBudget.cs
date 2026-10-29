using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_pb_ExtendedScenarioBudget")]
    public class ScenarioBudget : Entity<int>
    {       
        [Column("YearWork")]
        public int Id { get; set; }
        [Column("District")]
        public int District { get; set; }
        [Column("ScenId")]
        public int ScenarioId { get; set; }
        public bool IsInterstate { get; set; }
        public string AssetType { get; set; }
        public decimal? Budget { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
