using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {

    public class Variable {
        public SAT3Formula ParentFormula;

        private static string DefaultVariableName = "x";

        public string Name { get; set; }
        public int CnfIndex { get; set; }
        public VariableValueEnum Valuation { get; set; } = VariableValueEnum.Null;

        public int References => ReferencesPositive + ReferencesNegative;
        public int ReferencesPositive { get; set; } = 0;
        public int ReferencesNegative { get; set; } = 0;

        public Variable(string valueInCnf) {

            valueInCnf = valueInCnf.Trim();

            if (valueInCnf[0] == '-') {
                Name = Variable.DefaultVariableName + valueInCnf.Substring(1, valueInCnf.Length - 1);
                CnfIndex = Convert.ToInt32(valueInCnf.Substring(1, valueInCnf.Length - 1));
                //ReferencesNegative++;
            }
            else if (valueInCnf[0] == '+') {
                Name = Variable.DefaultVariableName + valueInCnf.Substring(1, valueInCnf.Length - 1);
                CnfIndex = Convert.ToInt32(valueInCnf.Substring(1, valueInCnf.Length - 1));
                //ReferencesPositive++;
            }
            else {
                Name = Variable.DefaultVariableName + valueInCnf;
                CnfIndex = Convert.ToInt32(valueInCnf);
                //ReferencesPositive++;
            }
        }
    }
}
