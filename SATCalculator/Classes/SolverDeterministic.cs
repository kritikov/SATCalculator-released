using SATCalculator.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SATCalculator.Classes
{
    public static class SolverDeterministic
    {
        public static void Solve(SATFormula formula, CancellationToken cancellationToken, SolverResults results, MainWindow window)
        {
            try
            {
                SATFormula formulaClone = formula.Copy();

                if (formulaClone.Variables.Count == 0)
                    throw new Exception("The formula has no variables");

                // initialize the valuations
                foreach (Variable variable in formulaClone.Variables)
                    variable.Valuation = ValuationEnum.False;


                // examine all combinations
                ulong checkedCombinations = 1;
                double totalCombinations = Math.Pow(2, formulaClone.Variables.Count);
                int counter = formulaClone.Variables.Count;
                bool continueLoop = true;
                while (continueLoop)
                {
                    if (checkedCombinations % 10000 == 0)
                    {
                        if (window.SolverUpdateMessage)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                window.Message = $"checking {checkedCombinations} valuations from a total of {totalCombinations}";
                            });
                        }
                    }

                    // check formula valuation
                    if (formulaClone.Valuation == ValuationEnum.True)
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            Solution solution = CreateSolution(formulaClone);
                            results.Solutions.Add(solution);
                        });
                    }

                    // stop the process if the user has cancel it
                    cancellationToken.ThrowIfCancellationRequested();

                    // go to the next valuation
                    int index = 0;
                    while (index <= counter - 1)
                    {
                        if (formulaClone.Variables[index].Valuation == ValuationEnum.False)
                        {
                            formulaClone.Variables[index].Valuation = ValuationEnum.True;
                            checkedCombinations++;
                            break;
                        }
                        else
                        {
                            formulaClone.Variables[index].Valuation = ValuationEnum.False;
                            index++;
                        }
                    }

                    // check if must end the loop
                    if (index == counter && formulaClone.Variables[index - 1].Valuation == ValuationEnum.False)
                        continueLoop = false;
                }

                Application.Current.Dispatcher.Invoke(() => {
                    results.Statistics.Add(new Statistics() { Name = "solutions", Value = results.Solutions.Count });
                    results.Statistics.Add(new Statistics() { Name = "total variables", Value = formula.VariablesCount });
                    window.Message = $"checked {checkedCombinations} valuations from a total of {totalCombinations}";
                });

                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Create a solution from the valuation of the variables of a formula
        /// </summary>
        /// <returns></returns>
        public static Solution CreateSolution(SATFormula formula)
        {
            Solution solution = new Solution();

            foreach (Variable variable in formula.Variables)
            {
                VariableValuation valuation = new VariableValuation() { Valuation = variable.Valuation, VariableName = variable.Name };
                solution.ValuationsList.Add(valuation);
            }

            return solution;
        }
    }

    public class SolverResults : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Solution> Solutions { get; set; } = new ObservableCollection<Solution>();

        public ObservableCollection<Statistics> Statistics { get; set; } = new ObservableCollection<Statistics>();

        public int SolutionsCount => Solutions.Count;

        private Solution selectedSolution = null;
        public Solution SelectedSolution
        {
            get => selectedSolution;
            set
            {
                selectedSolution = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedSolution"));
            }
        }
    }

    public class Statistics
    {
        public string Name { get; set; }
        public int Value { get; set; } = 0;

        public string DisplayValue
        {
            get
            {
                string value = $"{Name} = {Value}";

                return value;
            }
        }

        public override string ToString()
        {
            return DisplayValue;
        }
    }
}
