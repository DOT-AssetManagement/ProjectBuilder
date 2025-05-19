using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core.Models
{
    public class MapsResultModel
    {
        public string Error { get; set; }
        public string Result { get; set; }
        public bool HasError { get; set; }
        public MapsResultModel(string error, string result, bool hasError)
        {
            Error = error;
            Result = result;
            HasError = hasError;
        }
    }
}
