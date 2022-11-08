using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    public class Literal {
        public Variable Variable { get; set; }
        public bool IsPositive { get; set; }
        public string Value {
            get {
                if (this.IsPositive)
                    return Variable.Name;
                else
                    return "-" + Variable.Name;
            }
        }

        public Literal(Variable variable, bool isPositive) {
            Variable = variable;
            IsPositive = isPositive;
        }
    }
}
