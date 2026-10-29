using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProjectBuilder.Entities
{
    [Table("tbl_dss_MeasureScalingLinear")]
    public class MeasureScalingLinearEntity
    {
        public int MeasureId { get; set; }
        public int? MinScore { get; set; }
        public float? MeasureScoreA { get; set; }
        public float? MaxScore { get; set; }
        public float? MeasureScoreB { get; set; }
        public float? Slope { get; set; }
        public float? Intercept { get; set; }
    }
}
