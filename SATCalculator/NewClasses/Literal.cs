using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.NewClasses
{
    public class Literal
    {

        #region Fields

        public Variable Variable { get; set; } = new Variable();
        public Sign Sign { get; set; }
        public string Name
        {
            get
            {
                if (Variable.CnfIndex == -1)
                {
                    if (Sign == Sign.Positive)
                        return "TRUE";
                    else
                        return "FALSE";
                }
                else
                {
                    if (Sign == Sign.Positive)
                        return Variable.Name;
                    else
                        return "-" + Variable.Name;
                }
            }
        }
        public ValuationEnum Valuation
        {
            get
            {

                if (Variable == null || Variable?.Valuation == ValuationEnum.Null)
                    return ValuationEnum.Null;

                if (Sign == Sign.Positive)
                {
                    return Variable.Valuation;
                }

                if (Variable.Valuation == ValuationEnum.True)
                    return ValuationEnum.False;
                else
                    return ValuationEnum.True;
            }
        }

        public List<Clause> ClausesWithAppearances { get; set; } = new List<Clause>();

        public Literal Opposite { 
            get
            {
                if (this.Sign == Sign.Positive)
                    return this.Variable.NegativeLiteral;
                else
                    return this.Variable.PositiveLiteral;
            } 
        }

        #endregion


        #region Constructors

        public Literal(Variable variable, Sign sign)
        {
            Variable = variable;
            Sign = sign;
        }

        #endregion


        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
