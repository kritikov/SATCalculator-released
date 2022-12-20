using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    
    public class VariablesCollection {

        #region Fields

        public HashSet<Variable> Items { get; set; } = new HashSet<Variable>();
        public int References { get; set; } = 0;

        public string Name
        {
            get
            {
                string name = "";
                foreach (var variable in Items)
                {
                    if (name != "")
                        name += "-";

                    name += variable.Name;
                }

                return name;
            }
        }

        public string Contents
        {
            get
            {
                string name = "";
                foreach (var variable in Items)
                {
                    if (name != "")
                        name += ", ";

                    name += variable.Name;
                }

                return name;
            }
        }
        #endregion


        #region Constructors

        public VariablesCollection()
        {

        }

        public VariablesCollection(Clause clause) : base() {

            List<Literal> literalsOrdered = clause.Literals.OrderBy(p => p.Variable.Name).ToList();

            foreach (var literal in literalsOrdered) {
                Items.Add(literal.Variable);
            }

            References = 1;
        }

        #endregion


        #region Methods

        public override string ToString() {
            return Contents;
        }

        #endregion
    }
}
