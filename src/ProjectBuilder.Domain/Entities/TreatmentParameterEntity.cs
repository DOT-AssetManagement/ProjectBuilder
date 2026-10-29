using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_pb_UserTreatmentParameters")]
    public class TreatmentParameterEntity : Entity<int>
    {
        [Column("UserTreatmentTypeNo")]
        public int Id { get; set; }
        public double? UserTreatmentBenefitWeight { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
