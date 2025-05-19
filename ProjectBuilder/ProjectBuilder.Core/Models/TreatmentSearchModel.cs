using System;
using System.Collections.Generic;

namespace ProjectBuilder.Core
{
    public class TreatmentSearchModel
    {
        public string AssetType { get; set; }
        public byte? Cnty { get; set; }
        public byte? District { get; set; }
        public Guid? LibraryId { get; set; }
        public int? Route { get; set; }
        public int? ProjectId { get; set; }
        public int? BridgeToSection { get; set; }
        public string Category { get; set; }
        public int? Year { get; set; }
        public int? ScenarioId { get; set; }
        public int? FromSection { get; set; }
        public int? ToSection { get; set; }
        public bool? Direction { get; set; }
        public List<UserTreatmentModel> Items { get; set; }
        public int TotalCount { get; set; }

    }
}
