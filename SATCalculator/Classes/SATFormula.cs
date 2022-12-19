using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace SATCalculator.Classes {

    public class SATFormula : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        #region Fields

        public ObservableCollection<Clause> Clauses { get; set; } = new ObservableCollection<Clause>();
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, VariablesCollection> VariablesPerClause { get; set; } = new Dictionary<string, VariablesCollection>();
        public string DisplayValue => this.ToString();
        public int ClausesCount => Clauses.Count;
        public int VariablesCount => Variables.Count;
        public int VariablesPerClauseCount => VariablesPerClause.Count;

        private Variable selectedVariable;
        public Variable SelectedVariable
        {
            get => selectedVariable;
            set
            {
                selectedVariable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedVariable"));
            }
        }
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
        /// Add a clause to the formula. Thiw method will synchronize also the variables of the formula and the literals
        /// </summary>
        /// <param name="clause"></param>
        public void AddClause(Clause clause)
        {
            clause.ParentFormula = this;
            this.Clauses.Add(clause);
            clause.Variables.Clear();

            // update the variables list
            foreach(var literal in clause.Literals)
            {
                if (Variables.ContainsKey(literal.Variable.Name))
                {
                    // if the variable is allready exists in the list then use this one in the literal and its clause
                    Variable existingVariable = Variables[literal.Variable.Name];
                    literal.Variable = existingVariable;
                    clause.Variables.Add(existingVariable.Name, existingVariable);

                    if (literal.Sign == Sign.Positive)
                        Variables[literal.Variable.Name].ClausesWithPositiveAppearance.Add(clause);
                    else if (literal.Sign == Sign.Negative)
                        Variables[literal.Variable.Name].ClausesWithNegativeAppearance.Add(clause);
                }
                else
                {
                    // if the variable is not created yet in the clause then add it from the literal
                    this.Variables.Add(literal.Variable.Name, literal.Variable);
                }
            }

            clause.VariablesCollection = new VariablesCollection(clause);


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

        public void AddClause(List<string> parts, VariableCreationType creationType)
        {
            Clause clause = new Clause(parts, creationType);
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

                    parts.Add($"{pros}{literal.Variable.Name}");
                }

                formula.AddClause(parts, VariableCreationType.Default);

            }

            return formula;
        }

        /// <summary>
        /// Get the literals of the clauses in a list to be saved in a SAT file
        /// </summary>
        /// <returns></returns>
        public List<string> GetSATLines()
        {
            List<string> cnfLines = new List<string>();

            foreach(var clause in this.Clauses)
            {
                string line = "";
                foreach(var literal in clause.Literals)
                {
                    if (line != "")
                        line += " ";

                    line += literal.Value;
                    
                }
                line += $" 0";
                cnfLines.Add(line);
            }

            return cnfLines;
        } 

        #endregion
    }
}
