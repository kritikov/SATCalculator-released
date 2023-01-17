using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SATCalculator.NewClasses
{
    public class SATFormula : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        #region Fields

        public ObservableCollection<Clause> Clauses { get; set; } = new ObservableCollection<Clause>();
        public ObservableCollection<Variable> Variables { get; set; } = new ObservableCollection<Variable>();
        public ObservableCollection<Literal> Literals { get; set; } = new ObservableCollection<Literal>();

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

                value += ")";

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
                throw new Exception(ex.Message);
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

                    // create the clause
                    Clause clause = new Clause();

                    if (lineParts.Count > 0)
                    {
                        if (lineParts[0].Length > 0)
                        {
                            if (lineParts[0] == "c" || lineParts[0] == "p" || lineParts[0] == "0" || lineParts[0] == "%")
                                continue;

                            foreach (var part in lineParts)
                            {
                                if (part != "0")
                                {
                                    // create the variable
                                    Variable variable = new Variable(part);

                                    // use the stored variable if exists or add the new to the dictionary
                                    if (formula.VariablesDict.ContainsKey(variable.Name))
                                        variable = formula.VariablesDict[variable.Name];
                                    else
                                    {
                                        formula.VariablesDict.Add(variable.Name, variable);
                                        formula.Variables.Add(variable);
                                    }

                                    // create the literal
                                    Literal literal;
                                    if (part[0] == '-')
                                        literal = variable.NegativeLiteral;
                                    else
                                        literal = variable.PositiveLiteral;

                                    // add the literal in the dictionary if doesnt exists
                                    if (!formula.LiteralsDict.ContainsKey(literal.Name))
                                        formula.LiteralsDict.Add(literal.Name, literal);
                                    if (!formula.Literals.Contains(literal))
                                        formula.Literals.Add(literal);

                                    // add the literal to the clause
                                    clause.Literals.Add(literal);
                                    literal.ClausesWithAppearances.Add(clause);
                                }
                            }
                        }
                    }

                    // sort the literals in the clause
                    clause.Literals = clause.Literals.OrderBy(p => p.Variable.CnfIndex).ToList();

                    // add the clause to the dictionary and the list with formula clauses
                    // it wont add duplicate clauses
                    if (!formula.ClausesDict.ContainsKey(clause.Name))
                    {
                        formula.Clauses.Add(clause);
                        formula.ClausesDict.Add(clause.Name, clause);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return formula;

        }

        public List<string> GetCNFLines()
        {
            List<string> cnfLines = new List<string>();

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

            return cnfLines;

        }

        /// <summary>
        /// Create a clone of the formula in single SAT form
        /// </summary>
        /// <returns></returns>
        public SATFormula CopyAsSATFormula()
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
            // remove clause from the literals lists
            foreach(var literal in clause.Literals)
            {
                literal.ClausesWithAppearances.Remove(clause);
            }

            // remove the clause from the dictionaries
            if (ClausesDict.ContainsKey(clause.Name))
                ClausesDict.Remove(clause.Name);

            if (Clauses.Contains(clause))
                Clauses.Remove(clause);

        }

        #endregion
    }
}
