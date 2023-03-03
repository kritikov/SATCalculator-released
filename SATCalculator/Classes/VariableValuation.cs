using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes
{
    public class VariableValuation
    {
        public Variable Variable { get; set; } = new Variable();
        public ValuationEnum Valuation { get; set; } = ValuationEnum.Null;

    }
}
