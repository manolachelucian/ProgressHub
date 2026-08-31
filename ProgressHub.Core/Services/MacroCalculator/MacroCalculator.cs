using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models.Enums;

namespace ProgressHub.Core.Services.MacroCalculator
{
    public class MacroCalculator : IMacroCalculatorService
    {
        public const double MinProteinPerKg = 0.5;
        public const double MaxProteinPerKg = 5.0;

        public bool IsProteinValid(double protein) =>
            protein is >= MinProteinPerKg and <= MaxProteinPerKg;


        public MacroCalculationResult Calculate(MacroCalculationInput input)
        {
            ArgumentNullException.ThrowIfNull(input);

            if (!IsProteinValid(input.ProteinPerKg))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(input.ProteinPerKg),
                    $"Protein must be between {MinProteinPerKg} and {MaxProteinPerKg} g/kg.");
            }

            // Mifflin-St Jeor vzorec
            double bmrBase = (10 * input.WeightInKg) + (6.25 * input.HeightInCm) - (5 * input.AgeInYears);
            double calculatedBmr = input.Gender == Gender.Female ? bmrBase - 161 : bmrBase + 5;

            int bmr = (int)Math.Round(calculatedBmr);
            int tdee = (int)Math.Round(bmr * input.ActivityMultiplier);

            int calorieDelta = input.Goal switch
            {
                FitnessGoal.WeightLoss => -500,
                FitnessGoal.MuscleGain => 300,
                FitnessGoal.Recomposition => -200,
                FitnessGoal.Endurance => 100,
                _ => 0
            };

            int targetCalories = Math.Max(1200, tdee + calorieDelta);
            int protein = (int)Math.Round(input.WeightInKg * input.ProteinPerKg);
            int fats = (int)Math.Round((targetCalories * 0.25) / 9);

            int remainingCalories = targetCalories - (protein * 4) - (fats * 9);
            int carbs = Math.Max(0, (int)Math.Round(remainingCalories / 4.0));

            return new MacroCalculationResult
            {
                Bmr = bmr,
                Tdee = tdee,
                TargetCalories = targetCalories,
                TargetProteinGrams = protein,
                TargetCarbsGrams = carbs,
                TargetFatsGrams = fats
            };
        }

    }
}
