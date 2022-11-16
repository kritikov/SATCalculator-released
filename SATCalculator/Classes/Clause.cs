using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    public class Clause {

        #region Fields

        public SATFormula ParentFormula { get; set; }
        public List<Literal> Literals { get; set; }
        public VariablesCollection Variables { get; set; }
        public VariableValueEnum Valuation {
            get {

                if (Literals.Count(p=>p.Valuation == VariableValueEnum.True) == Literals.Count)
                    return VariableValueEnum.True;

                if (Literals.Count(p => p.Valuation == VariableValueEnum.False) == Literals.Count)
                    return VariableValueEnum.False;

                return VariableValueEnum.Null;
            }
        }

        public string Name {
            get {

                string name = "";
                foreach(var literal in Literals) {
                    if (name != "")
                        name += " ∨ ";

                    name += literal.Value;
                }

                return name;
            }
        }

        #endregion


        #region Constructors

        public Clause()
        {
            Literals = new List<Literal>();
            Variables = new VariablesCollection(this);
        }

        public Clause(SATFormula formula, List<string> parts) {

            ParentFormula = formula;
            Literals = new List<Literal>();

            foreach(string part in parts) {
                if (part != "0")
                    CreateAndAddLiteral(part);
            }

            Literals = this.Literals.OrderBy(c => c.Variable.CnfIndex).ToList();
            Variables = CreateVariablesCollection();
        }


        #endregion


        #region Methods

        public override string ToString() {
            return Name;
        }

        /// <summary>
        /// Check if the collection of variables from the clause exists in the list with the formula variables per clause.
        /// If exists then returns the existing collection or else creates a new one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public VariablesCollection CreateVariablesCollection() {
            VariablesCollection variablesCollection = new VariablesCollection(this);

            if (this.ParentFormula.VariablesPerClause.ContainsKey(variablesCollection.Name)) {
                variablesCollection = this.ParentFormula.VariablesPerClause[variablesCollection.Name];
                variablesCollection.References++;
            }
            else {
                this.ParentFormula.VariablesPerClause.Add(variablesCollection.Name, variablesCollection);
            }

            return variablesCollection;
        }

        /// <summary>
        /// Create a literal from a string and add it to the list
        /// </summary>
        /// <param name="part"></param>
        public void CreateAndAddLiteral(string part) {
            Variable variable = ParentFormula.CreateVariables(part);

            Literal literal;

            if (part[0] == '-') {
                variable.ClausesWithNegativeAppearance.Add(this);
                literal = new Literal(this, false, variable);
            }
            else if (part[0] == '+') {
                variable.ClausesWithPositiveAppearance.Add(this);
                literal = new Literal(this, true, variable);
            }
            else {
                variable.ClausesWithPositiveAppearance.Add(this);
                literal = new Literal(this, true, variable);
            }

            this.Literals.Add(literal);
        }

        #endregion


    }
}
