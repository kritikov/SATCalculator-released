using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes
{
    public class Solution
    {
        public ObservableCollection<VariableValuation> ValuationsList { get; set; } = new ObservableCollection<VariableValuation>();

        public string DisplayValue
        {
            get
            {
                string value = "";

                foreach (var variableValuation in ValuationsList)
                {
                    if (value != "")
                        value += ", ";

                    string valuation = variableValuation.Valuation == ValuationEnum.True ? "T" : "F";

                    value += $"{variableValuation.Variable.Name} = {valuation}";
                }

                return value;
            }
        }

        public override string ToString()
        {
            return DisplayValue;
        }
    }
}
