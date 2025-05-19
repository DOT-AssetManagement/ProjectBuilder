using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess.Entities
{
    [Table("tbl_pb_UserTreatmentParameters")]
    public class TreatmentParameterEntity : IEntity<int>
    {
        [Column("UserTreatmentTypeNo")]
        public int EntityId { get; set; }
        public double? UserTreatmentBenefitWeight { get; set; }
    }
}
