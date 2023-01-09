using System;
using System.Collections.Generic;
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
        public Dictionary<string, EndingVariablesAppearances> EndingVariablesAppearancesDict = new Dictionary<string, EndingVariablesAppearances>();

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
            //foreach(var pair in analysisResults.VariablePairList)
            //{
            //    EndingVariablesAppearances endingClausesAppearances = new EndingVariablesAppearances();
            //    endingClausesAppearances.Variable = pair.Variable;

            //    foreach(var item in pair.ClausesWhenPositiveIsTrue)
            //    {

            //    }
            //}
            

            //if (analysisResults.EndingVariablesAppearancesDict.ContainsKey(variable.Name))
            //{
            //    endingClausesAppearances = analysisResults.EndingVariablesAppearancesDict[variable.Name];
            //}
            //else
            //{
            //    endingClausesAppearances = new EndingVariablesAppearances();
            //    endingClausesAppearances.Variable = variableParent;
            //    analysisResults.EndingVariablesAppearancesDict[variable.Name] = endingClausesAppearances;
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

    public class EndingVariablesAppearances
    {
        public Variable Variable { get; set; } = new Variable();
        public Literal FirstAppearance { get; set; }
        public Literal SecondAppearance { get; set; }

        public List<int> AppearancesPerVariable { get; set; }
    }
}
