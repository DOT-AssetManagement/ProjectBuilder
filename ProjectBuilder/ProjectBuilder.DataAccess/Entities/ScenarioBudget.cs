using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_pb_ExtendedScenarioBudget")]
    public class ScenarioBudget : IEntity<int>
    {       
        [Column("YearWork")]
        public int EntityId { get; set; }
        [Column("District")]
        public int District { get; set; }
        [Column("ScenId")]
        public int ScenarioId { get; set; }
        public bool IsInterstate { get; set; }
        public string AssetType { get; set; }
        public decimal? Budget { get; set; }
    }
}
