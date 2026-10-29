using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    public class UserTreatmentType : Entity<int>
    {
        [Column("UserTreatmentTypeNo")]
        public int Id { get; set; }
        public string UserTreatmentName { get; set; }

        public override string ToString()
        {
            return UserTreatmentName;
        }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
