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
using System.Windows.Shapes;

namespace SATCalculator
{
    /// <summary>
    /// Interaction logic for NewFormula.xaml
    /// </summary>
    public partial class NewFormulaWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region VARIABLES AND NESTED CLASSES

        private string message = "";
        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
            }
        }

        private string formulaString = "";
        public string FormulaString {
            get => formulaString;
            set
            {
                formulaString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
            } 
        }

        public List<string> FormulaCnfLines { get; set; } = new List<string>();


        #endregion

        
        #region CONSTRUCTORS

        public NewFormulaWindow()
        {
            InitializeComponent();
            DataContext = this;
        }


        #endregion


        #region EVENTS

        private void CreateFormula(object sender, RoutedEventArgs e)
        {
            if (FormatFormulaStringAsCnf())
                Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            FormulaString = "";
            Close();
        }

        #endregion


        #region METHODS

        private bool FormatFormulaStringAsCnf()
        {
            Message = "";
            //(1, 4)^(2, -4)^(-2, 5)^(1, -5)^(-1, 6)^(3,-6)^(-1,7)^(-7,-3)

            try
            {
                FormulaCnfLines = new List<string>();

                var clausesArray = FormulaString.Split('^');

                foreach (var clause in clausesArray)
                {
                    string newClause = clause.Trim();

                    // check if the clause starts end ends with parenthesis
                    if (!newClause.StartsWith("(") || !newClause.EndsWith(")"))
                    {
                        Message = "The formula has clauses without parenthesis and is not valid";
                        FormulaCnfLines.Clear();
                        return false;
                    }

                    newClause = newClause.Substring(1, newClause.Length-2);
                    var literalsArray = newClause.Split(',');

                    // check if the clause is empty
                    if (literalsArray.Length == 0)
                    {
                        Message = "The formula contains empty clause and is not valid";
                        FormulaCnfLines.Clear();
                        return false;
                    }

                    // create a cnf line with literals
                    string line = "";
                    foreach (var literal in literalsArray)
                    {
                        if (line != "")
                            line += " ";

                        string newLiteral = literal.Trim();
                        line += newLiteral;
                    }
                    line += " 0";

                    FormulaCnfLines.Add(line);
                }
            }
            catch(Exception ex)
            {
                Message = ex.Message;
            }

            return true;
        }

        #endregion
    }
}
