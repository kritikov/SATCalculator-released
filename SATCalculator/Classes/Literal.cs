using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    public class Literal {

        #region Fields

        public Clause ParentClause = new Clause();
        public Variable Variable { get; set; } = new Variable();
        public Sign Sign { get; set; }
        public bool IsPositive { get; set; } = true;
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
            IsPositive = true;
        }

        public Literal(string displayValue) : base()
        {
            Variable = new Variable(displayValue);

            if (displayValue[0] == '-')
            {
                IsPositive = false;
                Sign = Sign.Negative;
            }
            else
            {
                IsPositive = true;
                Sign = Sign.Positive;
            }
        }

        public Literal(string displayValue, VariableCreationType creationType) : base()
        {
            Variable = new Variable(displayValue, creationType);

            if (displayValue[0] == '-')
            {
                IsPositive = false;
                Sign = Sign.Negative;
            }
            else
            {
                IsPositive = true;
                Sign = Sign.Positive;
            }
        }

        public Literal(Clause clause, bool isPositive, Variable variable) : base()
        {
            this.ParentClause = clause;

            Variable = variable;
            IsPositive = isPositive;
        }


        #endregion
    }
}
