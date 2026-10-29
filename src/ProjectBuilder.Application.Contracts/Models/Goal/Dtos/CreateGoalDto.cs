namespace ProjectBuilder.Models.Goal.Dtos
{
    public class CreateGoalDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte? Status { get; set; }
    }
}
