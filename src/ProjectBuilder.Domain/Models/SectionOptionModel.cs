namespace ProjectBuilder.Models
{
    // One pickable "Section" value for the Map treatment editor's location cascade,
    // carrying the From/To range so the dropdown can render "10 - 10" like the
    // project treatments table does, instead of the bare Section string.
    public class SectionOptionModel
    {
        public string Section { get; set; }
        public int? FromSection { get; set; }
        public int? ToSection { get; set; }
    }
}
