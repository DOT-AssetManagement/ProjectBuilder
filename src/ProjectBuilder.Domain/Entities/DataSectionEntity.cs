using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProjectBuilder.Entities
{
    [Table("tbl_dss_DataSections")]
    public class DataSectionEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Notes { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ShortName { get; set; }
        public byte? ExportOrdNum { get; set; }
    }
}
