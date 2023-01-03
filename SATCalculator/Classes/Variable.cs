using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SATCalculator.Classes
{

    public class Variable
    {

        #region Fields

        public static string DefaultVariableName = "x";

        public Guid Id = Guid.NewGuid();
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
        public VariableValueEnum Valuation { get; set; } = VariableValueEnum.Null;


        // usefull for fast searching
        public SATFormula ParentFormula { get; set; }
        public List<Clause> ClausesWithAppearance { get; set; } = new List<Clause>();
        public List<Clause> ClausesWithPositiveAppearance { get; set; } = new List<Clause>();
        public List<Clause> ClausesWithNegativeAppearance { get; set; } = new List<Clause>();

        public int References => ClausesWithPositiveReferencesCount + ClausesWithNegativeReferencesCount;
        public int ClausesWithPositiveReferencesCount => ClausesWithPositiveAppearance.Count;
        public int ClausesWithNegativeReferencesCount => ClausesWithNegativeAppearance.Count;
        public int Contrasts => Math.Min(ClausesWithPositiveAppearance.Count, ClausesWithNegativeAppearance.Count);

        #endregion


        #region Constructors
        public Variable()
        {

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
                    value = value.Trim();

                    if (value[0] == '-' || value[0] == '+')
                    {
                        string text = value.Substring(1, value.Length - 1);
                        string numbers = new String(text.Where(Char.IsDigit).ToArray());

                        if (String.IsNullOrEmpty(numbers))
                            numbers = "0";

                        CnfIndex = Convert.ToInt32(numbers);
                    }
                    else
                    {
                        string numbers = new String(value.Where(Char.IsDigit).ToArray());

                        if (String.IsNullOrEmpty(numbers))
                            numbers = "0";

                        CnfIndex = Convert.ToInt32(numbers);
                    }
                }
            }
            catch(Exception ex)
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
