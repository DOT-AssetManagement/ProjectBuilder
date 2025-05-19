using ProjectBuilder.Core.Models;

namespace ProjectBuilder.Web.MVC.Models
{
    public class TreatmentParameterViewModel
    {
        public List<TreatmentParameterModel> TreatmentParameters { get; set; } = new List<TreatmentParameterModel>();
        public int TreatmentParameterId { get; set; }
        public double? UserTreatmentBenefitWeight { get; set; }
    }
}
