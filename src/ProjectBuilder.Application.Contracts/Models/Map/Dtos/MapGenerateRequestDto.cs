using System.ComponentModel.DataAnnotations;

namespace ProjectBuilder.Models.Map.Dtos
{
    public class MapGenerateRequestDto
    {
        [Required]
        public int ScenId { get; set; }
        public int? District { get; set; }
        public int? Cnty { get; set; }
        public int? Route { get; set; }
        public string? TreatmentType { get; set; }
    }
}
