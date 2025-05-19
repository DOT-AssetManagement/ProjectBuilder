using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_pb_Counties")]
    public class CountyEntity : IEntity<int?>
    {
        public int? District { get; set; }
        [Column("County")]      
        public string CountyName { get; set; }
        [Column("Cnty")]
        public int? EntityId { get; set; }
        public string CountyFullName { get { return string.IsNullOrEmpty(CountyName) || EntityId is null  ? null :$"{EntityId}-{CountyName}"; } }
        public override string ToString()
        {
            return string.IsNullOrEmpty(CountyName) || EntityId is null ? null : $"{EntityId}-{CountyName}";
        }
        public CountyEntity()
        {

        }
        public CountyEntity(string fullName)
        {
           SetCountyName(fullName);
        }
        private void SetCountyName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                EntityId = null;
                CountyName = null;
                return;
            }
            var values = fullName?.Split('-');
            if (values?.Length > 1)
            {
                EntityId = int.Parse(values[0]);
                CountyName = values[1];
            }
        }
    }
}
