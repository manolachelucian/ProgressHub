using ProgressHub.Core.Enums;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Services
{
    public class ClientAnalyticsService : IClientAnalyticsService
    {
        private readonly TimeProvider _timeProvider;

        public ClientAnalyticsService(TimeProvider? timeProvider = null)
        {
            _timeProvider = timeProvider ?? TimeProvider.System;
        }
    
        public ClientAnalyticsSummary CalculateSummary(User client, AnalyticsTimeWindow window = AnalyticsTimeWindow.Days14)
        {
            ArgumentNullException.ThrowIfNull(client);

            var sortedLogs = client.DailyLogs.OrderBy(l => l.Date).ToList();
            var totalCount = sortedLogs.Count;

            if (totalCount == 0)
            {
                return new ClientAnalyticsSummary
                {
                    SelectedWindow = window,
                    TotalLogsCount = 0,
                    DeltaContextText = "No logs recorded",
                    DeltaColorStatus = "muted"
                };
            }

            var firstLog = sortedLogs.First();
            var latestLog = sortedLogs.Last();

            // 7denní klouzavý průměr k poslednímu záznamu
            var last7DaysLogs = client.DailyLogs
                .Where(l => l.Date >= latestLog.Date.AddDays(-6) && l.Date <= latestLog.Date)
                .ToList();

            double? sevenDayAvg = last7DaysLogs.Count > 0
                ? Math.Round(last7DaysLogs.Average(l => l.Weight), 1)
                : null;

            // Výpočet změny ZA dané vybrané období (14d, 30d, 90d, 180d, 365d, All-time)
            DateOnly windowStartDate = window switch
            {
                AnalyticsTimeWindow.AllTime => firstLog.Date,
                _ => latestLog.Date.AddDays(-(int)window)
            };

            // Záznamy spadající do vybraného období
            var periodLogs = sortedLogs
                .Where(l => l.Date >= windowStartDate && l.Date <= latestLog.Date)
                .ToList();

            // Výchozí váha PRO DANÉ OBDOBÍ (nejstarší log v daném okně nebo nejbližší předchozí)
            var periodStartLog = sortedLogs
                .Where(l => l.Date <= windowStartDate)
                .LastOrDefault() ?? periodLogs.FirstOrDefault();

            double? periodAverageWeight = periodLogs.Count > 0
                ? Math.Round(periodLogs.Average(l => l.Weight), 1)
                : null;

            // Delta za dané období = Aktuální váha (nebo průměr) - Váha na začátku okna
            double? periodDelta = (periodStartLog is not null && latestLog.Id != periodStartLog.Id)
                ? Math.Round(latestLog.Weight - periodStartLog.Weight, 1)
                : (window == AnalyticsTimeWindow.AllTime && sortedLogs.Count > 1
                    ? Math.Round(latestLog.Weight - firstLog.Weight, 1)
                    : null);

            var (contextText, colorStatus) = EvaluateDelta(client.FitnessGoal, periodDelta, periodLogs.Count);

            return new ClientAnalyticsSummary
            {
                StartWeight = firstLog.Weight,
                StartDate = firstLog.Date,
                CurrentWeight = latestLog.Weight,
                SevenDayAverageWeight = sevenDayAvg,
                SelectedWindow = window,
                PeriodStartDate = periodStartLog?.Date ?? windowStartDate,
                PeriodEndDate = latestLog.Date,
                PeriodAverageWeight = periodAverageWeight,
                PeriodLogsCount = periodLogs.Count,
                WeightDeltaVsStart = periodDelta,
                DeltaContextText = contextText,
                DeltaColorStatus = colorStatus,
                CurrentStreak = CalculateStreak(client.DailyLogs),
                TotalLogsCount = totalCount
            };
        }

        private static (string text, string color) EvaluateDelta(FitnessGoal goal, double? delta, int logsInPeriod)
        {
            if (delta is null || logsInPeriod == 0)
            {
                return ("No data in period", "muted");
            }

            if (Math.Abs(delta.Value) < 0.01)
            {
                return ("No change vs start", "muted");
            }

            return goal switch
            {
                FitnessGoal.WeightLoss => delta < 0
                    ? ($"Loss vs start ({delta:0.0} kg)", "success")
                    : ($"Gain vs start (+{delta:0.0} kg)", "danger"),
                FitnessGoal.MuscleGain => delta > 0
                    ? ($"Gain vs start (+{delta:0.0} kg)", "success")
                    : ($"Loss vs start ({delta:0.0} kg)", "warning"),
                _ => ("Progress tracked", "primary")
            };
        }

        private int CalculateStreak(IEnumerable<DailyLog> logs)
        {
            var dates = logs
                .Select(l => l.Date)
                .Distinct()
                .ToHashSet();

            if (dates.Count == 0) return 0;

            var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
            var checkDate = dates.Contains(today) ? today : today.AddDays(-1);

            int streak = 0;
            while (dates.Contains(checkDate))
            {
                streak++;
                checkDate = checkDate.AddDays(-1);
            }

            return streak;
        }
    }

}
