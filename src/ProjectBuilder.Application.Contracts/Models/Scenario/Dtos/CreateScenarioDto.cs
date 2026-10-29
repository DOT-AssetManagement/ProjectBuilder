using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.ParameterEntity.Dtos;
using ProjectBuilder.Models.ScenarioBudgetFlat.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.Scenario.Dtos
{
    public class CreateScenarioDto
    {
        public Guid? LibraryId { get; set; }

        [JsonPropertyName("scenarioname")]
        public string ScenarioName { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = "None";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public string LastRunBy { get; set; } = "None";
        public DateTime? LastRunAt { get; set; }
        public DateTime? LastStarted { get; set; }
        public DateTime? LastFinished { get; set; }
        public bool? Locked { get; set; }
        public string Notes { get; set; } = "None";
        public bool Stale { get; set; }
        public string SelectScen { get; set; }
        public int? FirstYear { get; set; }
        public int? LastYear { get; set; }
        public bool IsScenarioShared { get; set; }

        [DisplayName("ID")]
        public int ScenarioId { get; set; }

        public List<LibraryModel> newLibrarySend { get; set; }
        public List<ScenarioParamModel> parameters { get; set; }
        public List<budgetConstraintsModel> budgetConstraints { get; set; }
        public List<CandidatePoolDto> CandidatePools { get; set; } = new List<CandidatePoolDto>();
    }

    public class ScenarioParamModel
    {
        public string ParameterId { get; set; }
        public string DefaultValue { get; set; }
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
}
