using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes
{
    public class VariableValuation
    {
        public string VariableName { get; set; }
        public ValuationEnum Valuation { get; set; } = ValuationEnum.Null;

        public string DisplayValue
        {
            get
            {
                string value = $"{VariableName} = {Valuation}";

                return value;
            }
        }

        public override string ToString()
        {
            return DisplayValue;
        }

    }
}
