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

        private string formulaString = "(1 ∨ 4) ∧ (2 ∨ -4) ∧ (-2 ∨ 5) ∧ (1 ∨ -5) ∧ (-1 ∨ 6) ∧ (3 ∨ -6) ∧ (-1 ∨ 7) ∧ (-7 ∨ -3)";
        public string FormulaString {
            get => formulaString;
            set
            {
                formulaString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
            } 
        }

        private string andSymbol = "∧";
        public string AndSymbol {
            get => andSymbol; 
            set
            {
                if (AndSymbolIsValid(value))
                {
                    andSymbol = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AndSymbol"));
                }
            }
        }

        private string orSymbol = "∨";
        public string OrSymbol {
            get => orSymbol; 
            set
            {
                if (OrSymbolIsValid(value))
                {
                    andSymbol = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OrSymbol"));
                }
            }
        }

        public readonly string AndSymbolOriginal = "∧";
        public readonly string OrSymbolOriginal = "∨";

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
            if (!FormulaIsValid(FormulaString))
                return;

            string formulaFormatted = FormatFormulaString(FormulaString);

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

        private string FormatFormulaString(string formula)
        {
            string formulaFormatted = "";

            try
            {
                var clausesList = SplitFormulaString(formula);
                formulaFormatted = ComposeClauseList(clausesList);

            }
            catch(Exception ex)
            {
                Message = ex.Message;
            }

            return formulaFormatted;
        }

        /// <summary>
        /// Split a formula string to a list with components
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        private List<List<string>> SplitFormulaString(string formula)
        {
            List<List<string>> clausesList = new List<List<string>>();

            try
            {
                // create the clauses
                var clausesArray = formula.Split(AndSymbol[0]);

                // edit each clause
                foreach (var clause in clausesArray)
                {
                    // clear all parenthesis end empty spaces
                    string clauseFormatted = clause.Replace(" ", "");
                    clauseFormatted = clauseFormatted.Replace("(", "");
                    clauseFormatted = clauseFormatted.Replace(")", "");

                    // create the literals of the clause
                    var literals = clauseFormatted.Split(OrSymbol[0]).ToList();

                    // add the clause with its literals in the list
                    if (literals.Count > 0)
                        clausesList.Add(literals);
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }

            return clausesList;
        }

        private string ComposeClauseList(List<List<string>> clausesList)
        {
            string formula = "";

            foreach (var clause in clausesList)
            {
                if (formula != "")
                    formula += $" {AndSymbolOriginal} ";

                formula += "(";
                foreach (var literal in clause)
                {

                }
                formula += ")";
            }

            return formula;
        }

        private bool FormatFormulaStringAsCnf()
        {
            Message = "";
            //(1 ∨ 4) ∧ (2 ∨ -4) ∧ (-2 ∨ 5) ∧ (1 ∨ -5) ∧ (-1 ∨ 6) ∧ (3 ∨ -6) ∧ (-1 ∨ 7) ∧ (-7 ∨ -3)

            try
            {
                FormulaCnfLines = new List<string>();

                var clausesArray = FormulaString.Split(AndSymbol[0]);

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
                    var literalsArray = newClause.Split(OrSymbol[0]);

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

        private bool AndSymbolIsValid(string value)
        {
            if (value == OrSymbol)
            {
                Message = "The And symbol cannot be the same as the Or symbol";
                return false;
            }

            if (value == String.Empty)
            {
                Message = "The And symbol cannot be empty";
                return false;
            }

            return true;
        }

        private bool OrSymbolIsValid(string value)
        {
            if (value == AndSymbol)
            {
                Message = "The Or symbol cannot be the same as the And symbol";
                return false;
            }

            if (value == string.Empty)
            {
                Message = "The Or symbol cannot be empty";
                return false;
            }

            return true;
        }

        private bool FormulaIsValid(string formula)
        {
            if (formula == string.Empty)
            {
                Message = "The Or symbol cannot be empty";
                return false;
            }

            int leftParenthesisCount = formula.Count(f => (f == '('));
            int rightParenthesisCount = formula.Count(f => (f == ')'));
            if (leftParenthesisCount != rightParenthesisCount)
            {
                Message = "Some parenthesis are missing";
                return false;
            }

            return true;

        }

        #endregion
    }
}
