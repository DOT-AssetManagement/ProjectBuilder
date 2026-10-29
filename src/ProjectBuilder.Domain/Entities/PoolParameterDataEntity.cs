using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBuilder.Entities
{
    public class PoolParameterDataEntity
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        public Guid? DataSectionId { get; set; }
        public Guid? ItemId { get; set; }
        public float? ItemValue { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
