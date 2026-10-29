using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.AraEnumerations.Dtos
{
    public class CreateAraEnumerationDto
    {
        public string EnumFamily { get; set; }

        public string EnumName { get; set; }

        public int EnumInt { get; set; }
    }

}
