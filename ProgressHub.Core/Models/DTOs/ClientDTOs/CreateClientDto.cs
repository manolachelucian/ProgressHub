using ProgressHub.Core.Constants;
using ProgressHub.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;


namespace ProgressHub.Core.Models.DTOs.ClientDTOs
{
    public class CreateClientDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name is too long.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name is too long.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]

        public string Email { get; set; } = string.Empty;


        public string PhonePrefixCode { get; set; } = PhonePrefix.DefaultPrefix;

        [RegularExpression(@"^[0-9]{6,12}$", ErrorMessage = "Enter a valid phone number without spaces or prefix.")]
        public string? LocalPhoneNumber { get; set; }
        public string? FullPhoneNumber =>
        string.IsNullOrWhiteSpace(LocalPhoneNumber) ? null : $"{PhonePrefixCode}{LocalPhoneNumber.Trim()}";

        [Required]
        public DateOnly DateOfBirth { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25));

        public DateTime CreatedAt {  get; set; } = DateTime.UtcNow;

        public Gender Gender { get; set; } = Gender.Male;
        public FitnessGoal FitnessGoal { get; set; } = FitnessGoal.WeightLoss;

        [Range(100, 250, ErrorMessage = "Height must be between 100 and 250 cm.")]
        public int HeightInCm { get; set; } = 175;

        [Range(1000, 10000, ErrorMessage = "Target calories must be realistic.")]
        public int TargetCalories { get; set; } = 1000;

        [Range(0, 500)]
        public int TargetProteinGrams { get; set; } = 0;

        [Range(0, 1000)]
        public int TargetCarbsGrams { get; set; } = 0;

        [Range(0, 300)]
        public int TargetFatsGrams { get; set; } = 0;
    }
}
