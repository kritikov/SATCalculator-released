using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {

    public class Variable {

        #region Fields

        private static string DefaultVariableName = "x";

        public Guid Id = Guid.NewGuid();
        public string Name { get; set; }
        public int CnfIndex { get; set; } = -1;
        public VariableValueEnum Valuation { get; set; } = VariableValueEnum.Null;


        // usefull for fast searching
        public SATFormula ParentFormula { get; set; }
        public List<Clause> ClausesWithPositiveAppearance { get; set; } = new List<Clause>();
        public List<Clause> ClausesWithNegativeAppearance { get; set; } = new List<Clause>();

        public int References => ClausesWithPositiveReferencesCount + ClausesWithNegativeReferencesCount;
        public int ClausesWithPositiveReferencesCount => ClausesWithPositiveAppearance.Count;
        public int ClausesWithNegativeReferencesCount => ClausesWithNegativeAppearance.Count;
        public int Contradictions => Math.Min(ClausesWithPositiveAppearance.Count, ClausesWithNegativeAppearance.Count);

        #endregion


        #region Constructors
        public Variable()
        {

        }

        /// <summary>
        /// Create a variable from a string as found in a cnf file
        /// </summary>
        /// <param name="value"></param>
        public Variable(string value) : base(){

            value = value.Trim();

            if (value[0] == '-' || value[0] == '+')
            {
                Name = value.Substring(1, value.Length - 1);
            }
            else
            {
                Name = value;
            }
        }

        public Variable(string value, VariableCreationType type) : base()
        {
            if (type == VariableCreationType.Cnf)
            {
                value = value.Trim();

                if (value[0] == '-' || value[0] == '+')
                {
                    Name = Variable.DefaultVariableName + value.Substring(1, value.Length - 1);
                    CnfIndex = Convert.ToInt32(value.Substring(1, value.Length - 1));
                }
                else
                {
                    Name = Variable.DefaultVariableName + value;
                    CnfIndex = Convert.ToInt32(value);
                }
            }
            else if (type == VariableCreationType.Default)
            {
                value = value.Trim();

                if (value[0] == '-' || value[0] == '+')
                {
                    Name = value.Substring(1, value.Length - 1);
                }
                else
                {
                    Name = value;
                }
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
