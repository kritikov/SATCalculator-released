using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SATCalculator.NewClasses
{
    public class Clause
    {
        #region Fields

        public List<Literal> Literals { get; set; } = new List<Literal>();

        public string Name
        {
            get
            {
                string name = "";
                foreach (var literal in Literals)
                {
                    if (name != "")
                        name += " ∨ ";

                    name += literal.Name;
                }

                return name;
            }
        }

        public string NameSorted
        {
            get
            {
                var literals = Literals.OrderBy(p => p.Variable.CnfIndex).Select(p => p.Name).ToArray();
                string keyText = string.Join(" ∨ ", literals);
                return keyText;
            }
        }

        public ValuationEnum Valuation
        {
            get
            {
                if (Literals.Any(p => p.Valuation == ValuationEnum.True))
                    return ValuationEnum.True;

                if (Literals.Count(p => p.Valuation == ValuationEnum.False) == Literals.Count)
                    return ValuationEnum.False;

                return ValuationEnum.Null;
            }
        }

        #endregion


        #region Constructors

        public Clause()
        {

        }


        #endregion


        #region Methods

        public override string ToString()
        {
            return Name;
        }

        /// return a clause from the resolution of two others 
        public static Clause Resolution(Variable variable, Clause positiveClause, Clause negativeClause)
        {
            Clause newClause = new Clause();

            try
            {
                // if the two clauses has one literal in contrast values then we have a contradiction
                if (positiveClause.Literals.Count == 1 && negativeClause.Literals.Count == 1 &&
                    positiveClause.Literals[0].Variable == variable &&
                    negativeClause.Literals[0].Variable == variable)
                {
                    newClause = new Clause();
                    newClause.Literals.Add(Variable.FixedVariable.NegativeLiteral);
                    return newClause;
                }

                // check the literals in the clause with the positive reference of the variable
                foreach (var literal in positiveClause.Literals)
                {
                    if (literal.Variable != variable)
                    {
                        // if the literal exists in the new clause but with opposite value
                        // then the clause is always true and can be discarded
                        if (newClause.Literals.Contains(literal.Opposite))
                        {
                            newClause = new Clause();
                            newClause.Literals.Add(Variable.FixedVariable.PositiveLiteral);
                            return newClause;
                        }

                        // if the literal doesnt allready exists in the new clause then add it
                        if (!newClause.Literals.Contains(literal))
                            newClause.Literals.Add(literal);
                    }
                }

                // check the literals in the clause with the negative reference of the variable
                foreach (var literal in negativeClause.Literals)
                {
                    if (literal.Variable != variable)
                    {
                        // if the literal exists in the new clause but with opposite value
                        // then the clause is always true and can be discarded
                        if (newClause.Literals.Contains(literal.Opposite))
                        {
                            newClause = new Clause();
                            newClause.Literals.Add(Variable.FixedVariable.PositiveLiteral);
                            return newClause;
                        }

                        // if the literal doesnt allready exists in the new clause then add it
                        if (!newClause.Literals.Contains(literal))
                            newClause.Literals.Add(literal);
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
            }

            return newClause;
        }
        #endregion

    }
}
