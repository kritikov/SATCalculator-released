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


        /// <summary>
        /// Check if a variable name exists in the list with the formula unique variable.
        /// If exists then returns the existing variable or creates a new one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Variable CreateVariables(string value) {
            Variable variable = new Variable(value);

            if (this.Variables.ContainsKey(variable.Name)) {
                variable = Variables[variable.Name];
            }
            else
                this.Variables.Add(variable.Name, variable);

            variable.ParentFormula = this;

            return variable;

        }

        /// <summary>
        /// Create a clause from strings and add it to the list with the clauses
        /// </summary>
        /// <param name="lineParts"></param>
        public void CreateAndAddClause(List<string> lineParts) {
            Clause clause = new Clause(this, lineParts);
            this.Clauses.Add(clause);
        }

        /// <summary>
        /// Create a clone of the formula in single SAT form
        /// </summary>
        /// <returns></returns>
        public SATFormula CopyAsSATFormula() {
            SATFormula formula = new SATFormula();

            List<string> parts;
            foreach (var clause in Clauses) {
                parts = new List<string>();
                foreach (var literal in clause.Literals) {
                    string pros = literal.IsPositive ? "+" : "-";

                    parts.Add($"{pros}{literal.Variable.CnfIndex}");
                }

                formula.CreateAndAddClause(parts);

            }

            return formula;
        }

        #endregion
    }
}
