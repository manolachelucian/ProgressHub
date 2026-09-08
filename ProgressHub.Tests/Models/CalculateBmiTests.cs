using ProgressHub.Core.Models;
using FluentAssertions;

namespace ProgressHub.Tests.Models
{
    public class CalculateBmiTests
    {
        [Fact]
        public void CalculateBmi_ShouldReturnCorrectValue_ForTypicalInputs()
        {
            var user = new User { HeightInCm = 180 };

            var bmi = user.CalculateBmi(weightKg: 80);

            // 80 / (1.80 * 1.80) = 24.691...
            bmi.Should().BeApproximately(24.69, 0.01);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void CalculateBmi_ShouldReturnNull_WhenHeightIsZeroOrNegative(int heightCm)
        {
            var user = new User { HeightInCm = heightCm };

            var bmi = user.CalculateBmi(weightKg: 75);

            bmi.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void CalculateBmi_ShouldReturnNull_WhenWeightIsZeroOrNegative(double weightKg)
        {
            var user = new User { HeightInCm = 180 };

            var bmi = user.CalculateBmi(weightKg);

            bmi.Should().BeNull();
        }

        [Fact]
        public void CalculateBmi_ShouldHandleExtremeButValidInputs_WithoutThrowing()
        {
            // Upper bounds of the [Range] constraints on HeightInCm/Weight — the
            // method itself has no upper limit, so this just confirms no overflow
            // or divide-by-near-zero surprises at the domain's stated extremes.
            var user = new User { HeightInCm = 300 };

            var bmi = user.CalculateBmi(weightKg: 500);

            bmi.Should().NotBeNull();
            bmi!.Value.Should().BeApproximately(55.56, 0.01);
        }
    }
}
