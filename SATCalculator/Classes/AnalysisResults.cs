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

        public List<VariablePair> VariablePairList = new List<VariablePair>();

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

            // check the number of the variables per clause

            // get a list with the variables sorted by the contrasts?
            var variables = formula.VariablesDict.Select(p => p.Value).OrderByDescending(p => p.Contrasts).ToList();

            // create a list with the variables and their connected clauses
            foreach (var variable in variables)
            {
                VariablePair variablePair = new VariablePair();
                Variable variableParent = null;

                // get the clauses with the positive appearances, remove the positive appearances of the variable
                // and add the clauses in the proper list
                foreach (var clause in variable.ClausesWithPositiveAppearance)
                {
                    Clause positiveClause = new Clause();
                    Clause reducedClause = new Clause();

                    foreach(var literal in clause.Literals)
                    {
                        // create a clause based on the original clause
                        positiveClause.AddLiteral(new Literal(literal.Value));

                        if (literal.Variable != variable)
                        {
                            reducedClause.AddLiteral(new Literal(literal.Value));
                        }
                        else if (variableParent == null)
                        {
                            Literal removedLiteral = new Literal(literal.Value);
                            variableParent = removedLiteral.Variable;
                        }
                    }
                    variablePair.Variable = variableParent;
                    variablePair.PositiveClauses.Add(positiveClause);
                    variablePair.ClausesWhenNegativeIsTrue.Add(reducedClause);

                }

                // get the clauses with the negative appearances, remove the negative appearances of the variable
                // and add the clauses in the proper list
                foreach (var clause in variable.ClausesWithNegativeAppearance)
                {
                    Clause negativeClause = new Clause();
                    Clause reducedClause = new Clause();

                    foreach (var literal in clause.Literals)
                    {
                        // create a clause based on the original clause
                        negativeClause.AddLiteral(new Literal(literal.Value));

                        if (literal.Variable != variable)
                        {
                            reducedClause.AddLiteral(new Literal(literal.Value));
                        }
                        else if (variableParent == null)
                        {
                            Literal removedLiteral = new Literal(literal.Value);
                            variableParent = removedLiteral.Variable;
                        }
                    }
                    variablePair.Variable = variableParent;
                    variablePair.NegativeClauses.Add(negativeClause);
                    variablePair.ClausesWhenPositiveIsTrue.Add(reducedClause);
                }
                analysisResults.VariablePairList.Add(variablePair);
            }

            return analysisResults;
        }


        #endregion
    }

    public class VariablePair
    {
        public Variable Variable = new Variable();
        public List<Clause> PositiveClauses = new List<Clause>();
        public List<Clause> NegativeClauses = new List<Clause>();
        public List<Clause> ClausesWhenPositiveIsTrue = new List<Clause>();
        public List<Clause> ClausesWhenNegativeIsTrue = new List<Clause>();

        public VariablePair()
        {
            
        }
    }
}
