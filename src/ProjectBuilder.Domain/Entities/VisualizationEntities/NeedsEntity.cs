using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{  
    public class AllNeedsEntity : Entity<int?>
    {
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
  
        [Column("TreatmentYear")]
        public int? Id { get; set; }
        public double? InterstateCost { get; set; }
        public double? NonInterstateCost { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
    public class BridgeNeedsEntity : Entity<int?>
    {
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
     
        [Column("TreatmentYear")]
        public int? Id { get; set; }
        public double? InterstateCost { get; set; }
        public double? NonInterstateCost { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
    public class PavementNeedsEntity : Entity<int?>
    {
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }  
        [Column("TreatmentYear")]
        public int? Id { get; set; }
        public double? InterstateCost { get; set; }
        public double? NonInterstateCost { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
    public class AllPotentialBenefitEntity : Entity<int?>
    {
        [Column("TreatYear")]
        public int? Id { get; set; }
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
        public double? InterBenefit { get; set; }
        public double? NonInterBenefit { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
    public class BridgePotentialBenefitEntity : Entity<int?>
    {
        [Column("TreatYear")]
        public int? Id { get; set; }
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
        public double? InterBenefit { get; set; }
        public double? NonInterBenefit { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
    public class PavementPotentialBenefitEntity : Entity<int?>
    {
        [Column("TreatYear")]
        public int? Id { get; set; }
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
        public double? InterBenefit { get; set; }
        public double? NonInterBenefit { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
