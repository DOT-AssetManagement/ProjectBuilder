using ProjectBuilder.Core.Models;

namespace ProjectBuilder.Web.MVC.Models
{
    public class ParameterViewModel
    {
        public List<ParameterModel> parameters = new List<ParameterModel>();
        public string ParameterId { get; set; }
        public string Parmfamily { get; set; }
        public string ParmName { get; set; }
        public string ParmDescription { get; set; }
        public double? DefaultValue { get; set; }
    }
}
