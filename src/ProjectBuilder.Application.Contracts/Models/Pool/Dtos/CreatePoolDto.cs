namespace ProjectBuilder.Models.Pool.Dtos
{
    // Only user-supplied fields; status and lineage are set server-side
    public class CreatePoolDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSharedLibrary { get; set; }
    }
}
