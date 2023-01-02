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
        public Dictionary<string, Variable> VariablesDict { get; set; } = new Dictionary<string, Variable>();
        public HashSet<string> ClausesDict { get; set; } = new HashSet<string>();
        public string DisplayValue => this.ToString();
        public int ClausesCount => Clauses.Count;
        public int VariablesCount => VariablesDict.Count;

        private Variable selectedVariable = null;
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
                if (VariablesDict.ContainsKey(literal.Variable.Name))
                {
                    // if the variable is allready exists in the list then use this one in the literal and its clause
                    Variable existingVariable = VariablesDict[literal.Variable.Name];
                    literal.Variable = existingVariable;
                    clause.Variables.Add(existingVariable.Name, existingVariable);

                    VariablesDict[literal.Variable.Name].ClausesWithAppearance.Add(clause);
                    if (literal.Sign == Sign.Positive)
                        VariablesDict[literal.Variable.Name].ClausesWithPositiveAppearance.Add(clause);
                    else if (literal.Sign == Sign.Negative)
                        VariablesDict[literal.Variable.Name].ClausesWithNegativeAppearance.Add(clause);
                }
                else
                {
                    // if the variable is not created yet in the clause then add it from the literal
                    this.VariablesDict.Add(literal.Variable.Name, literal.Variable);
                    clause.Variables.Add(literal.Variable.Name, literal.Variable);
                }
            }

            ClausesDict.Add(clause.NameSorted);
        }

        public void AddClause(List<string> parts, VariableCreationType creationType)
        {
            Clause clause = new Clause(parts, creationType);
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
                    string pros = literal.Sign == Sign.Positive ? "+" : "-";

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
        /// <summary>
        /// Get the literals of the clauses in a list to be saved in a SAT file
        /// </summary>
        /// <returns></returns>
        public List<string> GetCNFLines()
        {
            List<string> cnfLines = new List<string>();

            ReformatAsCnf();

            foreach (var clause in this.Clauses)
            {
                string line = "";
                foreach (var literal in clause.Literals)
                {
                    if (line != "")
                        line += " ";

                    if (literal.Sign == Sign.Positive)
                        line += literal.Variable.CnfIndex.ToString();
                    else
                        line += "-" + literal.Variable.CnfIndex.ToString();
                }
                line += $" 0";
                cnfLines.Add(line);
            }

            return cnfLines;
        } 

        /// <summary>
        /// Recreates cnf indexes in the variables of the formula
        /// </summary>
        public void ReformatAsCnf()
        {
            int cnfIndex = 0;
            foreach(var variableDict in VariablesDict)
            {
                Variable variable = variableDict.Value;
                variable.CnfIndex = ++cnfIndex;
                variable.Name = $"{Variable.DefaultVariableName}{cnfIndex}";
            }
        }

        /// <summary>
        /// Create an instance of SAT3Formula from a cnf file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SATFormula GetFromCnfFile(string filename)
        {

            SATFormula formula = new SATFormula();

            try
            {
                string[] lines = File.ReadAllLines(filename);

                foreach (string line in lines)
                {
                    var lineParts = line.Trim().Split(' ').ToList();

                    if (lineParts[0] == "c")
                        continue;

                    if (lineParts[0] == "p")
                        continue;

                    if (lineParts[0] == "0")
                        continue;

                    formula.AddClause(lineParts, VariableCreationType.Cnf);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


            return formula;
        }

        #endregion
    }
}
