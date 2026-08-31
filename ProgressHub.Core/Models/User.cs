using ProgressHub.Core.Models.Enums;
using ProgressHub.Core.Validation;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;


namespace ProgressHub.Core.Models
{

    public class User
    {

        // Basic profile information

        public int Id { get; set; }

        [Required(ErrorMessage ="First name is required.")]
        [StringLength(50, MinimumLength = 1)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage ="Last name is required.")]
        [StringLength(50, MinimumLength = 1)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        // Not user-entered via forms — populated by an auth/hashing service.
        // No [Required] here on purpose; see question below about auth scope.
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public UserRole UserRole { get; set; } = UserRole.Client;
        public Gender Gender { get; set; } = Gender.Male;

        // goal for client
        public FitnessGoal FitnessGoal { get; set; } = FitnessGoal.None;

        // Physical parameters for BMI etc.

        [Range(50, 300, ErrorMessage = "Height must be between 50 and 300 cm.")]
        public int HeightInCm { get; set; }

        [NotFutureDate(ErrorMessage = "Date of birth cannot be in the future.")]
        public DateOnly DateOfBirth { get; set; }



        // Goals from Coach
        [Range(0, 10000, ErrorMessage = "Target calories must be between 0 and 10000.")]
        public int TargetCalories { get; set; }

        [Range(0, 500, ErrorMessage = "Target protein must be between 0 and 500 g.")]
        public int TargetProteinGrams { get; set; }

        [Range(0, 1000, ErrorMessage = "Target carbs must be between 0 and 1000 g.")]
        public int TargetCarbsGrams { get; set; }

        [Range(0, 500, ErrorMessage = "Target fats must be between 0 and 500 g.")]
        public int TargetFatsGrams { get; set; }

        // 1:N relation to logs
        public List<DailyLog> DailyLogs { get; set; } =new();

        //Method for get age
        public int GetAge()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - DateOfBirth.Year;

            if (DateOfBirth > today.AddYears(-age))
            {
                age--;
            }
            return age;
        }
        public string GetFullName() => $"{FirstName} {LastName}".Trim();
        public double? CalculateBmi(double weightKg)
        {
            if (HeightInCm <= 0 || weightKg <= 0)
            {
                return null;
            }

            var heightM = HeightInCm / 100.0;
            return weightKg / (heightM * heightM);
        }
    }
}
