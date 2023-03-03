using SATCalculator.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes
{
    public class Variable
    {

        #region Fields

        public static string DefaultVariableName = "x";

        public Guid Id = Guid.NewGuid();

        public Literal PositiveLiteral { get; set; }
        public Literal NegativeLiteral { get; set; }

        public string Name
        {
            get
            {
                if (CnfIndex == -1)
                    return "T/F";
                else
                    return DefaultVariableName + CnfIndex.ToString();
            }
        }
        public int CnfIndex { get; set; } = 0;
        public ValuationEnum Valuation { get; set; } = ValuationEnum.Null;

        public int ClausesWithPositiveReferencesCount => PositiveLiteral.ClausesContainingIt.Count;
        public int ClausesWithNegativeReferencesCount => NegativeLiteral.ClausesContainingIt.Count;
        public int References => ClausesWithPositiveReferencesCount + ClausesWithNegativeReferencesCount;
        public int Contrasts => Math.Min(ClausesWithPositiveReferencesCount, ClausesWithNegativeReferencesCount);

        public static Variable FixedVariable = new Variable("FIXED");

        #endregion


        #region Constructors

        public Variable()
        {
            
        }

        public Variable(string value) : base()
        {
            PositiveLiteral = new Literal(this, Sign.Positive);
            NegativeLiteral = new Literal(this, Sign.Negative);

            try
            {
                if (value == "FIXED")
                {
                    CnfIndex = -1;
                }
                else
                {
                    string numbers = new string(value.Where(Char.IsDigit).ToArray());

                    if (string.IsNullOrEmpty(numbers))
                        numbers = "0";

                    CnfIndex = Convert.ToInt32(numbers);
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
            }
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
