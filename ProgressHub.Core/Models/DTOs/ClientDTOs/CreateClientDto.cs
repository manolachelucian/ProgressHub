using ProgressHub.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [Required]
        public DateOnly DateOfBirth { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25));

        public Gender Gender { get; set; } = Gender.Male;
        public FitnessGoal FitnessGoal { get; set; } = FitnessGoal.WeightLoss;

        [Range(100, 250, ErrorMessage = "Height must be between 100 and 250 cm.")]
        public int HeightInCm { get; set; } = 175;

        [Range(1000, 10000, ErrorMessage = "Target calories must be realistic.")]
        public int TargetCalories { get; set; } = 2000;

        [Range(0, 500)]
        public int TargetProteinGrams { get; set; }

        [Range(0, 1000)]
        public int TargetCarbsGrams { get; set; }

        [Range(0, 300)]
        public int TargetFatsGrams { get; set; }
    }
}
