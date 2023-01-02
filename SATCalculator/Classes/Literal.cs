using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SATCalculator.Classes {
    public class Literal {

        #region Fields

        public Clause ParentClause = new Clause();
        public Variable Variable { get; set; } = new Variable();
        public Sign Sign { get; set; }
        public string Value {
            get {
                if (Sign == Sign.Positive)
                    return Variable.Name;
                else
                    return "-" + Variable.Name;
            }
        }
        public VariableValueEnum Valuation {
            get {

                if (Variable == null || Variable?.Valuation == VariableValueEnum.Null)
                    return VariableValueEnum.Null;

                if (Sign == Sign.Positive) {
                    return Variable.Valuation;
                }

                if (Variable.Valuation == VariableValueEnum.True)
                    return VariableValueEnum.False;
                else
                    return VariableValueEnum.True;
            }
        }

        #endregion


        #region Constructors

        public Literal()
        {
            Sign = Sign.Positive;
        }

        public Literal(string displayValue) : base()
        {
            Variable = new Variable(displayValue);

            if (displayValue[0] == '-')
            {
                Sign = Sign.Negative;
            }
            else
            {
                Sign = Sign.Positive;
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return Value;
        }

        #endregion
    }
}
