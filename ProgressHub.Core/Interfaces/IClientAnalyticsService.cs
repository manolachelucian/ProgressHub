using ProgressHub.Core.Enums;
using ProgressHub.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Interfaces
{
    public interface IClientAnalyticsService
    {
        ClientAnalyticsSummary CalculateSummary(User client, AnalyticsTimeWindow window = AnalyticsTimeWindow.Days14);
    }
}
