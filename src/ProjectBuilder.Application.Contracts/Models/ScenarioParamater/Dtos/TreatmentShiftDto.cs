namespace ProjectBuilder.Models.ScenarioParamater.Dtos
{
    public class TreatmentShiftDto
    {
        public string Treatment { get; set; }
        public int MaxAccelerate { get; set; } = 5;
        public int MaxDelay { get; set; }
    }
}
