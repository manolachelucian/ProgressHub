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
    public class BmrAndTdeeTests
    {
        private readonly IMacroCalculatorService _calculator = new MacroCalculator();


        // bmrBase = 10*W + 6.25*H - 5*A
        // Male: +5 | Female: -161 | Other: -78 (midpoint, see prior review)
        [Theory]
        [InlineData(Gender.Male, 77, 175, 22, 1.55, 1758.75, 2726)]
        [InlineData(Gender.Female, 60, 165, 30, 1.2, 1320.25, 1584)]
        [InlineData(Gender.Other, 70, 170, 25, 1.375, 1559.5, 2144)]
        public void Calculate_ShouldComputeCorrectBmrAndTdee_PerGender(Gender gender, double weightKg, int heightCm, int ageYears,double activityMultiplier, double expectedBmr, int expectedTdee)
        {
            var input = new MacroCalculationInput
            {
                Gender = gender,
                WeightInKg = weightKg,
                HeightInCm = heightCm,
                AgeInYears = ageYears,
                ActivityMultiplier = activityMultiplier,
                Goal = FitnessGoal.Maintenance,
                ProteinPerKg = 2.0
            };

            var result = _calculator.Calculate(input);

            result.Bmr.Should().BeApproximately(expectedBmr, 0.001);
            result.Tdee.Should().Be(expectedTdee);
        }

        [Fact]
        public void Calculate_ShouldThrow_WhenGenderIsUnmappedEnumValue()
        {
            // A cast past the enum's declared members — simulates corrupted/invalid
            // data reaching the calculator (e.g. deserialization of a stale integer).
            var input = new MacroCalculationInput
            {
                Gender = (Gender)99,
                WeightInKg = 70,
                HeightInCm = 175,
                AgeInYears = 30,
                ProteinPerKg = 2.0
            };

            var act = () => _calculator.Calculate(input);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }


    }
}
