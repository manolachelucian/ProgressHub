using ProgressHub.Core.Validation;
using System.ComponentModel.DataAnnotations;


namespace ProgressHub.Core.Models
{
    public class DailyLog
    {
        
        public int Id { get; set; }

        // Relation to user
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [NotFutureDate]
        public DateOnly Date {  get; set; }

        [Range(0.1, 500, ErrorMessage = "Weight must be greater than 0.")]
        public double Weight { get; set; }


       

        [Range(0, 20000, ErrorMessage = "Calories cannot be negative.")]
        public int ConsumedCalories { get; set; }

        [Range(0, 2000, ErrorMessage = "Protein cannot be negative.")]
        public int ConsumedProteins { get; set; }

        [Range(0, 2000, ErrorMessage = "Carbs cannot be negative.")]
        public int ConsumedCarbs { get; set; }

        [Range(0, 2000, ErrorMessage = "Fats cannot be negative.")]
        public int ConsumedFats { get; set; }

        [StringLength(500)]
        public string? Note {  get; set; }
    }
}
