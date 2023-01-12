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

        public List<VariableSelectionStep> VariableSelectionStepsList { get; set; } = new List<VariableSelectionStep>();

        public int VariablesCount { get; set; } = 0;
        public Dictionary<Variable, EndVariableAppearances> EndVariablesDict = new Dictionary<Variable, EndVariableAppearances>();
        public DataTable ConflictsTable { get; set; }
        public DataTable EndVariableAppearancesTable { get; set; }
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
            int conflictTableColumnIndex = 0;

            AnalysisResults analysisResults = new AnalysisResults();
            analysisResults.VariablesCount = editFormula.VariablesCount;

            // check the number of the variables per clause

            // get a list with the variables sorted by the contrasts?
            var variablesSequence = editFormula.VariablesDict.Select(p => p.Value).OrderByDescending(p => p.Contrasts).ToList();

            #region CREATE A LIST WITH THE VARIABLES AND THEIR CONNECTED CLAUSES
            
            foreach (var variable in variablesSequence)
            {
                VariableSelectionStep variablePair = new VariableSelectionStep();
                variablePair.Variable = variable;

                if (conflictTableColumnIndex == 0)
                    conflictTableColumnIndex = 1;
                else
                    conflictTableColumnIndex = conflictTableColumnIndex + 2;

                variablePair.Variable.SequenceIndex = conflictTableColumnIndex;

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

                analysisResults.VariableSelectionStepsList.Add(variablePair);
            }

            #endregion

            #region CREATE THE LIST WITH THE END-VARIABLES APPEARANCES

            foreach (var pair in analysisResults.VariableSelectionStepsList)
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

            #endregion

            #region CREATE THE CONFLICTS DATATABLE TO DISPLAY THE RESULTS

            analysisResults.ConflictsTable = new DataTable();
            analysisResults.ConflictsTable.Columns.Add("", typeof(string));

            // create the columns
            foreach (var variable in variablesSequence)
            {
                analysisResults.ConflictsTable.Columns.Add(variable.Name, typeof(string), "");
                analysisResults.ConflictsTable.Columns.Add("-" + variable.Name, typeof(string), "");
            }

            // create the rows
            foreach (var variable in variablesSequence)
            {
                DataRow rowPos = analysisResults.ConflictsTable.NewRow();
                rowPos[0] = variable.Name;
                analysisResults.ConflictsTable.Rows.Add(rowPos);

                DataRow rowNeg = analysisResults.ConflictsTable.NewRow();
                rowNeg[0] = "-" + variable.Name;
                analysisResults.ConflictsTable.Rows.Add(rowNeg);
            }

            // fill the table
            foreach(var endVariable in analysisResults.EndVariablesDict)
            {
                foreach (var positiveAppearance in endVariable.Value.PositiveAppearances)
                {
                    int columnIndex = positiveAppearance.Item1.SequenceIndex;
                    if (positiveAppearance.Item2 == Sign.Negative)
                        columnIndex++;

                    foreach (var negativeAppearance in endVariable.Value.NegativeAppearances)
                    {
                        // forward
                        int rowIndex = negativeAppearance.Item1.SequenceIndex - 1;
                        if (negativeAppearance.Item2 == Sign.Negative)
                            rowIndex++;

                        DataRow row = analysisResults.ConflictsTable.Rows[rowIndex];
                        row[columnIndex] = "x";
                        if (positiveAppearance.Item2 == Sign.Negative)
                            row[columnIndex-1] = "v";
                        else
                            row[columnIndex+1] = "v";

                        // backward
                        int rowIndex2 = columnIndex - 1;
                        int columnIndex2 = rowIndex + 1;
                        row = analysisResults.ConflictsTable.Rows[rowIndex2];
                        row[columnIndex2] = "x";
                        if (negativeAppearance.Item2 == Sign.Negative)
                            row[columnIndex2 - 1] = "v";
                        else
                            row[columnIndex2 + 1] = "v";
                    }
                }
            }

            #endregion

            #region CREATE THE END VARIABLE APPEARANCES DATATABLE

            analysisResults.EndVariableAppearancesTable = new DataTable();
            analysisResults.EndVariableAppearancesTable.Columns.Add("", typeof(string));

            // create the columns
            foreach (var variable in variablesSequence)
            {
                analysisResults.EndVariableAppearancesTable.Columns.Add(variable.Name, typeof(string), "");
                analysisResults.EndVariableAppearancesTable.Columns.Add("-" + variable.Name, typeof(string), "");
            }

            // fill the table
            foreach (var endVariable in analysisResults.EndVariablesDict)
            {
                DataRow rowPos = analysisResults.EndVariableAppearancesTable.NewRow();
                rowPos[0] = endVariable.Value.Variable.Name;

                foreach (var positiveAppearance in endVariable.Value.PositiveAppearances)
                {
                    int columnIndex = positiveAppearance.Item1.SequenceIndex;
                    if (positiveAppearance.Item2 == Sign.Negative)
                        columnIndex++;

                    rowPos[columnIndex] = "+";
                }

                foreach (var negativeAppearance in endVariable.Value.NegativeAppearances)
                {
                    int columnIndex = negativeAppearance.Item1.SequenceIndex;
                    if (negativeAppearance.Item2 == Sign.Negative)
                        columnIndex++;

                    rowPos[columnIndex] = "-";
                }

                analysisResults.EndVariableAppearancesTable.Rows.Add(rowPos);
            }

            #endregion


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

    public class VariableSelectionStep
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
 
}
