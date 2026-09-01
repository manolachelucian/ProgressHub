using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Models.DTOs.DailyLogDTOs
{
    public class DailyLogItemDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly Date { get; set; }
        public double Weight { get; set; }
        public int ConsumedCalories { get; set; }
        public int ConsumedProteins { get; set; }
        public int ConsumedCarbs { get; set; }
        public int ConsumedFats { get; set; }
        public string? Note { get; set; }
    }
}

