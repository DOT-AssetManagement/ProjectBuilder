namespace ProjectBuilder.Models
{
    // One row of the Parameters page "Treatment Shift" tab.
    public class TreatmentShiftRow
    {
        public string Treatment { get; set; }
        public int MaxAccelerate { get; set; } = 5;
        public int MaxDelay { get; set; }
    }
}
