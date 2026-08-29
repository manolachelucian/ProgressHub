using ProgressHub.Core.Services.MacroCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Interfaces
{
    public  interface IMacroCalculatorService
    {
        public MacroCalculationResult Calculate(MacroCalculationInput input);
        public bool IsProteinValid(double protein);

    }
}
