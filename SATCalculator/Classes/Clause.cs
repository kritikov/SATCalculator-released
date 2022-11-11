using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    public class Clause {

        #region Fields

        public SAT3Formula ParentFormula { get; set; }
        public List<Literal> Literals { get; set; }
        public Trinity Trinity { get; set; }
        public VariableValueEnum Valuation {
            get {
                if (Literals[0].Valuation == VariableValueEnum.True ||
                    Literals[1].Valuation == VariableValueEnum.True ||
                    Literals[2].Valuation == VariableValueEnum.True)
                    return VariableValueEnum.True;

                else if (Literals[0].Valuation == VariableValueEnum.Null &&
                    Literals[1].Valuation == VariableValueEnum.Null &&
                    Literals[2].Valuation == VariableValueEnum.Null) {
                    return VariableValueEnum.Null;
                }
                else {
                    return VariableValueEnum.False;
                }
            }
        }

        public string Name => $@"({Literals[0].Value} ∨ {Literals[1].Value} ∨ {Literals[2].Value})";

        #endregion


        #region Constructors

        public Clause(SAT3Formula formula, string[] parts) {

            ParentFormula = formula;
            Literals = new List<Literal>();

            for (int i = 0; i < 3; i++) {
                CreateAndAddLiteral(parts[i]);
            }

            Literals = this.Literals.OrderBy(c => c.Variable.CnfIndex).ToList();
            Trinity = CreateTrinity();
        }


        #endregion


        #region Methods

        public override string ToString() {
            return Name;
        }

        /// <summary>
        /// Check if the trinity from variables of the clause exists in the list with the formula trinities.
        /// If exists then returns the existing trinity or else creates a new one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Trinity CreateTrinity() {
            Trinity trinity = new Trinity(this);

            if (this.ParentFormula.Trinities.ContainsKey(trinity.Name)) {
                trinity = this.ParentFormula.Trinities[trinity.Name];
                trinity.References++;
            }
            else {
                this.ParentFormula.Trinities.Add(trinity.Name, trinity);
            }

            return trinity;
        }

        /// <summary>
        /// Create a literal from a string and add it to the list
        /// </summary>
        /// <param name="part"></param>
        public void CreateAndAddLiteral(string part) {
            Literal literal;

            if (part[0] == '-') {
                literal = new Literal(this, false, part);
            }
            else if (part[0] == '+') {
                literal = new Literal(this, true, part);
            }
            else {
                literal = new Literal(this, true, part);
            }

            this.Literals.Add(literal);
        }

        #endregion


    }
}
