using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static SATCalculator.Classes.SAT3;

namespace SATCalculator.Classes
{
    public enum Sign { Positive, Nagetive };

    public class Literal
    {
        public string VariableName { get; set; }
        public bool IsPositive { get; set; }
        public string Value { 
            get
            {
                if (this.IsPositive)
                    return VariableName;
                else
                    return "-" + VariableName;
            }
        }

        public Literal(string name, bool isPositive)
        {
            VariableName = name;
            IsPositive = isPositive;
        }
    }

    public class Clause
    {
        public Literal[] Literals = new Literal[3];

        public Clause(Literal x1, Literal x2, Literal x3)
        {
            Literals[0] = x1;
            Literals[1] = x2;
            Literals[2] = x3;
        }

        public override string ToString()
        {
            string value = $@"({Literals[0].Value} ∨ {Literals[1].Value} ∨ {Literals[2].Value})";

            return value;
        }
    }

    public class SAT3Formula
    {

        public List<Clause> Clauses = new List<Clause>();
        public Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();

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

    }
}
