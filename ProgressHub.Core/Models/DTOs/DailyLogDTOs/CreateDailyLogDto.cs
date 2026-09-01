using ProgressHub.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Models.DTOs.DailyLogDTOs
{
    public class CreateDailyLogDto
    {
        public int UserId { get; set; }

        [Required]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [Range(30.0, 300.0, ErrorMessage = "Weight must be between 30 and 300 kg.")]
        public double Weight { get; set; }

        [Range(0, 10000)]
        public int ConsumedCalories { get; set; }

        [Range(0, 500)]
        public int ConsumedProteins { get; set; }

        [Range(0, 1000)]
        public int ConsumedCarbs { get; set; }

        [Range(0, 300)]
        public int ConsumedFats { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        public TrainingType TrainingType { get; set; } = TrainingType.RestDay;
    }


}
