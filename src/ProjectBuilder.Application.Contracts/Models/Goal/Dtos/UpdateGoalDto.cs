using System;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Goal.Dtos
{
    public class UpdateGoalDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte? Status { get; set; }
    }
}
