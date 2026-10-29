using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.Counties.Dtos
{
    public class CreateCountyDto
    {
        public string CountyName { get; set; }

        public int? District { get; set; }
    }
}
