using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess.Entities
{
    public class UserTreatmentType : IEntity<int>
    {
        [Column("UserTreatmentTypeNo")]
        public int EntityId { get; set; }
        public string UserTreatmentName { get; set; }

        public override string ToString()
        {
            return UserTreatmentName;
        }
    }
}
