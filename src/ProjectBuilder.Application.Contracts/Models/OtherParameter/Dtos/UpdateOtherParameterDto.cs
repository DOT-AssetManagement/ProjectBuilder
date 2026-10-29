using System;

namespace ProjectBuilder.Models.OtherParameter.Dtos
{
    public class UpdateOtherParameterDto
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public float? Val { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}
