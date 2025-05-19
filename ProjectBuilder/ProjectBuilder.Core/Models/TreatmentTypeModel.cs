using System.Diagnostics.CodeAnalysis;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class TreatmentTypeModel
    {
        public string TreatmentName { get; set; }
        public int? MoveEarlier { get; set; }
        public int? MoveLater { get; set; }
        public string AssetType { get; set; }
    }
}
