using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    public class Variable {
        private static string DefaultVariableName = "x";

        public string Name { get; set; }

        public int ReferencesInLiterals { get; set; }

        public Variable(string value) {

            ReferencesInLiterals = 1;
            value = value.Trim();

            if (value[0] == '-') {
                Name = Variable.DefaultVariableName + value.Substring(1, value.Length - 1);
            }
            else if (value[0] == '+') {
                Name = Variable.DefaultVariableName + value.Substring(1, value.Length - 1);
            }
            else {
                Name = Variable.DefaultVariableName + value;
            }
        }
    }
}
