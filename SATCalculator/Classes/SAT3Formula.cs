using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;

namespace SATCalculator.Classes
{
    public class SAT3Formula
    {
        #region Fields

        public ObservableCollection<Clause> Clauses { get; set; } = new ObservableCollection<Clause>();
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, Trinity> Trinities { get; set; } = new Dictionary<string, Trinity>();
        public string DisplayValue => this.ToString();
        public int ClausesCount => Clauses.Count;
        public int VariablesCount => Variables.Count;
        public int TrinitiesCount => Trinities.Count;

        #endregion


        #region Constructors

        public SAT3Formula()
        {

        }

        #endregion


        #region Methods

        public override string ToString()
        {
            string value = "";

            foreach (Clause clause in Clauses)
            {
                if (value != "")
                    value += " ^ ";

                value += clause.ToString();
            }

            return value;
        }


        /// <summary>
        /// Check if a variable name exists in the list with the formula unique variable.
        /// If exists then returns the existing variable or creates a new one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Variable CreateVariable(string value) {
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
        /// Create an instance of SAT3Formula from a cnf file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SAT3Formula GetFromFile(string filename) {

            SAT3Formula formula = new SAT3Formula();

            try {
                string[] lines = File.ReadAllLines(filename);

                foreach (string line in lines) {
                    //Console.WriteLine(line);

                    var lineParts = line.Trim().Split(' ');

                    if (lineParts[0] == "c")
                        continue;

                    if (lineParts[0] == "p")
                        continue;

                    if (lineParts.Count() != 4)
                        continue;

                    formula.CreateAndAddClause(lineParts);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return formula;
        }

        /// <summary>
        /// Create a clause from strings and add it to the list with the clauses
        /// </summary>
        /// <param name="lineParts"></param>
        public void CreateAndAddClause(string[] lineParts) {
            Clause clause = new Clause(this, lineParts);
            this.Clauses.Add(clause);
        }

        #endregion
    }

}
