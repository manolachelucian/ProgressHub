using ProgressHub.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Services.MacroCalculator
{
    public  class MacroCalculationInput
    {

        public MacroCalculationInput() { }

        public Gender Gender { get; set; }
        public int AgeInYears { get; set; }
        public int HeightInCm { get; set; }
        public double WeightInKg { get; set; }
        public double ActivityMultiplier { get; set; } = 1.55;
        public FitnessGoal Goal { get; set; }
        public double ProteinPerKg { get; set; } = 2.0;
    }
}
