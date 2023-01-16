using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATanalyzer.Classes
{
    public class SATFormula : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        #region Fields

        public ObservableCollection<Clause> Clauses { get; set; } = new ObservableCollection<Clause>();
        public ObservableCollection<Variable> Variables { get; set; } = new ObservableCollection<Variable>();

        public Dictionary<string, Variable> VariablesDict { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, Clause> ClausesDict { get; set; } = new Dictionary<string, Clause>();

        public int ClausesCount => Clauses.Count;
        public int VariablesCount => Variables.Count;

        private Variable selectedVariable = null;
        public Variable SelectedVariable
        {
            get => selectedVariable;
            set
            {
                selectedVariable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedVariable"));
            }
        }

        public string Name
        {
            get{
                string value = "";

                foreach (Clause clause in Clauses)
                {
                    if (value != "")
                        value += " ^ ";

                    value += $"({clause})";
                }

                value += ")";

                return value;
            }
        }

        public string DisplayValue => this.ToString();

        #endregion


        #region Constructors

        public SATFormula()
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
