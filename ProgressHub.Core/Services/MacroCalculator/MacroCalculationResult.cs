using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Services.MacroCalculator
{
    public class MacroCalculationResult
    {

        public MacroCalculationResult() { }

        public int Bmr { get; init; }
        public int Tdee { get; init; }
        public int TargetCalories { get; init; }
        public int TargetProteinGrams { get; init; }
        public int TargetCarbsGrams { get; init; }
        public int TargetFatsGrams { get; init; }
    }
}
