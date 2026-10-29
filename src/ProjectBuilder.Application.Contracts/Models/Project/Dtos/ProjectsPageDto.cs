using System.Collections.Generic;

namespace ProjectBuilder.Models.Project.Dtos
{
    public class ProjectsPageDto
    {
        public List<ProjectDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
