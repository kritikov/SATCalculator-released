using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes
{
    public class Trinity
    {
        public Variable[] Variables { get; set; } = new Variable[3];
        public int References { get; set; }

        public Trinity(Clause clause)
        {
            Variables[0] = clause.Literals[0].Variable;
            Variables[1] = clause.Literals[1].Variable;
            Variables[2] = clause.Literals[2].Variable;

            References = 1;
        }

        public string Name => $"{Variables[0].Name}-{Variables[1].Name}-{Variables[2].Name}";

        public string Contents => $"{Variables[0].Name}, {Variables[1].Name}, {Variables[2].Name}";

        public override string ToString()
        {
            return Name;
        }
    }
}
