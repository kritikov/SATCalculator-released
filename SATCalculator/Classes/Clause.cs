using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    public class Clause {

        #region Fields

        public SATFormula ParentFormula { get; set; } = new SATFormula();
        public List<Literal> Literals { get; set; } = new List<Literal>();
        public VariablesCollection Variables { get; set; } = new VariablesCollection();
        public Dictionary<string, Variable> VariablesList { get; set; } = new Dictionary<string, Variable>();
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
            VariablesList = new Dictionary<string, Variable>();
        }

        public Clause(List<string> parts) :base()
        {
            foreach (string part in parts)
            {
                if (part != "0")
                {
                    Literal literal = new Literal(part);
                    AddLiteral(literal);
                }
            }

            Variables = new VariablesCollection(this);
        }

        public Clause(SATFormula formula, List<string> parts) : base() {

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

        // Add a literal in the clause and update its variable with proper clauses references
        public void AddLiteral(Literal literal)
        {
            literal.ParentClause = this;
            Literals.Add(literal);

            if (VariablesList.ContainsKey(literal.Variable.Name))
            {
                // if the variable is allready exists in the clause then use this one in the literal
                Variable existingVariable = VariablesList[literal.Variable.Name];
                literal.Variable = existingVariable;
            }
            else
            {
                // if the variable is not created yet in the clause then add it from the literal
                this.VariablesList.Add(literal.Variable.Name, literal.Variable);
            }

            if (literal.Sign == Sign.Positive)
                literal.Variable.ClausesWithPositiveAppearance.Add(this);
            else if (literal.Sign == Sign.Negative)
                literal.Variable.ClausesWithNegativeAppearance.Add(this);
        }

        /// return a clause from a resolution of two others 
        public static Clause Resolution(Variable variable, Clause positiveClause, Clause negativeClause)
        {
            Clause newClause = new Clause();

            // check the literals in the clause with the positive reference of the variable
            foreach (var literal in positiveClause.Literals)
            {
                if (literal.Variable != variable)
                {
                    // if the literal doesnt exists allready in the new clause add it
                    Literal existingLiteral = newClause.Literals.Where(p => p.Variable == literal.Variable).FirstOrDefault();
                    if (existingLiteral == null)
                    {
                        newClause.Literals.Add(literal);
                    }
                    else
                    {
                        // if the literal exists in the new clause but with opposite value
                        // then the clause is always true and can be discarded
                        if (existingLiteral.IsPositive != literal.IsPositive)
                        {
                            //newClause.Literals.Remove(existingLiteral);
                            return new Clause();
                        }
                    }
                }
            }

            // check the literals in the clause with the negative reference of the variable
            foreach (var literal in negativeClause.Literals)
            {
                if (literal.Variable != variable)
                {
                    // if the literal doesnt exists allready in the new clause add it
                    Literal existingLiteral = newClause.Literals.Where(p => p.Variable == literal.Variable).FirstOrDefault();
                    if (existingLiteral == null)
                    {
                        newClause.Literals.Add(literal);
                    }
                    else
                    {
                        // if the literal exists in the new clause but with opposite value
                        // then the clause is always true and can be discarded
                        if (existingLiteral.IsPositive != literal.IsPositive)
                        {
                            //newClause.Literals.Remove(existingLiteral);
                            return new Clause();
                        }
                    }
                }
            }

            return newClause;
        }

        #endregion


    }
}
