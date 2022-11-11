using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    public class Literal {

        #region Fields

        public Clause ParentClause;
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
        public VariableValueEnum Valuation {
            get {

                if (Variable == null || Variable?.Valuation == VariableValueEnum.Null)
                    return VariableValueEnum.Null;

                if (this.IsPositive) {
                    return Variable.Valuation;
                }
                else {
                    if (Variable.Valuation == VariableValueEnum.True)
                        return VariableValueEnum.False;
                    else
                        return VariableValueEnum.True;
                }
            }
        }

        #endregion


        #region Constructors

        public Literal(Clause clause, bool isPositive, string part) {
            Variable variable = clause.ParentFormula.CreateVariables(part);

            this.ParentClause = clause;

            if (isPositive)
                variable.ReferencesPositive++;
            else 
                variable.ReferencesNegative++;

            Variable = variable;
            IsPositive = isPositive;
        }

        #endregion
    }
}
