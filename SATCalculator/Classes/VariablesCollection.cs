using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    
    public class VariablesCollection {
        public HashSet<Variable> Items { get; set; } = new HashSet<Variable>();
        public int References { get; set; }

        public VariablesCollection(Clause clause) {

            foreach(var literal in clause.Literals) {
                Items.Add(literal.Variable);
            }

            References = 1;
        }

        public string Name {
            get {

                string name = "";
                foreach (var variable in Items) {
                    if (name != "")
                        name += "-";

                    name += variable.Name;
                }

                return name;
            }
        }

        public string Contents {
            get {

                string name = "";
                foreach (var variable in Items) {
                    if (name != "")
                        name += ", ";

                    name += variable.Name;
                }

                return name;
            }
        }

        public override string ToString() {
            return Contents;
        }
    }
}
