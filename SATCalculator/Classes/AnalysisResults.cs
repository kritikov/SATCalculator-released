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
        public Dictionary<Variable, int> VariablesColumns = new Dictionary<Variable, int>();
        public string[,] AppearancesArray { get; set; }
        public DataTable AppearancesDataTable { get; set; }

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
            var variables = formula.VariablesDict.Select(p => p.Value).OrderByDescending(p => p.Contrasts).ToList();

            // reset the used flag on formula clauses
            foreach(var clause in formula.Clauses)
                clause.Used = false;

            // create a list with the variables and their connected clauses
            foreach (var variable in variables)
            {
                VariablePair variablePair = new VariablePair();
                Variable variableParent = new Variable();
                variableParent.CnfIndex = variable.CnfIndex;
                variablePair.Variable = variableParent;

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
                            Literal newLiteral = new Literal(literal.Value);

                            if (literal.Variable != variable)
                                reducedClause.AddLiteral(newLiteral);
                            else
                                newLiteral.Variable = variableParent;

                            positiveClause.AddLiteral(newLiteral);
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
                            Literal newLiteral = new Literal(literal.Value);

                            if (literal.Variable != variable)
                                reducedClause.AddLiteral(newLiteral);
                            else
                                newLiteral.Variable = variableParent;

                            negativeClause.AddLiteral(newLiteral);
                        }

                        variablePair.NegativeClauses.Add(negativeClause);
                        variablePair.ClausesWhenPositiveIsTrue.Add(reducedClause);
                        clause.Used = true;
                    }
                }

                if (variablePair.ClausesWhenPositiveIsTrue.Count == 0)
                {
                    Literal newLiteral = new Literal($"+{variableParent}");
                    newLiteral.Variable = variableParent;
                    Clause reducedClause = new Clause();
                    reducedClause.AddLiteral(newLiteral);
                    variablePair.ClausesWhenPositiveIsTrue.Add(reducedClause);
                }

                if (variablePair.ClausesWhenNegativeIsTrue.Count == 0)
                {
                    Literal newLiteral = new Literal($"-{variableParent}");
                    newLiteral.Variable = variableParent;
                    Clause reducedClause = new Clause();
                    reducedClause.AddLiteral(newLiteral);
                    variablePair.ClausesWhenNegativeIsTrue.Add(reducedClause);
                }

                analysisResults.VariablePairList.Add(variablePair);
            }

            // create the array with the appearances

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
                        }
                        else
                        {
                            if (variableAppearances.FirstAppearanceSign == literal.Sign)
                            {
                                variableAppearances.AppearancesPerVariable[columnIndex] = 1;
                            }
                            else
                            {
                                variableAppearances.AppearancesPerVariable[columnIndex + 1] = 1;
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
                        }
                        else
                        {
                            if (variableAppearances.FirstAppearanceSign == literal.Sign)
                            {
                                variableAppearances.AppearancesPerVariable[columnIndex + 2] = 1;
                            }
                            else
                            {
                                variableAppearances.AppearancesPerVariable[columnIndex + 3] = 1;
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

    public class EndingVariableAppearances
    {
        public string VariableValue { get; set; }
        public bool FoundOnce { get; set; } = false;
        public Sign? FirstAppearanceSign { get; set; }
        public Sign? SecondAppearanceSign { get; set; }

        public int[] AppearancesPerVariable { get; set; }
    }
}
