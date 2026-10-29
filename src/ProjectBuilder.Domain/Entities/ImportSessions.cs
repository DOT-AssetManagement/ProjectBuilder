using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_pb_ImportSessions")]
    public class ImportSessions:Entity<Guid>
    {
        public int NoId { get; set; }
        [Column("Id")]
        public Guid Id { get; set; }
        public string? ImportSource { get; set; }
        public string? DataSourceType { get; set; }
        public string? DataSourceName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CompletedStatus { get; set; }
        public string? Notes { get; set; }
        public Guid LibraryId { get; set; }

        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
