using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    public class OtherParameterEntity : Entity<string>
    {
        public OtherParameterEntity() { }

        // Id is the Code column; the base setter is protected, so the key must come through here
        public OtherParameterEntity(string code)
        {
            Id = code;
        }

        public string? Description { get; set; }
        public float? Val { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
