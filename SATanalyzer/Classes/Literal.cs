using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATanalyzer.Classes
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
                if (Sign == Sign.Positive)
                    return Variable.Name;
                else
                    return "-" + Variable.Name;
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
