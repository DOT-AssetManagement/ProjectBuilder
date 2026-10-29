using System;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Pool.Dtos
{
    // Only user-editable fields; audit and status fields are set server-side
    public class UpdatePoolDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSharedLibrary { get; set; }
    }
}
