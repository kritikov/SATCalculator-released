using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SATCalculator.NewClasses
{
    public class Clause
    {
        #region Fields

        public List<Literal> Literals { get; set; } = new List<Literal>();

        public string Name
        {
            get
            {
                string name = "";
                foreach (var literal in Literals)
                {
                    if (name != "")
                        name += " ∨ ";

                    name += literal.Name;
                }

                return name;
            }
        }

        public string NameSorted
        {
            get
            {
                var literals = Literals.OrderBy(p => p.Variable.CnfIndex).Select(p => p.Name).ToArray();
                string keyText = string.Join(" ∨ ", literals);
                return keyText;
            }
        }

        #endregion


        #region Constructors

        public Clause()
        {

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
