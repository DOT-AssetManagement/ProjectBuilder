using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core.Models
{
    public class AraEnumerationsModel
    {
        public string EnumFamily { get; set; }
        public string EnumName { get; set; }
        public int EnumInt { get; set; }
    }
}
