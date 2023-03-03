using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SATCalculator.Classes
{
    public class SATFormula : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        #region Fields

        public ObservableCollection<Clause> Clauses { get; set; } = new ObservableCollection<Clause>();
        public ObservableCollection<Variable> Variables { get; set; } = new ObservableCollection<Variable>();
        public ObservableCollection<Literal> Literals { get; set; } = new ObservableCollection<Literal>();
        public ObservableCollection<Solution> Solutions { get; set; } = new ObservableCollection<Solution>();

        public Dictionary<string, Literal> LiteralsDict { get; set; } = new Dictionary<string, Literal>();
        public Dictionary<string, Variable> VariablesDict { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, Clause> ClausesDict { get; set; } = new Dictionary<string, Clause>();

        public int ClausesCount => Clauses.Count;
        public int VariablesCount => Variables.Count;

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

        public string Name
        {
            get{
                string value = "";

                foreach (Clause clause in Clauses)
                {
                    if (value != "")
                        value += " ^ ";

                    value += $"({clause})";
                }

                return value;
            }
        }

        public string DisplayValue => this.ToString();

        #endregion


        #region Constructors

        public SATFormula()
        {

        }

        #endregion


        #region Methods

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Create an instance of SAT3Formula from a cnf file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SATFormula GetFromCnfFile(string filename)
        {
            SATFormula formula;

            try
            {
                string[] linesArr = File.ReadAllLines(filename);

                List<string> lines = linesArr.ToList();

                formula = CreateFromCnfLines(lines);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return formula;
        }

        public static SATFormula CreateFromCnfLines(List<string> lines)
        {
            SATFormula formula = new SATFormula();

            try
            {
                foreach (string line in lines)
                {
                    var lineParts = line.Trim().Split(' ').ToList();

                    formula.CreateAndAddClause(lineParts);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return formula;

        }

        /// <summary>
        /// Conver the formula to a list of cnf lines
        /// </summary>
        /// <returns></returns>
        public List<string> GetCNFLines()
        {
            List<string> cnfLines = new List<string>();

            try
            {
                foreach (var clause in this.Clauses)
                {
                    string line = "";
                    foreach (var literal in clause.Literals)
                    {
                        if (line != "")
                            line += " ";

                        line += literal.Name;
                    }
                    line += $" 0";
                    cnfLines.Add(line);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return cnfLines;

        }

        /// <summary>
        /// Create a clone of the formula
        /// </summary>
        /// <returns></returns>
        public SATFormula Copy()
        {
            List<string> parts = this.GetCNFLines();
            SATFormula formula = CreateFromCnfLines(parts);

            return formula;
        }

        /// <summary>
        /// Remove a clause from the formula and its lists
        /// </summary>
        /// <param name="clause"></param>
        public void RemoveClause(Clause clause)
        {
            try
            {
                // remove clause from the literals lists
                foreach (var literal in clause.Literals)
                {
                    literal.ClausesContainingIt.Remove(clause);
                }

                // remove the clause from the dictionaries
                if (ClausesDict.ContainsKey(clause.Name))
                    ClausesDict.Remove(clause.Name);

                if (Clauses.Contains(clause))
                    Clauses.Remove(clause);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Add a clause to the formula if doesnt allready exists. The clause must allready use
        /// variables and literals declared in the formula
        /// </summary>
        /// <param name="clause"></param>
        public void AddClause(Clause clause)
        {
            try
            {
                // if the clause allready exists in the formula then exit
                if (ClausesDict.ContainsKey(clause.Name))
                    return;

                ClausesDict.Add(clause.Name, clause);
                Clauses.Add(clause);

                // add clause to the literals lists
                foreach (var literal in clause.Literals)
                    literal.ClausesContainingIt.Add(clause);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Create and add a clause to the formula from a list of literals
        /// </summary>
        /// <param name="lineParts"></param>
        public void CreateAndAddClause(List<string> lineParts)
        {
            try
            {
                // create the clause
                Clause clause = new Clause();

                if (lineParts.Count > 0)
                {
                    if (lineParts[0].Length > 0)
                    {
                        if (lineParts[0] == "c" || lineParts[0] == "p" || lineParts[0] == "0" || lineParts[0] == "%")
                            return;

                        foreach (var part in lineParts)
                        {
                            if (part != "0")
                            {
                                // create the variable
                                Variable variable = new Variable(part);

                                // use the stored variable if exists or add the new to the dictionary
                                if (VariablesDict.ContainsKey(variable.Name))
                                    variable = VariablesDict[variable.Name];
                                else
                                {
                                    VariablesDict.Add(variable.Name, variable);
                                    Variables.Add(variable);
                                }

                                // create the literal
                                Literal literal;
                                if (part[0] == '-')
                                    literal = variable.NegativeLiteral;
                                else
                                    literal = variable.PositiveLiteral;

                                // add the literal in the dictionary if doesnt exists
                                if (!LiteralsDict.ContainsKey(literal.Name))
                                    LiteralsDict.Add(literal.Name, literal);
                                if (!Literals.Contains(literal))
                                    Literals.Add(literal);

                                // add the literal to the clause
                                clause.Literals.Add(literal);
                                literal.ClausesContainingIt.Add(clause);
                            }
                        }
                    }
                }

                // sort the literals in the clause
                clause.Literals = clause.Literals.OrderBy(p => p.Variable.CnfIndex).ToList();

                // add the clause to the dictionary and the list with formula clauses
                // it wont add duplicate clauses
                if (!ClausesDict.ContainsKey(clause.Name))
                {
                    Clauses.Add(clause);
                    ClausesDict.Add(clause.Name, clause);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SolveDetermistic()
        {
            bool findAllSolutions = false;

            Solutions.Clear();


            Solution solution = new Solution();
            solution.ValuationsList.Add(new VariableValuation() { Valuation = ValuationEnum.True, Variable = this.Variables[0] });

            Solutions.Add(solution);

        }

        #endregion
    }
}
