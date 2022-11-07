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

        public SAT3Formula Formula;

        #endregion


        #region CONSTRUCTORS

        public MainWindow() {
            InitializeComponent();

            this.DataContext = this;

            //Formula = new SAT3Formula();

            //Clause clause;

            //clause = new Clause(new Literal("x1", true), new Literal("x2", true), new Literal("x3", true));
            //Formula.Clauses.Add(clause);

            //clause = new Clause(new Literal("x1", false), new Literal("x4", true), new Literal("x3", true));
            //Formula.Clauses.Add(clause);

            //clause = new Clause(new Literal("x5", true), new Literal("x3", true), new Literal("x7", true));
            //Formula.Clauses.Add(clause);

            //clause = new Clause(new Literal("x1", true), new Literal("x4", false), new Literal("x3", true));
            //Formula.Clauses.Add(clause);

            //clause = new Clause(new Literal("x3", true), new Literal("x2", true), new Literal("x1", false));
            //Formula.Clauses.Add(clause);

            //FormulaString = Formula.ToString();
        }

        #endregion


        #region EVENTS

        private void AnalyzeFormula(object sender, RoutedEventArgs e) {
            //AnalysisResults = Formula.Analyze();
        }

        #endregion


        private void LoadFormula(object sender, RoutedEventArgs e) {

            // Configure open file dialog box
            string path = AppDomain.CurrentDomain.BaseDirectory;
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.InitialDirectory = path;
            dialog.FileName = "Document"; // Default file name
            dialog.DefaultExt = ".cnf"; // Default file extension
            dialog.Filter = "SAT3 documents (.cnf)|*.cnf"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true) {
                // Open document
                string filename = dialog.FileName;
                Formula = SAT3Formula.GetFromFile(filename);

                FormulaString = Formula.ToString();
            }
            else {
                // error
            }



        }
    }
}
