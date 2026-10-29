using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.OutPut.Dtos
{
    public class MapsResultDto
    {
        public string Error { get; set; }
        public string Result { get; set; }
        public bool HasError { get; set; }
        public MapsResultDto(string error, string result, bool hasError)
        {
            Error = error;
            Result = result;
            HasError = hasError;
        }
    }
}
