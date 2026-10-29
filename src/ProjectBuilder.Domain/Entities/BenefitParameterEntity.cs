using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProjectBuilder.Entities
{
    [Table("tbl_dss_BenefitParameters")]
    public class BenefitParameterEntity
    {
        public string Family { get; set; } 
        public byte Code { get; set; }
        public string? Description { get; set; }
        public float? Val { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
