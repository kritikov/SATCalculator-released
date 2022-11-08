using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    public class Variable {
        private static string DefaultVariableName = "x";

        public string Name { get; set; }
        public int CnfIndex { get; set; }

        public int ReferencesInLiterals { get; set; }

        public Variable(string valueInCnf) {

            ReferencesInLiterals = 1;
            valueInCnf = valueInCnf.Trim();

            if (valueInCnf[0] == '-') {
                Name = Variable.DefaultVariableName + valueInCnf.Substring(1, valueInCnf.Length - 1);
                CnfIndex = Convert.ToInt32(valueInCnf.Substring(1, valueInCnf.Length - 1));
            }
            else if (valueInCnf[0] == '+') {
                Name = Variable.DefaultVariableName + valueInCnf.Substring(1, valueInCnf.Length - 1);
                CnfIndex = Convert.ToInt32(valueInCnf.Substring(1, valueInCnf.Length - 1));
            }
            else {
                Name = Variable.DefaultVariableName + valueInCnf;
                CnfIndex = Convert.ToInt32(valueInCnf);
            }
        }
    }
}
