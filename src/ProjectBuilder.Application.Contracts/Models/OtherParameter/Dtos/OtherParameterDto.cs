using System;

namespace ProjectBuilder.Models.OtherParameter.Dtos
{
    public class OtherParameterDto
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public float? Val { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
