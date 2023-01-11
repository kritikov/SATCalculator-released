using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes
{
    public class AnalysisResults
    {
        #region VARIABLES AND NESTED CLASSES

        public List<VariablePair> VariablePairList { get; set; } = new List<VariablePair>();

        public int VariablesCount { get; set; } = 0;
        public Dictionary<string, EndingVariableAppearances> EndingVariablesAppearancesDict = new Dictionary<string, EndingVariableAppearances>();
        public Dictionary<Variable, Conflict> ConflictsDict = new Dictionary<Variable, Conflict>();
        public Dictionary<Variable, int> VariablesColumns = new Dictionary<Variable, int>();
        public DataTable AppearancesDataTable { get; set; }
        public List<string> ProblemsList { get; set; } = new List<string>();

        #endregion


        #region Constructors

        public AnalysisResults()
        {

        }

        #endregion


        #region Methods

        /// <summary>
        /// Ananyze the formula based on algorithm 1
        /// </summary>
        public static AnalysisResults Analyze(SATFormula formula)
        {
            AnalysisResults analysisResults = new AnalysisResults();
            analysisResults.VariablesCount = formula.VariablesCount;

            // check the number of the variables per clause

            // get a list with the variables sorted by the contrasts?
            var variablesSequence = formula.VariablesDict.Select(p => p.Value).OrderByDescending(p => p.Contrasts).ToList();

            // reset the used flag on formula clauses
            foreach(var clause in formula.Clauses)
                clause.Used = false;

            // create a list with the variables and their connected clauses
            foreach (var variable in variablesSequence)
            {
                VariablePair variablePair = new VariablePair();
                variablePair.Variable = variable;

                // get the clauses with the positive appearances, remove the positive appearances of the variable
                // and add the clauses in the proper list
                foreach (var clause in variable.ClausesWithPositiveAppearance)
                {
                    if (!clause.Used)
                    {
                        Clause positiveClause = new Clause();
                        Clause reducedClause = new Clause();

                        foreach (var literal in clause.Literals)
                        {
                            Literal newLiteral = new Literal(literal.Variable, literal.Sign);

                            if (literal.Variable != variable)
                            {
                                reducedClause.Literals.Add(newLiteral);
                                if (!reducedClause.Variables.ContainsKey(literal.Variable.Name))
                                    reducedClause.Variables.Add(literal.Variable.Name, literal.Variable);
                            }

                            positiveClause.AddLiteralSimple(newLiteral);
                            //positiveClause.Literals.Add(newLiteral);
                            //if (!positiveClause.Variables.ContainsKey(literal.Variable.Name))
                            //    positiveClause.Variables.Add(literal.Variable.Name, literal.Variable);
                        }

                        variablePair.PositiveClauses.Add(positiveClause);
                        variablePair.ClausesWhenNegativeIsTrue.Add(reducedClause);
                        clause.Used = true;
                    }
                }

                // get the clauses with the negative appearances, remove the negative appearances of the variable
                // and add the clauses in the proper list
                foreach (var clause in variable.ClausesWithNegativeAppearance)
                {
                    if (!clause.Used)
                    {
                        Clause negativeClause = new Clause();
                        Clause reducedClause = new Clause();

                        foreach (var literal in clause.Literals)
                        {
                            Literal newLiteral = new Literal(literal.Variable, literal.Sign);

                            if (literal.Variable != variable)
                            {
                                reducedClause.Literals.Add(newLiteral);
                                if (!reducedClause.Variables.ContainsKey(literal.Variable.Name))
                                    reducedClause.Variables.Add(literal.Variable.Name, literal.Variable);
                            }

                            negativeClause.AddLiteralSimple(newLiteral);
                            negativeClause.Literals.Add(newLiteral);
                            if (!negativeClause.Variables.ContainsKey(literal.Variable.Name))
                                negativeClause.Variables.Add(literal.Variable.Name, literal.Variable);
                        }

                        variablePair.NegativeClauses.Add(negativeClause);
                        variablePair.ClausesWhenPositiveIsTrue.Add(reducedClause);
                        clause.Used = true;
                    }
                }

                if (variablePair.ClausesWhenPositiveIsTrue.Count == 0)
                {
                    Literal newLiteral = new Literal($"+{variable}");
                    newLiteral.Variable = variable;
                    Clause reducedClause = new Clause();
                    reducedClause.AddLiteralSimple(newLiteral);
                    //reducedClause.Literals.Add(newLiteral);
                    //if (!reducedClause.Variables.ContainsKey(newLiteral.Variable.Name))
                    //    reducedClause.Variables.Add(newLiteral.Variable.Name, newLiteral.Variable);
                    variablePair.ClausesWhenPositiveIsTrue.Add(reducedClause);
                }

                if (variablePair.ClausesWhenNegativeIsTrue.Count == 0)
                {
                    Literal newLiteral = new Literal($"-{variable}");
                    newLiteral.Variable = variable;
                    Clause reducedClause = new Clause();
                    reducedClause.AddLiteralSimple(newLiteral);
                    //reducedClause.Literals.Add(newLiteral);
                    //if (!reducedClause.Variables.ContainsKey(newLiteral.Variable.Name))
                    //    reducedClause.Variables.Add(newLiteral.Variable.Name, newLiteral.Variable);
                    variablePair.ClausesWhenNegativeIsTrue.Add(reducedClause);
                }

                analysisResults.VariablePairList.Add(variablePair);
            }

            // reset the used flag on formula clauses
            foreach (var clause in formula.Clauses)
                clause.Used = false;

            // OBSOLETE: create the array with the appearances
            // create the columns
            int i = 0;
            foreach (var pair in analysisResults.VariablePairList)
            {
                analysisResults.VariablesColumns[pair.Variable] = i++;
            }

            // create the lines
            foreach (var pair in analysisResults.VariablePairList)
            {
                int columnIndex = analysisResults.VariablesColumns[pair.Variable] * 4;

                foreach (var positiveClause in pair.ClausesWhenPositiveIsTrue)
                {
                    if (positiveClause.Literals.Count == 1)
                    {
                        Literal literal = positiveClause.Literals[0];

                        EndingVariableAppearances variableAppearances;
                        if (analysisResults.EndingVariablesAppearancesDict.ContainsKey(literal.Variable.Name))
                        {
                            variableAppearances = analysisResults.EndingVariablesAppearancesDict[literal.Variable.Name];
                        }
                        else
                        {
                            variableAppearances = new EndingVariableAppearances();
                            variableAppearances.AppearancesPerVariable = new int[analysisResults.VariablesCount * 4];
                            variableAppearances.FoundOnce = true;
                            variableAppearances.FirstAppearanceSign = literal.Sign;
                            variableAppearances.VariableValue = literal.Variable.Name;

                            analysisResults.EndingVariablesAppearancesDict.Add(literal.Variable.Name, variableAppearances);
                        }

                        if (variableAppearances.FoundOnce == false)
                        {
                            variableAppearances.AppearancesPerVariable[columnIndex] = 1;
                            variableAppearances.NormalAppearances.Add(pair.Variable.Name);
                        }
                        else
                        {
                            if (variableAppearances.FirstAppearanceSign == literal.Sign)
                            {
                                variableAppearances.AppearancesPerVariable[columnIndex] = 1;
                                variableAppearances.NormalAppearances.Add(pair.Variable.Name);
                            }
                            else
                            {
                                variableAppearances.AppearancesPerVariable[columnIndex + 1] = 1;
                                variableAppearances.ContrastAppearances.Add(pair.Variable.Name);
                            }
                        }
                    }
                }

                foreach (var negativeClause in pair.ClausesWhenNegativeIsTrue)
                {
                    if (negativeClause.Literals.Count == 1)
                    {
                        Literal literal = negativeClause.Literals[0];

                        EndingVariableAppearances variableAppearances;
                        if (analysisResults.EndingVariablesAppearancesDict.ContainsKey(literal.Variable.Name))
                        {
                            variableAppearances = analysisResults.EndingVariablesAppearancesDict[literal.Variable.Name];
                        }
                        else
                        {
                            variableAppearances = new EndingVariableAppearances();
                            variableAppearances.AppearancesPerVariable = new int[analysisResults.VariablesCount * 4];
                            variableAppearances.FoundOnce = true;
                            variableAppearances.FirstAppearanceSign = literal.Sign;
                            variableAppearances.VariableValue = literal.Variable.Name;

                            analysisResults.EndingVariablesAppearancesDict.Add(literal.Variable.Name, variableAppearances);
                        }

                        if (variableAppearances.FoundOnce == false)
                        {
                            variableAppearances.AppearancesPerVariable[columnIndex + 2] = 1;
                            variableAppearances.NormalAppearances.Add(pair.Variable.Name);
                        }
                        else
                        {
                            if (variableAppearances.FirstAppearanceSign == literal.Sign)
                            {
                                variableAppearances.AppearancesPerVariable[columnIndex + 2] = 1;
                                variableAppearances.NormalAppearances.Add("-" + pair.Variable.Name);
                            }
                            else
                            {
                                variableAppearances.AppearancesPerVariable[columnIndex + 3] = 1;
                                variableAppearances.ContrastAppearances.Add("-" + pair.Variable.Name);
                            }
                        }
                    }
                }
            }

            // create the datatable to display the results
            analysisResults.AppearancesDataTable = new DataTable();
            analysisResults.AppearancesDataTable.Columns.Add("", typeof(string));
            analysisResults.AppearancesDataTable.Columns.Add("", typeof(string));
            foreach (var column in analysisResults.VariablesColumns)
            {
                analysisResults.AppearancesDataTable.Columns.Add(column.Key.ToString() + "n", typeof(string));
                analysisResults.AppearancesDataTable.Columns.Add(column.Key.ToString() + "c", typeof(string));
                analysisResults.AppearancesDataTable.Columns.Add("-" + column.Key.ToString() + "n", typeof(string));
                analysisResults.AppearancesDataTable.Columns.Add("-" + column.Key.ToString() + "c", typeof(string));
            }

            foreach (var item in analysisResults.EndingVariablesAppearancesDict)
            {
                DataRow row = analysisResults.AppearancesDataTable.NewRow();
                string firstValue = item.Value.FirstAppearanceSign == Sign.Positive ? item.Key.ToString() : "-" + item.Key.ToString();
                string secondValue = item.Value.FirstAppearanceSign == Sign.Positive ? "-" + item.Key.ToString() : item.Key.ToString();

                row[0] = firstValue;
                row[1] = secondValue;

                int j = 2;
                foreach (var value in item.Value.AppearancesPerVariable)
                {
                    row[j] = value.ToString();
                    j++;
                }

                analysisResults.AppearancesDataTable.Rows.Add(row);
            }

            // create the array with the appearances
            foreach (var pair in analysisResults.VariablePairList)
            {
                foreach (var positiveClause in pair.ClausesWhenPositiveIsTrue)
                {
                    if (positiveClause.Literals.Count == 1)
                    {
                        Literal literal = positiveClause.Literals[0];

                        Conflict variableAppearances;
                        if (analysisResults.ConflictsDict.ContainsKey(literal.Variable))
                        {
                            variableAppearances = analysisResults.ConflictsDict[literal.Variable];
                        }
                        else
                        {
                            variableAppearances = new Conflict();
                            variableAppearances.Variable = literal.Variable;
                            analysisResults.ConflictsDict.Add(literal.Variable, variableAppearances);
                        }

                        variableAppearances.PositiveAppearances.Add(new Tuple<Variable, Sign>(pair.Variable, Sign.Positive));
                    }
                }

                foreach (var negativeClause in pair.ClausesWhenNegativeIsTrue)
                {
                    if (negativeClause.Literals.Count == 1)
                    {
                        Literal literal = negativeClause.Literals[0];

                        Conflict variableAppearances;
                        if (analysisResults.ConflictsDict.ContainsKey(literal.Variable))
                        {
                            variableAppearances = analysisResults.ConflictsDict[literal.Variable];
                        }
                        else
                        {
                            variableAppearances = new Conflict();
                            variableAppearances.Variable = literal.Variable;
                            analysisResults.ConflictsDict.Add(literal.Variable, variableAppearances);
                        }
                        variableAppearances.NegativeAppearances.Add(new Tuple<Variable, Sign>(pair.Variable, Sign.Negative));
                    }
                }
            }


            // create the problems list
            foreach (var item in analysisResults.ConflictsDict)
            {
                foreach (var positiveAppearance in item.Value.PositiveAppearances)
                {
                    foreach (var negativeAppearance in item.Value.NegativeAppearances)
                    {
                        string firstLiteral = positiveAppearance.Item2 == Sign.Positive ? positiveAppearance.Item1.Name : "-" + positiveAppearance.Item1.Name;
                        string secondLiteral = negativeAppearance.Item2 == Sign.Positive ? negativeAppearance.Item1.Name : "-" + negativeAppearance.Item1.Name;

                        string problem = $"When {firstLiteral}=A and {secondLiteral}=A then contrast at {item.Value.Variable.Name}";
                        analysisResults.ProblemsList.Add(problem);
                    }
                }
            }

            //foreach (var item in analysisResults.EndingVariablesAppearancesDict)
            //{
            //    foreach (var normalAppearance in item.Value.NormalAppearances)
            //    {
            //        foreach (var contrastAppearance in item.Value.ContrastAppearances)
            //        {
            //            string problem = $"When {normalAppearance}=A and {contrastAppearance}=A then contrast at {item.Value.VariableValue}";
            //            analysisResults.ProblemsList.Add(problem);
            //        }
            //    }
            //}

            return analysisResults;
        }


        #endregion
    }

    public class VariablePair
    {
        public Variable Variable { get; set; } = new Variable();
        public List<Clause> PositiveClauses { get; set; } = new List<Clause>();
        public List<Clause> NegativeClauses { get; set; } = new List<Clause>();
        public List<Clause> ClausesWhenPositiveIsTrue { get; set; } = new List<Clause>();
        public List<Clause> ClausesWhenNegativeIsTrue { get; set; } = new List<Clause>();
    }

    public class Conflict
    {
        public Variable Variable { get; set; }
        public List<Tuple<Variable, Sign>> PositiveAppearances = new List<Tuple<Variable, Sign>>();
        public List<Tuple<Variable, Sign>> NegativeAppearances = new List<Tuple<Variable, Sign>>();

    }

    public class EndingVariableAppearances
    {
        public string VariableValue { get; set; }
        public List<string> NormalAppearances = new List<string>();
        public List<string> ContrastAppearances = new List<string>();
        public bool FoundOnce { get; set; } = false;
        public Sign? FirstAppearanceSign { get; set; }
        public Sign? SecondAppearanceSign { get; set; }

        public int[] AppearancesPerVariable { get; set; }
    }
}
