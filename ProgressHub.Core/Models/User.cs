using ProgressHub.Core.Enums;


namespace ProgressHub.Core.Models
{
    public class User
    {

        //Basic profile informations
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public UserRole UserRole { get; set; } = UserRole.Client;
       
        //Physic parameters for calculation of BMI and etc...

        public int HeightInCm { get; set; }
        public DateOnly DateOfBirth { get; set; }



        // Goals from Coach
        public int TargetCalories { get; set; }
        public int TargetProteinGrams { get; set; }
        public int TargetCarbsGrams { get; set; }
        public int TargetFatsGrams { get; set; }

        // Propojení na záznamy (vazba 1:N)
        //public List<DailyLog> DailyLogs { get; set; } = new();



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
        public string GetFullName() => $"Fullname: {FirstName} {LastName}".Trim();



    }
}
