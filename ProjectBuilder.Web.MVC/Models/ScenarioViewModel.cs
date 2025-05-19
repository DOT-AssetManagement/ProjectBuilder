using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618
#pragma warning disable CS8600

namespace ProjectBuilder.Web.MVC.Models
{
    public class ScenarioViewModel
    {
        public IEnumerable<ScenarioModel> Scenarios { get; set; } = Enumerable.Empty<ScenarioModel>();
        [Required,BindProperty(Name = "minyear")]
        public int? FirstYear { get; set; }

        [Required,BindProperty(Name = "maxyear")]
        public int? LastYear { get; set; }

        [DisplayName("Scenario Name")]
        [Required,BindProperty(Name ="scenarioname")]
        public string ScenarioName { get; set; }
        [DisplayName("ID")]
        public int ScenarioId { get; set; }
        public Guid? LibraryId { get; set; }
        public List<ParamModel> parameters { get; set; }
        public List<budgetConstraintsModel> budgetConstraints { get; set; }
        public List<LibraryModel> newLibrarySend { get; set; }
		public bool Stale { get; set; }


		public string CandidatePool { get; set; }
        public string SelectScen { get; set; }
        public string CreatedBy { get; set; }
        public string NewLibName { get; set; }
        // lib

        public string librarydescription { get; set; }

        public bool IsEmptyLibrary { get; set; }
        public bool IsShared { get; set; }


        public string SearchString { get; set; }
    }

}
public class ParamModel
{
    public string ParameterId { get; set; }
    public string DefaultValue { get; set; }
}
public class budgetConstraintsModel
{
    public int YearofWork { get; set; }
    public int District { get; set; }
    public decimal? bridgeInterstateBudget { get; set; }
    public decimal? bridgeNonInterstateBudget { get; set; }
    public decimal? pavementInterstateBudget { get; set; }
    public decimal? pavementNonInterstateBudget { get; set; }
}
public class LibraryModel
{
    public string NewLibName { get; set; }
    public string librarydescription { get; set; }
    public bool IsEmptyLibrary { get; set; }
    public bool IsShared { get; set; }
    public string? SelectedAsset { get; set; }
    public int? SelectedDistrict { get; set; }
    public short? SelectedCounty { get; set; }
    public int? SelectedRoute { get; set; }
    public int? LibMinYear { get; set; }
    public int? LibMaxYear { get; set; }
}
