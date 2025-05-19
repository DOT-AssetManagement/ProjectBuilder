using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core.Models
{
    public class UserTreatmentTypeModel
    {
        public int UserTreatmentsId { get; set; }
        public string UserTreatmentName { get; set; }
    }
}
