using System;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Measure.Dtos
{
    public class UpdateMeasureDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte? Status { get; set; }
    }
}
