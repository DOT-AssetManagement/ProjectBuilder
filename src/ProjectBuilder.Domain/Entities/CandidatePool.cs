using ProjectBuilder.Models.CandidatePool.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_lib_UserLibraries")]
    public class CandidatePool : Entity<Guid>
    {
        public Guid Id { get; set; }
        [Column("LibNo")]
        public int CandidatePoolNumber { get; set; }
        public Guid UserId { get; set; }
        [NotMapped]
        public string? Owner { get; set; }
        public string Name { get; set; }
        // Description and CreatedBy are nullable in tbl_lib_UserLibraries. With
        // <Nullable>enable</Nullable> a bare `string` makes EF treat the column as NOT NULL and
        // materialize it via an unguarded reader.GetString(), which throws SqlNullValueException
        // on the first NULL row.
        public string? Description { get; set; }
        [NotMapped]
        public int TreatmentsCount { get; set; }
        [NotMapped]
        public int BridgeTreatmentsCount { get; set; }
        [NotMapped]
        public int PavementTreatmentsCount { get; set; }
        [NotMapped]
        public int ScenarioCount { get; set; }
        [NotMapped]
        public DateTime? PopulatedAt { get; set; }
        [NotMapped]
        public string? Source { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsActive { get; set; }
        [Column("IsSharedLibrary")]
        public bool IsShared { get; set; }

        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
