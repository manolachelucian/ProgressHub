using FluentAssertions;
using ProgressHub.Core.Interfaces;
using ProgressHub.Core.Models.Enums;
using ProgressHub.Core.Services.MacroCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Tests.Services.MacroCalculatorTests
{
    public class MacroDistributionTests
    {
        private readonly IMacroCalculatorService _calculator = new MacroCalculator();

        [Theory]
        [InlineData(FitnessGoal.MuscleGain, 3026)]      // tdee(2726) + 300
        [InlineData(FitnessGoal.Recomposition, 2526)]   // tdee(2726) - 200
        [InlineData(FitnessGoal.Endurance, 2826)]       // tdee(2726) + 100
        public void Calculate_ShouldApplyCorrectCalorieDelta_PerGoal(FitnessGoal goal, int expectedTargetCalories)
        {
            var input = MaleBaseline(goal);

            var result = _calculator.Calculate(input);

            result.TargetCalories.Should().Be(expectedTargetCalories);
        }

        [Theory]
        [InlineData(FitnessGoal.Maintenance)]
        [InlineData(FitnessGoal.None)]
        public void Calculate_ShouldLeaveTargetCaloriesEqualToTdee_WhenGoalHasNoDelta(FitnessGoal goal)
        {
            var input = MaleBaseline(goal);

            var result = _calculator.Calculate(input);

            // Asserting the invariant (delta == 0) rather than a hand-computed number —
            // robust to the exact BMR/TDEE arithmetic changing later.
            result.TargetCalories.Should().Be(result.Tdee);
        }

        [Fact]
        public void Calculate_ShouldEnforce1200CalorieFloor_WhenDeficitWouldGoLower()
        {
            // Small female, low activity, WeightLoss goal — TDEE(1584) - 500 = 1084,
            // which is below the 1200 kcal safety floor.
            var input = new MacroCalculationInput
            {
                Gender = Gender.Female,
                WeightInKg = 60,
                HeightInCm = 165,
                AgeInYears = 30,
                ActivityMultiplier = 1.2,
                Goal = FitnessGoal.WeightLoss,
                ProteinPerKg = 2.2
            };

            var result = _calculator.Calculate(input);

            result.TargetCalories.Should().Be(1200);
        }

        [Fact]
        public void Calculate_ShouldFloorCarbsAtZero_WhenProteinAndFatAloneExceedTargetCalories()
        {
            // Large male, sedentary, WeightLoss, protein pinned to the max valid
            // value (5.0 g/kg) — protein calories alone (2000 kcal) exceed the
            // 1906 kcal target, so carbs must clamp at 0 rather than go negative.
            var input = new MacroCalculationInput
            {
                Gender = Gender.Male,
                WeightInKg = 100,
                HeightInCm = 180,
                AgeInYears = 25,
                ActivityMultiplier = 1.2,
                Goal = FitnessGoal.WeightLoss,
                ProteinPerKg = 5.0
            };

            var result = _calculator.Calculate(input);

            result.TargetCarbsGrams.Should().Be(0);
            result.TargetProteinGrams.Should().Be(500);
            result.TargetFatsGrams.Should().Be(53);
        }

        [Theory]
        [InlineData(0.5)]   // inclusive lower bound
        [InlineData(5.0)]   // inclusive upper bound
        public void Calculate_ShouldNotThrow_WhenProteinIsExactlyOnTheValidBoundary(double proteinPerKg)
        {
            var input = MaleBaseline(FitnessGoal.Maintenance);
            input.ProteinPerKg = proteinPerKg;

            var act = () => _calculator.Calculate(input);

            act.Should().NotThrow();
        }

        private static MacroCalculationInput MaleBaseline(FitnessGoal goal) => new()
        {
            Gender = Gender.Male,
            WeightInKg = 77,
            HeightInCm = 175,
            AgeInYears = 22,
            ActivityMultiplier = 1.55,
            Goal = goal,
            ProteinPerKg = 2.0
        };
    }
}
