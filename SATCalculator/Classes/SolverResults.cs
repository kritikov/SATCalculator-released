using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SATCalculator.Classes
{
    public class SolverResults : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Solution> Solutions { get; set; } = new ObservableCollection<Solution>();

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
}
