﻿using System;
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
        public Dictionary<string, Literal> LiteralsDict = new Dictionary<string, Literal>();
        public Dictionary<VariableSign, List<VariableSign>> ConflictsDict = new Dictionary<VariableSign, List<VariableSign>>();
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

            // create a dictionary to keep the literals
            foreach(var variable in variablesSequence)
            {
                Literal positiveLiteral = new Literal(variable, Sign.Positive);
                analysisResults.LiteralsDict.Add(positiveLiteral.Value, positiveLiteral);

                Literal negativeLiteral = new Literal(variable, Sign.Negative);
                analysisResults.LiteralsDict.Add(negativeLiteral.Value, negativeLiteral);
            }

            #region Create a list with the variables and their connected clauses

            foreach (var variable in variablesSequence)
            {
                VariableSelectionStep variableSelectionStep = new VariableSelectionStep();
                variableSelectionStep.Variable = variable;
                variableSelectionStep.PositiveLiteral = analysisResults.LiteralsDict[$"{variable.Name}"];
                variableSelectionStep.NegativeLiteral = analysisResults.LiteralsDict[$"-{variable.Name}"];

                if (conflictTableColumnIndex == 0)
                    conflictTableColumnIndex = 1;
                else
                    conflictTableColumnIndex = conflictTableColumnIndex + 2;

                variableSelectionStep.Variable.SequenceIndex = conflictTableColumnIndex;

                // get the clauses with the positive appearances, remove the positive appearances of the variable
                // and add the reviewed clauses in the proper list
                foreach (var clause in variable.ClausesWithPositiveAppearance)
                {
                    if (!clause.Used)
                    {
                        Clause positiveClause = new Clause();
                        Clause reducedClause = new Clause();

                        foreach (var literal in clause.Literals)
                        {
                            string literalValue = literal.Variable.Name;
                            if (literal.Sign == Sign.Negative)
                                literalValue = "-" + literalValue;
                            Literal newLiteral = analysisResults.LiteralsDict[literalValue];

                            if (literal.Variable != variable)
                            {
                                reducedClause.Literals.Add(newLiteral);
                                if (!reducedClause.Variables.ContainsKey(literal.Variable.Name))
                                    reducedClause.Variables.Add(literal.Variable.Name, literal.Variable);
                            }

                            positiveClause.AddLiteralSimple(newLiteral);
                        }

                        variableSelectionStep.ClausesWithPositiveAppearance.Add(positiveClause);
                        variableSelectionStep.ClausesWhenNegativeIsTrue.Add(reducedClause);
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
                            string literalValue = literal.Variable.Name;
                            if (literal.Sign == Sign.Negative)
                                literalValue = "-" + literalValue;
                            Literal newLiteral = newLiteral = analysisResults.LiteralsDict[literalValue];

                            if (literal.Variable != variable)
                            {
                                reducedClause.Literals.Add(newLiteral);
                                if (!reducedClause.Variables.ContainsKey(literal.Variable.Name))
                                    reducedClause.Variables.Add(literal.Variable.Name, literal.Variable);
                            }

                            negativeClause.AddLiteralSimple(newLiteral);
                        }

                        variableSelectionStep.ClausesWithNegativeAppearance.Add(negativeClause);
                        variableSelectionStep.ClausesWhenPositiveIsTrue.Add(reducedClause);
                        clause.Used = true;
                    }
                }

                if (variableSelectionStep.ClausesWhenPositiveIsTrue.Count == 0)
                {
                    Literal newLiteral = analysisResults.LiteralsDict[variable.Name];
                    Clause reducedClause = new Clause();
                    reducedClause.AddLiteralSimple(newLiteral);
                    variableSelectionStep.ClausesWhenPositiveIsTrue.Add(reducedClause);
                }

                if (variableSelectionStep.ClausesWhenNegativeIsTrue.Count == 0)
                {
                    Literal newLiteral = analysisResults.LiteralsDict[$"-{variable.Name}"];
                    Clause reducedClause = new Clause();
                    reducedClause.AddLiteralSimple(newLiteral);
                    variableSelectionStep.ClausesWhenNegativeIsTrue.Add(reducedClause);
                }

                analysisResults.VariableSelectionStepsList.Add(variableSelectionStep);
            }

            #endregion

            #region Create the list with the end-variables appearances

            foreach (var step in analysisResults.VariableSelectionStepsList)
            {
                foreach (var clauseWhenPositiveIsTrue in step.ClausesWhenPositiveIsTrue)
                {
                    if (clauseWhenPositiveIsTrue.Literals.Count == 1)
                    {
                        Literal literal = clauseWhenPositiveIsTrue.Literals[0];

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
                            endVariableAppearances.PositiveAppearances.Add(new VariableSign(step.Variable, Sign.Positive));
                        else
                            endVariableAppearances.NegativeAppearances.Add(new VariableSign(step.Variable, Sign.Positive));
                    }
                }

                foreach (var negativeClause in step.ClausesWhenNegativeIsTrue)
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
                            endVariableAppearances.PositiveAppearances.Add(new VariableSign(step.Variable, Sign.Negative));
                        else
                            endVariableAppearances.NegativeAppearances.Add(new VariableSign(step.Variable, Sign.Negative));
                    }
                }
            }

            #endregion

            #region Create the conflicts datatable to display the results

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
            foreach (var endVariable in analysisResults.EndVariablesDict)
            {
                foreach (var positiveAppearance in endVariable.Value.PositiveAppearances)
                {
                    int columnIndex = positiveAppearance.Variable.SequenceIndex;
                    if (positiveAppearance.Sign == Sign.Negative)
                        columnIndex++;

                    foreach (var negativeAppearance in endVariable.Value.NegativeAppearances)
                    {
                        // forward
                        int rowIndex = negativeAppearance.Variable.SequenceIndex - 1;
                        if (negativeAppearance.Sign == Sign.Negative)
                            rowIndex++;

                        DataRow row = analysisResults.ConflictsTable.Rows[rowIndex];
                        row[columnIndex] = "x";
                        if (positiveAppearance.Sign == Sign.Negative)
                            row[columnIndex - 1] = "v";
                        else
                            row[columnIndex + 1] = "v";

                        // backward
                        int rowIndex2 = columnIndex - 1;
                        int columnIndex2 = rowIndex + 1;
                        row = analysisResults.ConflictsTable.Rows[rowIndex2];
                        row[columnIndex2] = "x";
                        if (negativeAppearance.Sign == Sign.Negative)
                            row[columnIndex2 - 1] = "v";
                        else
                            row[columnIndex2 + 1] = "v";
                    }
                }
            }

            #endregion

            #region Create the end variable appearances datatable to display the results

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
                    int columnIndex = positiveAppearance.Variable.SequenceIndex;
                    if (positiveAppearance.Sign == Sign.Negative)
                        columnIndex++;

                    rowPos[columnIndex] = "+";
                }

                foreach (var negativeAppearance in endVariable.Value.NegativeAppearances)
                {
                    int columnIndex = negativeAppearance.Variable.SequenceIndex;
                    if (negativeAppearance.Sign == Sign.Negative)
                        columnIndex++;

                    rowPos[columnIndex] = "-";
                }

                analysisResults.EndVariableAppearancesTable.Rows.Add(rowPos);
            }

            #endregion


            // Create the problems list
            foreach (var item in analysisResults.EndVariablesDict)
            {
                foreach (var positiveAppearance in item.Value.PositiveAppearances)
                {
                    foreach (var negativeAppearance in item.Value.NegativeAppearances)
                    {
                        string firstLiteral = positiveAppearance.Sign == Sign.Positive ? positiveAppearance.Variable.Name : "-" + positiveAppearance.Variable.Name;
                        string secondLiteral = negativeAppearance.Sign == Sign.Positive ? negativeAppearance.Variable.Name : "-" + negativeAppearance.Variable.Name;

                        string problem = $"When {firstLiteral}=A and {secondLiteral}=A then contrast at {item.Value.Variable.Name}";
                        analysisResults.ProblemsList.Add(problem);
                    }
                }
            }

            #region Create the conflicts list

            //ConflictsDict
            //foreach (var item in analysisResults.EndVariablesDict)
            //{
            //    foreach (var positiveAppearance in item.Value.PositiveAppearances)
            //    {
            //        List<VariableSign> variableSignList1;
            //        if (analysisResults.ConflictsDict.ContainsKey(positiveAppearance))
            //        {
            //            variableSignList1 = analysisResults.ConflictsDict[positiveAppearance];
            //        }
            //        else
            //        {
            //            variableSignList1 = new List<VariableSign>();
            //            analysisResults.ConflictsDict.Add(positiveAppearance, variableSignList1);
            //        }

            //        foreach (var negativeAppearance in item.Value.NegativeAppearances)
            //        {
            //            List<VariableSign> variableSignList2;
            //            if (analysisResults.ConflictsDict.ContainsKey(negativeAppearance))
            //            {
            //                variableSignList2 = analysisResults.ConflictsDict[negativeAppearance];
            //            }
            //            else
            //            {
            //                variableSignList2 = new List<VariableSign>();
            //                analysisResults.ConflictsDict.Add(negativeAppearance, variableSignList2);
            //            }

            //            variableSignList1.Add(negativeAppearance);
            //            variableSignList2.Add(positiveAppearance);
            //        }
            //    }
            //}

            #endregion


            return analysisResults;
        }


        #endregion
    }

    public class VariableSelectionStep
    {
        public Variable Variable { get; set; } = new Variable();
        public Literal PositiveLiteral { get; set; }
        public Literal NegativeLiteral { get; set; }
        public List<Clause> ClausesWithPositiveAppearance { get; set; } = new List<Clause>();
        public List<Clause> ClausesWithNegativeAppearance { get; set; } = new List<Clause>();
        public List<Clause> ClausesWhenPositiveIsTrue { get; set; } = new List<Clause>();
        public List<Clause> ClausesWhenNegativeIsTrue { get; set; } = new List<Clause>();
    }

    public class EndVariableAppearances
    {
        public Variable Variable { get; set; }
        public List<VariableSign> PositiveAppearances = new List<VariableSign>();
        public List<VariableSign> NegativeAppearances = new List<VariableSign>();

        public List<Literal> PositiveLiteralAppearances = new List<Literal>();
        public List<Literal> NegativeLiteralAppearances = new List<Literal>();
    }

    public class VariableSign
    {
        public Variable Variable { get; set; }
        public Sign Sign { get; set; }

        public VariableSign(Variable variable, Sign sign)
        {
            Variable = variable;
            Sign = sign;
        }
    }
}
