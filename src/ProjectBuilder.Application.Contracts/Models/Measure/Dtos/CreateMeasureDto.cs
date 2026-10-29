namespace ProjectBuilder.Models.Measure.Dtos
{
    public class CreateMeasureDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte? Status { get; set; }
    }
}
