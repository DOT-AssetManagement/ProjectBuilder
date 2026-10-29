using System;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Goal.Dtos
{
    public class GoalDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte? Status { get; set; }
        public Guid? CreatorId { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
