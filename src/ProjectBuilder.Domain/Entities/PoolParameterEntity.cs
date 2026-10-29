using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBuilder.Entities
{
    public class PoolParameterEntity
    {
        public Guid Id { get; set; }
        public string ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
