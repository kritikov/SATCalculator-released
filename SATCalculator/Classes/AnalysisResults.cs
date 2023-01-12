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
        public Dictionary<Variable, EndVariableAppearances> EndVariablesDict = new Dictionary<Variable, EndVariableAppearances>();
        public Dictionary<Variable, int> VariableIndexInSequenceDict = new Dictionary<Variable, int>();
        public DataTable AppearancesDataTable { get; set; }
        public DataTable ConflictsDataTable { get; set; }
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
        public static AnalysisResults Analyze(SATFormula Formula)
        {
            SATFormula editFormula = Formula.CopyAsSATFormula();
            int conflictTableIndex = 0;

            AnalysisResults analysisResults = new AnalysisResults();
            analysisResults.VariablesCount = editFormula.VariablesCount;

            // check the number of the variables per clause

            // get a list with the variables sorted by the contrasts?
            var variablesSequence = editFormula.VariablesDict.Select(p => p.Value).OrderByDescending(p => p.Contrasts).ToList();

            // reset the used flag on formula clauses
            foreach(var clause in editFormula.Clauses)
                clause.Used = false;

            // create a list with the variables and their connected clauses
            foreach (var variable in variablesSequence)
            {
                VariablePair variablePair = new VariablePair();
                variablePair.Variable = variable;
                conflictTableIndex = conflictTableIndex * 2 + 1;
                variablePair.Variable.ConflictTableIndex = conflictTableIndex;

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
                    variablePair.ClausesWhenPositiveIsTrue.Add(reducedClause);
                }

                if (variablePair.ClausesWhenNegativeIsTrue.Count == 0)
                {
                    Literal newLiteral = new Literal($"-{variable}");
                    newLiteral.Variable = variable;
                    Clause reducedClause = new Clause();
                    reducedClause.AddLiteralSimple(newLiteral);
                    variablePair.ClausesWhenNegativeIsTrue.Add(reducedClause);
                }

                analysisResults.VariablePairList.Add(variablePair);
            }

            // create the columns
            int i = 0;
            foreach (var variable in variablesSequence)
            {
                analysisResults.VariableIndexInSequenceDict[variable] = i++;
            }

            // OBSOLETE: create the array with the appearances
            // create the lines
            foreach (var pair in analysisResults.VariablePairList)
            {
                int columnIndex = analysisResults.VariableIndexInSequenceDict[pair.Variable] * 4;

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
            foreach (var column in analysisResults.VariableIndexInSequenceDict)
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


            // create the list with the end-variables appearances
            foreach (var pair in analysisResults.VariablePairList)
            {
                foreach (var positiveClause in pair.ClausesWhenPositiveIsTrue)
                {
                    if (positiveClause.Literals.Count == 1)
                    {
                        Literal literal = positiveClause.Literals[0];

                        EndVariableAppearances endVariableAppearances;
                        if (analysisResults.EndVariablesDict.ContainsKey(literal.Variable))
                        {
                            endVariableAppearances = analysisResults.EndVariablesDict[literal.Variable];
                        }
                        else
                        {
                            endVariableAppearances = new EndVariableAppearances();
                            endVariableAppearances.Variable = literal.Variable;
                            analysisResults.EndVariablesDict.Add(literal.Variable, endVariableAppearances);
                        }

                        if (literal.Sign == Sign.Positive)
                            endVariableAppearances.PositiveAppearances.Add(new Tuple<Variable, Sign>(pair.Variable, Sign.Positive));
                        else
                            endVariableAppearances.NegativeAppearances.Add(new Tuple<Variable, Sign>(pair.Variable, Sign.Positive));
                    }
                }

                foreach (var negativeClause in pair.ClausesWhenNegativeIsTrue)
                {
                    if (negativeClause.Literals.Count == 1)
                    {
                        Literal literal = negativeClause.Literals[0];

                        EndVariableAppearances endVariableAppearances;
                        if (analysisResults.EndVariablesDict.ContainsKey(literal.Variable))
                        {
                            endVariableAppearances = analysisResults.EndVariablesDict[literal.Variable];
                        }
                        else
                        {
                            endVariableAppearances = new EndVariableAppearances();
                            endVariableAppearances.Variable = literal.Variable;
                            analysisResults.EndVariablesDict.Add(literal.Variable, endVariableAppearances);
                        }
                        if (literal.Sign == Sign.Positive)
                            endVariableAppearances.PositiveAppearances.Add(new Tuple<Variable, Sign>(pair.Variable, Sign.Negative));
                        else
                            endVariableAppearances.NegativeAppearances.Add(new Tuple<Variable, Sign>(pair.Variable, Sign.Negative));
                    }
                }
            }

            // CREATE THE CONFLICTS DATATABLE TO DISPLAY THE RESULTS

            analysisResults.ConflictsDataTable = new DataTable();
            analysisResults.ConflictsDataTable.Columns.Add("", typeof(string));

            // create the columns
            foreach (var variable in variablesSequence)
            {
                analysisResults.ConflictsDataTable.Columns.Add(variable.Name, typeof(string), "");
                analysisResults.ConflictsDataTable.Columns.Add("-" + variable.Name, typeof(string), "");
            }

            // create the rows
            foreach (var variable in variablesSequence)
            {
                DataRow rowPos = analysisResults.ConflictsDataTable.NewRow();
                rowPos[0] = variable.Name;
                analysisResults.ConflictsDataTable.Rows.Add(rowPos);

                DataRow rowNeg = analysisResults.ConflictsDataTable.NewRow();
                rowNeg[0] = "-" + variable.Name;
                analysisResults.ConflictsDataTable.Rows.Add(rowNeg);
            }

            // fill the table
            foreach(var endVariable in analysisResults.EndVariablesDict)
            {
                DataRow row = analysisResults.ConflictsDataTable.Rows[endVariable.Value.Variable.ConflictTableIndex];

                foreach (var positiveAppearance in endVariable.Value.PositiveAppearances)
                {
                    foreach (var negativeAppearance in endVariable.Value.NegativeAppearances)
                    {
                        //string firstLiteral = positiveAppearance.Item2 == Sign.Positive ? positiveAppearance.Item1.Name : "-" + positiveAppearance.Item1.Name;
                        //string secondLiteral = negativeAppearance.Item2 == Sign.Positive ? negativeAppearance.Item1.Name : "-" + negativeAppearance.Item1.Name;

                        //string problem = $"When {firstLiteral}=A and {secondLiteral}=A then contrast at {endVariable.Value.Variable.Name}";
                        //analysisResults.ProblemsList.Add(problem);
                    }
                }

                
            }



            // CREATE THE PROBLEMS LIST
            foreach (var item in analysisResults.EndVariablesDict)
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

    public class EndVariableAppearances
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
