﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    public class Clause {

        #region Fields

        public SATFormula ParentFormula { get; set; } = new SATFormula();
        public List<Literal> Literals { get; set; } = new List<Literal>();
        public VariablesCollection VariablesCollection { get; set; } = new VariablesCollection();
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
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
            VariablesCollection = new VariablesCollection(this);
            Variables = new Dictionary<string, Variable>();
        }

        public Clause(List<string> parts, VariableCreationType creationType) :base()
        {
            foreach (string part in parts)
            {
                if (part != "0")
                {
                    Literal literal = new Literal(part, creationType);
                    AddLiteral(literal);
                }
            }

            VariablesCollection = new VariablesCollection(this);
        }

        #endregion


        #region Methods

        public override string ToString() {
            return Name;
        }

        // Add a literal in the clause and update its variable with proper clauses references
        public void AddLiteral(Literal literal)
        {
            literal.ParentClause = this;
            Literals.Add(literal);

            if (Variables.ContainsKey(literal.Variable.Name))
            {
                // if the variable is allready exists in the clause then use this one in the literal
                Variable existingVariable = Variables[literal.Variable.Name];
                literal.Variable = existingVariable;
            }
            else
            {
                // if the variable is not created yet in the clause then add it from the literal
                this.Variables.Add(literal.Variable.Name, literal.Variable);
            }

            if (literal.Sign == Sign.Positive)
                literal.Variable.ClausesWithPositiveAppearance.Add(this);
            else if (literal.Sign == Sign.Negative)
                literal.Variable.ClausesWithNegativeAppearance.Add(this);

            VariablesCollection = new VariablesCollection(this);
        }

        /// return a clause from the resolution of two others 
        public static Clause Resolution(Variable variable, Clause positiveClause, Clause negativeClause)
        {
            Clause newClause = new Clause();

            if (positiveClause.Literals.Count == 1 && negativeClause.Literals.Count == 1 &&
                positiveClause.Literals[0].Variable == variable &&
                negativeClause.Literals[0].Variable == variable)
            {
                Literal newLiteral = new Literal("FALSE");
                newClause = new Clause();
                newClause.AddLiteral(newLiteral);
                return newClause;
            }

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
                        if (existingLiteral.Sign != literal.Sign)
                        {
                            Literal newLiteral = new Literal("TRUE");
                            newClause = new Clause();
                            newClause.AddLiteral(newLiteral);
                            return newClause;
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
                        if (existingLiteral.Sign != literal.Sign)
                        {
                            //newClause.Literals.Remove(existingLiteral);
                            Literal newLiteral = new Literal("TRUE");
                            newClause = new Clause();
                            newClause.AddLiteral(newLiteral);
                            return newClause;
                        }
                    }
                }
            }

            return newClause;
        }

        public static Tuple<Clause, Clause> Simplification(Variable variableToReplace, Clause positiveClause, Clause negativeClause, List<Variable> allowedVariables)
        {
            Clause newPositiveClause = new Clause();
            Clause newNegativeClause = new Clause();

            // pick a valid variable to use for simplification
            Variable replacementVariable = null;
            foreach(var allowedVariable in allowedVariables)
            {
                if ( !positiveClause.Variables.ContainsKey(allowedVariable.Name) && !negativeClause.Variables.ContainsKey(allowedVariable.Name) )
                {
                    replacementVariable = allowedVariable;
                    break;
                }
            }

            // if no valid replacement found then use the same variable as replacement
            if (replacementVariable == null)
                replacementVariable = variableToReplace;

            // replace the variable in the positive clause
            foreach (var literal in positiveClause.Literals)
            {
                string value = literal.Sign == Sign.Positive ? "+" : "-";

                if (literal.Variable == variableToReplace)
                    value += replacementVariable.Name;
                else
                    value += literal.Variable.Name;

                Literal newLiteral = new Literal(value);
                newPositiveClause.AddLiteral(newLiteral);
            }

            // replace the variable in the negative clause
            foreach (var literal in negativeClause.Literals)
            {
                string value = literal.Sign == Sign.Positive ? "+" : "-";

                if (literal.Variable == variableToReplace)
                    value += replacementVariable.Name;
                else
                    value += literal.Variable.Name;

                Literal newLiteral = new Literal(value);
                newNegativeClause.AddLiteral(newLiteral);
            }

            Tuple<Clause, Clause> result = new Tuple<Clause, Clause>(newPositiveClause, newNegativeClause);

            return result;
        }

        #endregion


    }
}
