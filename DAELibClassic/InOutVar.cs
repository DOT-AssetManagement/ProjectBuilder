using Newtonsoft.Json;

namespace DEALib
{
    /// <summary>
    /// Variable that can be used as either an input (Cost) or output (Benefit)
    /// </summary>
    public class InOutVar
    {
        [JsonProperty (PropertyName = "Type", Order = 10)]
        public string VarType;      // "I" - input, "O" - output

        [JsonProperty(PropertyName = "Scale", Order = 20, NullValueHandling = NullValueHandling.Ignore)]
        public double Scale = 1.0;  // Multiplier to apply to the entry inn the CSV file
             
        [JsonIgnore]
        public int CsvPosIx = 0;    // 0-based index of the position in the CSV file

        [JsonProperty(PropertyName = "Index", Order = 30, NullValueHandling = NullValueHandling.Ignore)]
        public int? Index = null;       // Index in the V or U array in the Project obect

        [JsonProperty(PropertyName = "VarIndex", Order = 40, NullValueHandling = NullValueHandling.Ignore)]
        public int? VarIndex = null;   // Index in the 1-dimensional array of LP variables

        [JsonProperty(PropertyName = "MinWeightApplied", Order = 22, NullValueHandling = NullValueHandling.Ignore)]
        public double? MinWeightApplied = null;

        [JsonProperty(PropertyName = "MaxWeightApplied", Order = 24, NullValueHandling = NullValueHandling.Ignore)]
        public double? MaxWeightApplied = null;

        [JsonProperty(PropertyName = "MinWeightActual", Order = 42, NullValueHandling = NullValueHandling.Ignore)]
        public double? MinWeightActual = null;

        [JsonProperty(PropertyName = "MaxWeightActual", Order = 44, NullValueHandling = NullValueHandling.Ignore)]
        public double? MaxWeightActual = null;


    }
}
