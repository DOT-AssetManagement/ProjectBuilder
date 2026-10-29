using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProjectBuilder.Entities
{
    [Table("tbl_dss_MeasureWeights")]
    public class MeasureWeightEntity
    {
        public int? MeasureId { get; set; }
        public int? GoalId { get; set; }
        public float? Weight { get; set; }
    }
}
