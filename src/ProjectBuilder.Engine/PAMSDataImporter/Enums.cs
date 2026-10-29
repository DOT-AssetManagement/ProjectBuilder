namespace PAMSDataImporter
{
    public enum BudgetUsageStatus
    {
        /// <summary>
        ///     Indicates a budget that paid the entire remaining cost of the treatment.
        /// </summary>
        CostCoveredInFull,

        /// <summary>
        ///     Indicates a budget that paid some of the remaining cost of the treatment. Only
        ///     possible when <see cref="AnalysisMethod.ShouldUseExtraFundsAcrossBudgets"/> is true.
        /// </summary>
        CostCoveredInPart,

        /// <summary>
        ///     Indicates a budget that was insufficient to pay the remaining cost of the treatment.
        ///     When <see cref="AnalysisMethod.ShouldUseExtraFundsAcrossBudgets"/> is true, this
        ///     status further indicates that the budget had zero available funds.
        /// </summary>
        CostNotCovered,

        /// <summary>
        ///     Indicates a budget with one or more user-defined conditions, none of which were met.
        /// </summary>
        ConditionNotMet,

        /// <summary>
        ///     Indicates a budget that was usable, but other budgets before this one in the
        ///     scenario's budget order were sufficient to pay for the treatment.
        /// </summary>
        NotNeeded,

        /// <summary>
        ///     Indicates a budget excluded by the treatment's settings.
        /// </summary>
        NotUsable,
    }

    public enum TreatmentStatus
    {
        /// <summary>
        ///     Indicates the existence of incomplete logic in the analysis engine.
        /// </summary>
        Undefined,

        /// <summary>
        ///     Indicates a treatment that has been fully applied.
        /// </summary>
        Applied,

        /// <summary>
        ///     Indicates a treatment that has been partially applied (during the leading years of a
        ///     multi-year treatment period).
        /// </summary>
        Progressed,
    }

    public enum TreatmentRejectionReason
    {
        /// <summary>
        ///     Indicates the existence of incomplete logic in the analysis engine.
        /// </summary>
        Undefined,

        /// <summary>
        ///     Indicates a treatment rejected due to a previously selected treatment's "shadow"
        ///     preventing the selection of <em>any</em> treatment.
        /// </summary>
        WithinShadowForAnyTreatment,

        /// <summary>
        ///     Indicates a treatment rejected due to a previously selected treatment's "shadow"
        ///     preventing the selection of <em>the same</em> treatment.
        /// </summary>
        WithinShadowForSameTreatment,

        /// <summary>
        ///     Indicates a treatment whose user-defined feasibility criteria were not satisfied.
        /// </summary>
        NotFeasible,

        /// <summary>
        ///     Indicates a treatment rejected due to a user-defined treatment "supersession".
        /// </summary>
        Superseded,

        /// <summary>
        ///     Indicates a treatment whose cost was negative, zero, or an astronomically large
        ///     positive amount (larger than <see cref="decimal.MaxValue"/>).
        /// </summary>
        InvalidCost,

        /// <summary>
        ///     Indicates a treatment whose cost was less than the scenario's minimum project cost
        ///     limit. See <see cref="InvestmentPlan.MinimumProjectCostLimit"/>.
        /// </summary>
        CostIsBelowMinimumProjectCostLimit,
    }

    public enum TreatmentCause
    {
        /// <summary>
        ///     Indicates the existence of incomplete logic in the analysis engine.
        /// </summary>
        Undefined,

        /// <summary>
        ///     Indicates a treatment was not selected by the analysis engine.
        /// </summary>
        NoSelection,

        /// <summary>
        ///     Indicates a treatment was selected by the analysis engine.
        /// </summary>
        SelectedTreatment,

        /// <summary>
        ///     Indicates a treatment was scheduled by a previous treatment selection.
        /// </summary>
        ScheduledTreatment,

        /// <summary>
        ///     Indicates a treatment explicitly pre-selected in the input to the analysis engine.
        /// </summary>
        CommittedProject,

        /// <summary>
        ///     Indicates a non-initial year of a multi-year treatment. (Initial year uses <see cref="SelectedTreatment"/>.)
        /// </summary>
        CashFlowProject,
    }

    public enum ReasonAgainstCashFlow
    {
        /// <summary>
        ///     Indicates the existence of incomplete logic in the analysis engine.
        /// </summary>
        Undefined,

        /// <summary>
        ///     Indicates a cash flow rule that was after a selected cash flow rule in the
        ///     scenario's cash flow rule order.
        /// </summary>
        NotNeeded,

        /// <summary>
        ///     Indicates a cash flow rule that lacked a distribution rule applicable to the
        ///     treatment cost.
        /// </summary>
        NoApplicableDistributionRule,

        /// <summary>
        ///     Indicates a cash flow rule whose applicable distribution rule is for only one year
        ///     and thus is unnecessary.
        /// </summary>
        ApplicableDistributionRuleIsForOnlyOneYear,

        /// <summary>
        ///     Indicates a cash flow rule whose applicable distribution rule would have extended
        ///     the cash flow beyond the end of the analysis period.
        /// </summary>
        LastYearOfCashFlowIsOutsideOfAnalysisPeriod,

        /// <summary>
        ///     Indicates a cash flow rule whose applicable distribution rule would extend the cash
        ///     flow into a year blocked by other work, e.g. a previously scheduled treatment.
        /// </summary>
        FutureEventScheduleIsBlocked,

        /// <summary>
        ///     Indicates a cash flow rule whose applicable distribution rule could not be applied
        ///     due to lack of funding in the distribution period.
        /// </summary>
        FundingIsNotAvailable,

        /// <summary>
        ///     Indicates a selected cash flow rule.
        /// </summary>
        None,
    }
}