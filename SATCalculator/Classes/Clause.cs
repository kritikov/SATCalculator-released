using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes {
    public class Clause {
        public Literal[] Literals { get; set; }
        public Trinity Trinity { get; set; }

        public Clause(Literal x1, Literal x2, Literal x3) {
            Literals = new Literal[3];
            Literals[0] = x1;
            Literals[1] = x2;
            Literals[2] = x3;
        }

        public string Name => $@"({Literals[0].Value} ∨ {Literals[1].Value} ∨ {Literals[2].Value})";

        public override string ToString() {
            return Name;
        }


    }
}
