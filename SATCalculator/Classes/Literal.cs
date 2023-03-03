using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes
{
    public class Literal : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        #region Fields

        public Variable Variable { get; set; } = new Variable();
        public Sign Sign { get; set; }
        public string SignToString { get
            {
                if (this.Sign == Sign.Negative)
                    return "-";
                else
                    return "+";
            } 
        }
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

                // if literal is positive
                if (Sign == Sign.Positive)
                {
                    return Variable.Valuation;
                }

                // if literal is negative
                if (Variable.Valuation == ValuationEnum.True)
                    return ValuationEnum.False;
                else
                    return ValuationEnum.True;
            }
        }

        public ObservableCollection<Clause> ClausesContainingIt { get; set; } = new ObservableCollection<Clause>();

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
