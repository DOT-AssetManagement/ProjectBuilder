using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{  
    public class AllNeedsEntity : IEntity<int?>
    {
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
  
        [Column("TreatmentYear")]
        public int? EntityId { get; set; }
        public double? InterstateCost { get; set; }
        public double? NonInterstateCost { get; set; }
    }
    public class BridgeNeedsEntity : IEntity<int?>
    {
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
     
        [Column("TreatmentYear")]
        public int? EntityId { get; set; }
        public double? InterstateCost { get; set; }
        public double? NonInterstateCost { get; set; }
    }
    public class PavementNeedsEntity : IEntity<int?>
    {
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }  
        [Column("TreatmentYear")]
        public int? EntityId { get; set; }
        public double? InterstateCost { get; set; }
        public double? NonInterstateCost { get; set; }
    }
    public class AllPotentialBenefitEntity : IEntity<int?>
    {
        [Column("TreatYear")]
        public int? EntityId { get; set; }
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
        public double? InterBenefit { get; set; }
        public double? NonInterBenefit { get; set; }
    }
    public class BridgePotentialBenefitEntity : IEntity<int?>
    {
        [Column("TreatYear")]
        public int? EntityId { get; set; }
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
        public double? InterBenefit { get; set; }
        public double? NonInterBenefit { get; set; }
    }
    public class PavementPotentialBenefitEntity : IEntity<int?>
    {
        [Column("TreatYear")]
        public int? EntityId { get; set; }
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
        public double? InterBenefit { get; set; }
        public double? NonInterBenefit { get; set; }
    }
}
