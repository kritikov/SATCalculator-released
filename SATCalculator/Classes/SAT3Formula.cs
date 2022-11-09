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
    public enum Sign { Positive, Nagetive };

    public class SAT3Formula
    {
        public ObservableCollection<Clause> Clauses { get; set; } = new ObservableCollection<Clause>();
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, Trinity> Trinities { get; set; } = new Dictionary<string, Trinity>();

        public SAT3Formula()
        {

        }

        public void AddClause(Literal x1, Literal x2, Literal x3)
        {

        }

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

        public string DisplayValue => this.ToString();
        public int ClausesCount => Clauses.Count;
        public int VariablesCount => Variables.Count;
        public int TrinitiesCount => Trinities.Count;

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

            return variable;

        }

        /// <summary>
        /// Check if a trinity of variables exists in the list with the formula trinities.
        /// If exists then returns the existing trinity or creates a new one
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Trinity CreateTrinity(Clause clause)
        {
            Trinity trinity = new Trinity(clause);

            if (this.Trinities.ContainsKey(trinity.Name))
            {
                trinity = Trinities[trinity.Name];
                trinity.References++;
            }
            else
                this.Trinities.Add(trinity.Name, trinity);

            return trinity;

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

                    Literal[] literals = new Literal[3];
                    for (int i=0; i<3; i++) {
                        if (lineParts[i][0] == '-') {
                            Variable variable = formula.CreateVariable(lineParts[i]);
                            variable.ReferencesNegative++;
                            literals[i] = new Literal(variable, false);
                        }
                        else if (lineParts[0][0] == '+') {
                            Variable variable = formula.CreateVariable(lineParts[i]);
                            variable.ReferencesPositive++;
                            literals[i] = new Literal(variable, true);
                        }
                        else {
                            Variable variable = formula.CreateVariable(lineParts[i]);
                            literals[i] = new Literal(variable, true);
                            variable.ReferencesPositive++;
                        }
                    }

                    Literal[] sortedLiterals = literals.OrderBy(c => c.Variable.CnfIndex).ToArray();
                    Clause clause = new Clause(sortedLiterals[0], sortedLiterals[1], sortedLiterals[2]);

                    Trinity trinity = formula.CreateTrinity(clause);
                    clause.Trinity = trinity;

                    formula.Clauses.Add(clause);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return formula;
        }
    }
}
