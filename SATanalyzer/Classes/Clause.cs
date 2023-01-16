using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SATanalyzer.Classes
{
    public class Clause
    {
        #region Fields

        public ObservableCollection<Literal> Literals { get; set; } = new ObservableCollection<Literal>();

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

        #endregion

    }
}
