using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8602
#pragma warning disable CS8629

namespace PBLogic
{
    public class TimewiseConstrainedProject
    {
        // public Dictionary<int, Dictionary<string, BudgetPrefix>> CostDictionary = new Dictionary<int, Dictionary<string, BudgetPrefix>>();
        public Dictionary<long, NarrowLibraryTreatment> Treatments = new Dictionary<long, NarrowLibraryTreatment>();

        //public double TotalCost = 0;

        public int MinYear = 0;
        public int MaxYear = 0;

        public int? PriorityOrder = null;
        public bool HasCommitted = false;

        public bool IsUserCreated = false;
        public bool IsSelected = false;
        public bool DoNothingAlternativeExplicitlySelected = false;

        public int PreferredStartingYear = 0;
        public int PreferredEndingYear = 0;
        public int SelectedFirstYear = 0;

        private int MinLeftShift = 0;
        private int MaxRightShift = 0;

        public double UtilityPreferred = 0;

        public SortedDictionary<int, ProjectAlternative> Alternatives = new SortedDictionary<int, ProjectAlternative>();

        public void PopulateProjectDetail(Dictionary<string, int> budgetAttributePairs, int scenarioFirstYear, int scenarioLastYear,
                                            int maxLeftShift, int maxRightShift)
        {
            foreach (var treatEntry in Treatments)
            {
                NarrowLibraryTreatment treatment = treatEntry.Value;
                HasCommitted = HasCommitted || treatment.IsCommitted;
                if (PriorityOrder == null || treatment.PriorityOrder < PriorityOrder)
                {
                    PriorityOrder = treatment.PriorityOrder;
                }
            }

            MinLeftShift = MaxRightShift = 0;

            if (!HasCommitted)
            {
                if (PreferredStartingYear > scenarioFirstYear)
                {
                    MinLeftShift = scenarioFirstYear - PreferredStartingYear;   // Should be a negative number
                    if (Math.Abs(MinLeftShift) > maxLeftShift)
                    {
                        MinLeftShift = -maxLeftShift;
                    }
                }

                if (PreferredEndingYear < scenarioLastYear)
                {
                    MaxRightShift = scenarioLastYear - PreferredEndingYear;     // Should be a positive number
                    if (MaxRightShift > maxRightShift)
                    {
                        MaxRightShift = maxRightShift;
                    }
                }
            }

            // Clamp shift range to feasible window (MinYear/MaxYear intersection)
            if (MinYear > 0 && MaxYear > 0)
            {
                int leftBound = MinYear - PreferredStartingYear;
                int rightBound = MaxYear - PreferredEndingYear;
                MinLeftShift = Math.Max(MinLeftShift, leftBound);
                MaxRightShift = Math.Min(MaxRightShift, rightBound);

                // If preferred year span exceeds feasible window,
                // rebase preferred years to the feasible window
                if (MinLeftShift > MaxRightShift)
                {
                    PreferredStartingYear = MinYear;
                    PreferredEndingYear = MaxYear;
                    MinLeftShift = 0;
                    MaxRightShift = Math.Min(MaxYear - MinYear, maxRightShift);
                }
            }

            CreateAlternativeSkeletons(scenarioFirstYear, scenarioLastYear);
            PopulateAlternativeDetails(budgetAttributePairs);
        }

        private void CreateAlternativeSkeletons(int scenarioFirstYear, int scenarioLastYear)
        {
            Alternatives.Clear();

            int j = 0;
            for (int i = MinLeftShift; i <= MaxRightShift; i++)
            {
                ProjectAlternative alternative = new ProjectAlternative()
                {
                    AltNo = ++j,
                    ShiftFromPreferred = i,
                    FirstYearWork = PreferredStartingYear + i,
                    IsPreferred = (i == 0),
                    WorkingUtility = GetWorkingUtility(UtilityPreferred, PreferredStartingYear, PreferredStartingYear + i, scenarioFirstYear, scenarioLastYear)
                };

                Alternatives.Add(j, alternative);
            }
        }

        /// <summary>
        /// This function implements the formula that was used in the [dbo].[fn_pb_GetUtilityLinear] scalar function
        /// </summary>
        /// <param name="utilityPreferred"></param>
        /// <param name="preferredYear"></param>
        /// <param name="workingYear"></param>
        /// <param name="firstScenarioYear"></param>
        /// <param name="lastScenarioYear"></param>
        /// <returns></returns>
        private static double GetWorkingUtility(double utilityPreferred, int preferredYear, int workingYear, int firstScenarioYear, int lastScenarioYear)
        {
            if (workingYear == preferredYear || lastScenarioYear <= firstScenarioYear)
            {
                return utilityPreferred;
            }

            /* Comment is from [dbo].[fn_pb_GetUtilityLinear]
            --Prime numbers in denominator
            --are used to elimnate possibility of having
            -- identical utilities
            */

            double utility = 0;
            if (workingYear > preferredYear)
            {
                utility = utilityPreferred - 6.0 / 7.0 * (workingYear - preferredYear) / (lastScenarioYear - firstScenarioYear);
            }
            else
            {
                utility = utilityPreferred - 4.0 / 5.0 * (preferredYear - workingYear) / (lastScenarioYear - firstScenarioYear);
            }

            return utility;
        }

        public void PopulateAlternativeDetails(Dictionary<string, int> budgetAttributePairs)
        {
            foreach (var alt in Alternatives)
            {
                int altNo = alt.Key;
                ProjectAlternative alternative = alt.Value;

                foreach (var trt in Treatments)
                {
                    NarrowLibraryTreatment treatment = trt.Value;

                    double cost = treatment.Cost.Value;
                    int yr = treatment.PreferredYear.Value + alternative.ShiftFromPreferred;

#if NO_BUDGET_WILDCARDS
                    for (int i = 0; i < 2; i++)
                    {
                        SortedDictionary<string, double> Costs = alternative.Costs[i];
                        string key;

                        if (i == 0)
                        {
                            key = $"Y{yr}D{treatment.District}I{treatment.IsInterstate}T{treatment.AssetType.ToUpper()}";
                        }
                        else
                        {
                            key = $"Y{yr}D{treatment.District}I{treatment.IsInterstate}TR";
                        }



                        if (Costs.ContainsKey(key))
                        {
                            Costs[key] += treatment.Cost.Value;
                        }
                        else
                        {
                            Costs.Add(key, treatment.Cost.Value);
                        }

                        foreach (var cargoAttr in treatment.Cargo)
                        {
                            if (cargoAttr.Value != null && cargoAttr.Value.TextValue != null)
                            {
                                string attrName = cargoAttr.Key.ToUpper();
                                string attrValue = cargoAttr.Value.TextValue.ToUpper();
                                string suffix =  "[" + attrName + "].[" + attrValue + "]";
                                if (budgetAttributePairs.ContainsKey(suffix.ToUpper()))
                                {
                                    string keyPlus = $"{key}G{budgetAttributePairs[suffix.ToUpper()]}";

                                    if (Costs.ContainsKey(keyPlus))
                                    {
                                        Costs[keyPlus] += treatment.Cost.Value;
                                    }
                                    else
                                    {
                                        Costs.Add(keyPlus, treatment.Cost.Value);
                                    }

                                }
                            }
                        }
                    }
#else
                    // Wildcards

#pragma warning disable CS8600

                    for (int i = 0; i < 2; i++)
                    {
                        SortedDictionary<string, double> Costs = alternative.Costs[i];

                        for (int jD = 0; jD < 2; jD++)
                        {
                            string district = jD == 0 ? treatment.District.ToString() : "*";

                            for (int jI = 0; jI < 2; jI++)
                            {
                                string isInterstate = jI == 0 ? treatment.IsInterstate.ToString() : "*";

                                int nA = (i == 0) ? 2 : 1;      // If asset budggtes can be mixed then there is no need for the wildcard for assets

                                for (int jA = 0; jA < nA; jA++)
                                {
                                    string assetType = jA == 0 ? treatment.AssetType.ToString() : "*";
                                    string key;

                                    if (i == 0)
                                    {
                                        key = $"Y{yr}D{district}I{isInterstate}T{assetType.ToUpper()}";
                                    }
                                    else
                                    {
                                        key = $"Y{yr}D{district}I{isInterstate}TR";
                                    }



                                    if (Costs.ContainsKey(key))
                                    {
                                        Costs[key] += treatment.Cost.Value;
                                    }
                                    else
                                    {
                                        Costs.Add(key, treatment.Cost.Value);
                                    }

                                    foreach (var cargoAttr in treatment.Cargo)
                                    {
                                        if (cargoAttr.Value != null && cargoAttr.Value.TextValue != null)
                                        {
                                            string attrName = cargoAttr.Key.ToUpper();
                                            string attrValue = cargoAttr.Value.TextValue.ToUpper();
                                            string suffix = "[" + attrName + "].[" + attrValue + "]";
                                            if (budgetAttributePairs.ContainsKey(suffix.ToUpper()))
                                            {
                                                string keyPlus = $"{key}G{budgetAttributePairs[suffix.ToUpper()]}";

                                                if (Costs.ContainsKey(keyPlus))
                                                {
                                                    Costs[keyPlus] += treatment.Cost.Value;
                                                }
                                                else
                                                {
                                                    Costs.Add(keyPlus, treatment.Cost.Value);
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

#endif
                }
            }
        }
    }
}
