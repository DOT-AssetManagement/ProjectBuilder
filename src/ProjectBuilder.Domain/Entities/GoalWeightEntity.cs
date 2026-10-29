using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProjectBuilder.Entities
{
    [Table("tbl_dss_GoalWeights")]
    public class GoalWeightEntity
    {
        public int? GoalId { get; set; }
        public float? Weight { get; set; }
    }
}
