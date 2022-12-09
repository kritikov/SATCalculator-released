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
        public int CnfIndex { get; set; }
        public VariableValueEnum Valuation { get; set; } = VariableValueEnum.Null;


        // usefull for fast searching
        public SATFormula ParentFormula { get; set; }
        public List<Clause> ClausesWithPositiveAppearance { get; set; } = new List<Clause>();
        public List<Clause> ClausesWithNegativeAppearance { get; set; } = new List<Clause>();

        public int References => ClausesWithPositiveReferencesCount + ClausesWithNegativeReferencesCount;
        public int ClausesWithPositiveReferencesCount => ClausesWithPositiveAppearance.Count;
        public int ClausesWithNegativeReferencesCount => ClausesWithNegativeAppearance.Count;

        #endregion


        #region Constructors
        public Variable()
        {

        }

        /// <summary>
        /// Create a variable from a string as found in a cnf file
        /// </summary>
        /// <param name="valueInCnf"></param>
        public Variable(string valueInCnf) : base(){

            valueInCnf = valueInCnf.Trim();

            if (valueInCnf[0] == '-') {
                Name = Variable.DefaultVariableName + valueInCnf.Substring(1, valueInCnf.Length - 1);
                CnfIndex = Convert.ToInt32(valueInCnf.Substring(1, valueInCnf.Length - 1));
            }
            else if (valueInCnf[0] == '+') {
                Name = Variable.DefaultVariableName + valueInCnf.Substring(1, valueInCnf.Length - 1);
                CnfIndex = Convert.ToInt32(valueInCnf.Substring(1, valueInCnf.Length - 1));
            }
            else {
                Name = Variable.DefaultVariableName + valueInCnf;
                CnfIndex = Convert.ToInt32(valueInCnf);
            }
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
