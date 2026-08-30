using ProgressHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Models
{
    public class ClientAnalyticsSummary
    {
        public double? StartWeight { get; init; }
        public DateOnly? StartDate { get; init; }
        public double? CurrentWeight { get; init; }
        public double? SevenDayAverageWeight { get; init; }

        public DateOnly? PeriodStartDate { get; init; }
        public DateOnly? PeriodEndDate { get; init; }

        // Období a průměr za dané období
        public AnalyticsTimeWindow SelectedWindow { get; init; }
        public double? PeriodAverageWeight { get; init; }
        public int PeriodLogsCount { get; init; }

        // Porovnání: Průměr za zvolené období vs. Výchozí váha (Start Weight)
        public double? WeightDeltaVsStart { get; init; }
        public string DeltaContextText { get; init; } = "Awaiting data";
        public string DeltaColorStatus { get; init; } = "muted";

        // Streak
        public int CurrentStreak { get; init; }
        public int TotalLogsCount { get; init; }
    }
}
