using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATanalyzer.Classes
{
    public class Variable
    {

        #region Fields

        public static string DefaultVariableName = "x";

        public Guid Id = Guid.NewGuid();

        public Literal LiteralPositive { get; set; }
        public Literal LiteralNegative { get; set; }

        public string Name
        {
            get
            {
                if (CnfIndex == -1)
                    return "TRUE";
                else if (CnfIndex == -2)
                    return "FALSE";
                else
                {
                    return DefaultVariableName + CnfIndex.ToString();
                }
            }
        }
        public int CnfIndex { get; set; } = 0;
        public ValuationEnum Valuation { get; set; } = ValuationEnum.Null;

        #endregion


        #region Constructors

        public Variable()
        {
            LiteralPositive = new Literal(this, Sign.Positive);
            LiteralNegative = new Literal(this, Sign.Negative);
        }

        public Variable(string value) : base()
        {
            try
            {
                if (value == "TRUE" || value == "+TRUE" || value == "-TRUE")
                {
                    CnfIndex = -1;
                }
                else if (value == "FALSE" || value == "+FALSE" || value == "-FALSE")
                {
                    CnfIndex = -2;
                }
                else
                {
                    string numbers = new String(value.Where(Char.IsDigit).ToArray());

                    if (string.IsNullOrEmpty(numbers))
                        numbers = "0";

                    CnfIndex = Convert.ToInt32(numbers);
                }
            }
            catch (Exception ex)
            {
                //Logs.Write(ex.Message);
            }
        }

        #endregion


        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion

        //public override bool Equals(object @object)
        //{
        //    return @object is Variable variable &&
        //           Name == variable.Name;
        //}

        //public override int GetHashCode()
        //{
        //    return HashCode.Combine(Name);
        //}
    }
}
