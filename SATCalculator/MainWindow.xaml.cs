using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SATCalculator.Classes;

namespace SATCalculator {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        #region VARIABLES AND NESTED CLASSES

        public event PropertyChangedEventHandler PropertyChanged;

        private string formulaString = "";
        public string FormulaString {
            get => formulaString;
            set {
                formulaString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormulaString"));
            }
        }

        private List<string> analysisResults = new List<string>();
        public List<string> AnalysisResults {
            get => analysisResults;
            set {
                analysisResults = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AnalysisResults"));
            }
        }

        public SAT3 Formula;

        #endregion


        #region CONSTRUCTORS

        public MainWindow() {
            InitializeComponent();

            this.DataContext = this;

            Formula = new SAT3();

            Formula.Add("x1", "x2", "x3");
            Formula.Add("x1'", "x4", "x3'");
            Formula.Add("x5", "x2'", "x7");
            Formula.Add("x1'", "x4", "x3'");
            Formula.Add("x3", "x2", "x1");

            FormulaString = Formula.ToString();
        }

        #endregion


        #region EVENTS

        private void AnalyzeFormula(object sender, RoutedEventArgs e) {
            AnalysisResults = Formula.Analyze();
        }

        #endregion

    }
}
