using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_dss_Counties")]
    public class CountyEntity : Entity<int?>
    {
        public int? District { get; set; }
        [Column("County")]      
        public string CountyName { get; set; }
        [Column("Cnty")]
        public int? Id { get; set; }
        public string CountyFullName { get { return string.IsNullOrEmpty(CountyName) || Id is null  ? null :$"{Id}-{CountyName}"; } }
        public override string ToString()
        {
            return string.IsNullOrEmpty(CountyName) || Id is null ? null : $"{Id}-{CountyName}";
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
                Id = null;
                CountyName = null;
                return;
            }
            var values = fullName?.Split('-');
            if (values?.Length > 1)
            {
                Id = int.Parse(values[0]);
                CountyName = values[1];
            }
        }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
