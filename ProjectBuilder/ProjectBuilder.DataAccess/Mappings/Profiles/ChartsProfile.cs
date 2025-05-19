using AutoMapper;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class AllNeedsChartsProfile : Profile
    {
        public AllNeedsChartsProfile()
        {
            CreateMap<List<AllNeedsModel>, ChartsDataModel>().ConvertUsing((needs) => ConvertNeedsToChartData(needs));
            CreateMap<List<BridgeNeedsModel>, ChartsDataModel>().ConvertUsing((needs) => ConvertNeedsToChartData(needs));
            CreateMap<List<PavementNeedsModel>, ChartsDataModel>().ConvertUsing((needs) => ConvertNeedsToChartData(needs));
        }
        internal ChartsDataModel ConvertNeedsToChartData<T>(List<T> needsValues) where T : AllNeedsModel
        {
            if (needsValues is null || needsValues.Count <= 0)
            {
                return default;
            }
            var allneedsChartModel = new ChartsDataModel();
            var labels = new List<int?>();
            var interstateCost = new List<double?>();
            var nonInterstateCost = new List<double?>();
            var years = needsValues.Where(x => x.TreatmentYear != default).Select(x => x.TreatmentYear).Distinct();
            foreach (var year in years)
            {
                interstateCost = new List<double?>();
                nonInterstateCost = new List<double?>();
                labels.Add(year);
                var yearData = needsValues.Where(x => x.TreatmentYear == year);
                foreach (var data in yearData)
                {
                    if (!interstateCost.Contains(data.InterstateCost))
                        interstateCost.Add(data.InterstateCost);
                    if (!nonInterstateCost.Contains(data.NonInterstateCost))
                        nonInterstateCost.Add(data.NonInterstateCost);
                }
                allneedsChartModel.SeriesPoint.TryAdd($"{year.Value}", new()
                {
                    { "Interstate", interstateCost },
                    { "Non-Interstate", nonInterstateCost }
                });
            }
            allneedsChartModel.Labels = labels;

            return allneedsChartModel;
        }
    }
    public class PotentialBenefitsChartsProfile : Profile
    {
        public PotentialBenefitsChartsProfile()
        {
            CreateMap<List<AllPotentialBenefitsModel>, ChartsDataModel>().ConvertUsing((potentials) => ConvertPotentialBenefitsToChartData(potentials));
            CreateMap<List<BridgePotentialBenefitsModel>, ChartsDataModel>().ConvertUsing((potentials) => ConvertPotentialBenefitsToChartData(potentials));
            CreateMap<List<PavementPotentialBenefitsModel>, ChartsDataModel>().ConvertUsing((potentials) => ConvertPotentialBenefitsToChartData(potentials));
        }
        internal ChartsDataModel ConvertPotentialBenefitsToChartData<T>(List<T> potentialValues) where T : AllPotentialBenefitsModel
        {
            if (potentialValues is null || potentialValues.Count <= 0)
            {
                return default;
            }
            var chartData = new ChartsDataModel();
            var labels = new List<int?>();
            var interstateBenefits = new List<double?>();
            var nonInterstateBenefits = new List<double?>();
            var years = potentialValues.Where(x => x.TreatmentYear != default).Select(x => x.TreatmentYear).Distinct();
            foreach (var year in years)
            {
                interstateBenefits = new List<double?>();
                nonInterstateBenefits = new List<double?>();
                labels.Add(year);
                var yearData = potentialValues.Where(x => x.TreatmentYear == year);
                foreach (var data in yearData)
                {
                    if (!interstateBenefits.Contains(data.InterstateBenefit))
                        interstateBenefits.Add(data.InterstateBenefit);
                    if (!nonInterstateBenefits.Contains(data.NonInterstateBenefit))
                        nonInterstateBenefits.Add(data.NonInterstateBenefit);
                }
                chartData.SeriesPoint.TryAdd($"{year.Value}", new()
                {
                    { "Interstate", interstateBenefits },
                    { "Non-Interstate", nonInterstateBenefits }
                });

            }
            chartData.Labels = labels;
            return chartData;
        }
    }
    public class BudgetChartsProfile : Profile
    {
        public BudgetChartsProfile()
        {
            CreateMap<List<BudgetModel>, ChartsDataModel>().ConvertUsing((budgets) => ConvertBudgetToChartData(budgets));
        }
        internal ChartsDataModel ConvertBudgetToChartData(List<BudgetModel> budgets) 
        {
            var budgetChart = new ChartsDataModel();
            var labels = new List<int?>();
            var interstateBridgeBudget = new List<decimal?>();
            var nonInterstateBridgeBudget = new List<decimal?>();
            var interstatePavementBudget = new List<decimal?>();
            var nonInterstatePavementBudget = new List<decimal?>();
            var years = budgets.Where(x => x.ScenarioYear != default).Select(x => x.ScenarioYear).Distinct();
            foreach (var year in years)
            {
                interstateBridgeBudget = new List<decimal?>();
                nonInterstateBridgeBudget = new List<decimal?>();
                interstatePavementBudget = new List<decimal?>();
                nonInterstatePavementBudget = new List<decimal?>();
                labels.Add(year);
                var yearData = budgets.Where(x => x.ScenarioYear == year);
                foreach (var budget in yearData)
                {
                    if (!interstateBridgeBudget.Contains(budget.BridgeBudgetInterstate))
                        interstateBridgeBudget.Add(budget.BridgeBudgetInterstate);
                    if (!nonInterstateBridgeBudget.Contains(budget.BridgeBudgetNonInterstate))
                        nonInterstateBridgeBudget.Add(budget.BridgeBudgetNonInterstate);
                    if (!interstatePavementBudget.Contains(budget.PavementBudgetInterstate))
                        interstatePavementBudget.Add(budget.PavementBudgetInterstate);
                    if (!nonInterstatePavementBudget.Contains(budget.PavementBudgetNonInterstate))
                        nonInterstatePavementBudget.Add(budget.PavementBudgetNonInterstate);
                }
                budgetChart.SeriesPoint.TryAdd($"{year.Value}", new()
                {
                    { "Bridge Interstate", interstateBridgeBudget },
                    { "Bridge Non-Interstate", nonInterstateBridgeBudget },
                    { "Pavement Interstate", interstatePavementBudget },
                    { "Pavement Non-Interstate", nonInterstatePavementBudget }
                });
            }
            budgetChart.Labels = labels;
            return budgetChart;
        }

    }
    public class BudgetSpentChartsProfile : Profile
    {
        public BudgetSpentChartsProfile()
        {
            CreateMap<List<BudgetSpentModel>, ChartsDataModel>().ConvertUsing((budgetsSpent) => ConvertBudgetSpentToChartData(budgetsSpent));
        }
        internal ChartsDataModel ConvertBudgetSpentToChartData(List<BudgetSpentModel> budgetsSpent)
        {
            var budgetChart = new ChartsDataModel();
            var labels = new List<int?>();
            var interstateBridgeBudget = new List<double?>();
            var nonInterstateBridgeBudget = new List<double?>();
            var interstatePavementBudget = new List<double?>();
            var nonInterstatePavementBudget = new List<double?>();
            var years = budgetsSpent.Where(x => x.ScenarioYear != default).Select(x => x.ScenarioYear).Distinct();
            foreach (var year in years)
            {
                interstateBridgeBudget = new List<double?>();
                nonInterstateBridgeBudget = new List<double?>();
                interstatePavementBudget = new List<double?>();
                nonInterstatePavementBudget = new List<double?>();
                labels.Add(year);
                var yearData = budgetsSpent.Where(x => x.ScenarioYear == year);
                foreach (var budget in yearData)
                {
                    if (!interstateBridgeBudget.Contains(budget.BridgeBudgetInterstate))
                        interstateBridgeBudget.Add(budget.BridgeBudgetInterstate);
                    if (!nonInterstateBridgeBudget.Contains(budget.BridgeBudgetNonInterstate))
                        nonInterstateBridgeBudget.Add(budget.BridgeBudgetNonInterstate);
                    if (!interstatePavementBudget.Contains(budget.PavementBudgetInterstate))
                        interstatePavementBudget.Add(budget.PavementBudgetInterstate);
                    if (!nonInterstatePavementBudget.Contains(budget.PavementBudgetNonInterstate))
                        nonInterstatePavementBudget.Add(budget.PavementBudgetNonInterstate);
                }
                budgetChart.SeriesPoint.TryAdd($"{year.Value}", new()
                {
                    { "Bridge Interstate", interstateBridgeBudget },
                    { "Bridge Non-Interstate", nonInterstateBridgeBudget },
                    { "Pavement Interstate", interstatePavementBudget },
                    { "Pavement Non-Interstate", nonInterstatePavementBudget }
                });
            }
            budgetChart.Labels = labels;
            return budgetChart;
        }

    }
}
