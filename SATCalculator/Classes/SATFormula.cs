using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SATCalculator.Classes {

    public class SATFormula {

        #region Fields

        public ObservableCollection<Clause> Clauses { get; set; } = new ObservableCollection<Clause>();
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, VariablesCollection> VariablesPerClause { get; set; } = new Dictionary<string, VariablesCollection>();
        public string DisplayValue => this.ToString();
        public int ClausesCount => Clauses.Count;
        public int VariablesCount => Variables.Count;
        public int VariablesPerClauseCount => VariablesPerClause.Count;

        #endregion


        #region Constructors

        public SATFormula() {

        }

        #endregion


        #region Methods

        public override string ToString() {
            string value = "";

            foreach (Clause clause in Clauses) {
                if (value != "")
                    value += " ^ ";

                value += $"({clause})" ;
            }

            value += ")";

            return value;
        }

        public void AddClause(Clause clause)
        {
            clause.ParentFormula = this;
            this.Clauses.Add(clause);

            // update the variables list
            foreach (var variableDict in clause.VariablesList)
            {
                var clauseVariable = variableDict.Value;

                if (this.Variables.ContainsKey(variableDict.Key))
                {
                    // if the variable is allready exists in the clause then use this one in the clause
                    Variable existingVariable = this.Variables[variableDict.Key];
                    existingVariable.Valuation = clauseVariable.Valuation;

                    foreach (var variablePositiveClause in clauseVariable.ClausesWithPositiveAppearance)
                    {
                        existingVariable.ClausesWithPositiveAppearance.Add(variablePositiveClause);
                    }
                    foreach (var variableNegativeClause in clauseVariable.ClausesWithNegativeAppearance)
                    {
                        existingVariable.ClausesWithNegativeAppearance.Add(variableNegativeClause);
                    }

                    clause.VariablesList[variableDict.Key] = existingVariable;
                    //clauseVariable = existingVariable;
                }
                else
                {
                    clauseVariable.ParentFormula = this;
                    this.Variables.Add(variableDict.Key, clauseVariable);
                }
            }

            // update the variables collections
            if (this.VariablesPerClause.ContainsKey(clause.VariablesCollection.Name))
            {
                var existingCollection = this.VariablesPerClause[clause.VariablesCollection.Name];
                existingCollection.References++;
                clause.VariablesCollection = existingCollection;
            }
            else
            {
                this.VariablesPerClause.Add(clause.VariablesCollection.Name, clause.VariablesCollection);
            }
        }

        public void AddClause(List<string> parts)
        {
            Clause clause = new Clause(parts);
            clause.ParentFormula = this;
            this.AddClause(clause);
        }

        /// <summary>
        /// Create a clone of the formula in single SAT form
        /// </summary>
        /// <returns></returns>
        public SATFormula CopyAsSATFormula()
        {
            SATFormula formula = new SATFormula();

            List<string> parts;
            foreach (var clause in Clauses)
            {
                parts = new List<string>();
                foreach (var literal in clause.Literals)
                {
                    string pros = literal.IsPositive ? "+" : "-";

                    parts.Add($"{pros}{literal.Variable.CnfIndex}");
                }

                formula.AddClause(parts);

            }

            return formula;
        }
        #endregion
    }
}
