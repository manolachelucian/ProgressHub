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
    public class MacroCalculatorTests
    {
        private IMacroCalculatorService calculator = new MacroCalculator();
        [Fact]
        public void Calculate_ShouldComputeCorrectValues_ForMaleMuscleGain()
        {

            // Arrange
            var input = new MacroCalculationInput
            {
                Gender = Gender.Male,
                AgeInYears = 22,
                HeightInCm = 175,
                WeightInKg = 77.0,
                ActivityMultiplier = 1.55,
                Goal = FitnessGoal.MuscleGain,
                ProteinPerKg = 2.0
            };



            //Act
            var result = calculator.Calculate(input);

            //Assert

            result.Bmr.Should().Be(1759);
            result.TargetCalories.Should().BeGreaterThan(result.Tdee);
            result.TargetProteinGrams.Should().Be(154);

        }


        // wrong data
        [Theory]
        [InlineData(0.4)] 
        [InlineData(5.1)]
        [InlineData(-5.1)]
        public void Calculate_ShouldThrowException_WhenProteinOutOfRange(double invalidProtein)
        {
            //arrange
            var input = new MacroCalculationInput { ProteinPerKg = invalidProtein };

            var act = () => calculator.Calculate(input);
            
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

    }
}
