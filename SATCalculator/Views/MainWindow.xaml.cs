using Microsoft.Win32;
using SATCalculator.NewClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

namespace SATCalculator.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region VARIABLES AND NESTED CLASSES

        public class VariableValue
        {
            public ValuationEnum Value { get; set; }
            public string ValueAsString { get; set; }
        }

        public static List<VariableValue> VariableValues { get; set; } = new List<VariableValue>
        {
            new VariableValue(){Value = ValuationEnum.Null, ValueAsString="null" },
            new VariableValue(){Value = ValuationEnum.True, ValueAsString="true" },
            new VariableValue(){Value = ValuationEnum.False, ValueAsString="false" }
        };

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

        private SATFormula formulaOriginal;

        private SATFormula formula;
        public SATFormula Formula
        {
            get => formula;
            set
            {
                formula = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Formula"));
            }
        }

        private Variable selectedVariable;
        public Variable SelectedVariable
        {
            get => selectedVariable;
            set
            {
                selectedVariable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedVariable"));
            }
        }

        private ObservableCollection<Clause> editorResolutionResults = new ObservableCollection<Clause>();
        public ObservableCollection<Clause> EditorResolutionResults
        {
            get => editorResolutionResults;
            set
            {
                editorResolutionResults = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorResolutionResults"));
            }
        }


        #endregion


        #region VIEWS

        private CollectionViewSource logsSource = new CollectionViewSource();
        public ICollectionView LogsView
        {
            get
            {
                return this.logsSource.View;
            }
        }

        private readonly CollectionViewSource clausesSource = new CollectionViewSource();
        public ICollectionView ClausesView
        {
            get
            {
                return this.clausesSource.View;
            }
        }

        private readonly CollectionViewSource variablesSource = new CollectionViewSource();
        public ICollectionView VariablesView
        {
            get
            {
                return this.variablesSource.View;
            }
        }

        private readonly CollectionViewSource formulaRelatedClausesSource = new CollectionViewSource();
        public ICollectionView FormulaRelatedClausesView
        {
            get
            {
                return this.formulaRelatedClausesSource.View;
            }
        }

        private readonly CollectionViewSource formulaClausesSource = new CollectionViewSource();
        public ICollectionView FormulaClausesView
        {
            get
            {
                return this.formulaClausesSource.View;
            }
        }

        private readonly CollectionViewSource editorClausesWithReferencesSource = new CollectionViewSource();
        public ICollectionView EditorClausesWithReferencesView
        {
            get
            {
                return this.editorClausesWithReferencesSource.View;
            }
        }

        private readonly CollectionViewSource editorClausesWithPositiveReferencesSource = new CollectionViewSource();
        public ICollectionView EditorClausesWithPositiveReferencesView
        {
            get
            {
                return this.editorClausesWithPositiveReferencesSource.View;
            }
        }

        private readonly CollectionViewSource editorClausesWithNegativeReferencesSource = new CollectionViewSource();
        public ICollectionView EditorClausesWithNegativeReferencesView
        {
            get
            {
                return this.editorClausesWithNegativeReferencesSource.View;
            }
        }

        public CompositeCollection EditorClausesWithReferencesCollection { get; set; } = new CompositeCollection();

        #endregion


        #region CONSTRUCTORS

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            logsSource.Source = Logs.List;

            Logs.Write("Application started");
        }
        #endregion


        #region EVENTS

        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadFormula(object sender, RoutedEventArgs e)
        {
            LoadFormula();
        }

        private void NewFormula(object sender, RoutedEventArgs e)
        {
            CreateNewFormula();
        }

        private void SaveEditorFormulaAsCNF(object sender, RoutedEventArgs e)
        {
            SaveFormulaAsCNF(Formula);
        }

        private void ResetFormula(object sender, RoutedEventArgs e)
        {
            Formula = formulaOriginal.Copy();

            SelectedVariable = null;

            RefreshViews();
        }

        private void DeleteSelectedClause(object sender, RoutedEventArgs e)
        {
            if (ClausesView.CurrentItem != null)
            {
                var clause = ClausesView.CurrentItem as Clause;
                Formula.RemoveClause(clause);

                // create a new resolution formula
                Formula = Formula.Copy();

                RefreshViews();
            }
        }

        public void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            var control = (e.Source as ListView);
            //ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                    Sort(sortBy, control);
                }
            }
        }
        
        private void FormulaVariablesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Formula != null)
            {
                var grid = sender as DataGrid;

                if (grid.SelectedItem != null)
                {
                    var selectedItem = (KeyValuePair<string, Variable>)grid.SelectedItem;

                    SelectedVariable = selectedItem.Value;
                    if (SelectedVariable != null && FormulaRelatedClausesView != null && FormulaClausesView != null)
                    {
                        FormulaRelatedClausesView.Filter = RelatedClausesFilter;
                        FormulaClausesView.Refresh();
                    }
                }
            }
        }

        private void EditorVariablesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Formula != null)
            {
                var grid = sender as DataGrid;

                if (grid.SelectedItem != null)
                {
                    var selectedItem = (KeyValuePair<string, Variable>)grid.SelectedItem;

                    Formula.SelectedVariable = selectedItem.Value;
                    if (Formula.SelectedVariable != null)
                        RefreshEditorViews();
                }
            }
        }

        private void EditorSelectedClauseChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditorClausesWithPositiveReferencesView != null && EditorClausesWithNegativeReferencesView != null)
            {
                ResolutionSelectedClausesTest();
            }
        }

        private void EditorResolutionSelectedClauses(object sender, RoutedEventArgs e)
        {
            ResolutionSelectedClauses();
        }

        private void EditorResolutionAllClausesTest(object sender, RoutedEventArgs e)
        {
            ResolutionAllClausesTest();
        }

        private void EditorResolutionAllClauses(object sender, RoutedEventArgs e)
        {
            ResolutionAllClauses();
        }

        #endregion


        #region METHODS

        /// <summary>
        /// Sort a ListView by a field
        /// </summary>
        /// <param name="sortBy"></param>
        /// <param name="direction"></param>
        /// <param name="control"></param>
        private void Sort(string sortBy, ListView control)
        {
            try
            {
                ICollectionView dataView = CollectionViewSource.GetDefaultView(control.ItemsSource);

                ListSortDirection direction = ListSortDirection.Ascending;

                if (dataView.SortDescriptions.Count > 0)
                {
                    ListSortDirection oldDirection = dataView.SortDescriptions[0].Direction;
                    if (oldDirection == ListSortDirection.Descending)
                        direction = ListSortDirection.Ascending;
                    else
                        direction = ListSortDirection.Descending;
                }

                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void LoadFormula()
        {
            try
            {
                // Configure open file dialog box
                string path = AppDomain.CurrentDomain.BaseDirectory;
                path = "C:\\Users\\kritikov\\source\\repos\\SATCalculator\\SATCalculator\\Resources\\SAT3 formulas";

                var dialog = new OpenFileDialog();
                dialog.InitialDirectory = path;
                dialog.FileName = "Document"; // Default file name
                dialog.DefaultExt = ".cnf"; // Default file extension
                dialog.Filter = "SAT3 documents (.cnf)|*.cnf"; // Filter files by extension

                // Show open file dialog box
                bool? result = dialog.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    // Open document
                    string filename = dialog.FileName;

                    formulaOriginal = SATFormula.GetFromCnfFile(filename);
                    Formula = formulaOriginal.Copy();
                    SelectedVariable = null;

                    RefreshViews();
                }
                else
                {
                    Formula = new SATFormula();
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        /// <summary>
        /// Refresh all the views
        /// </summary>
        private void RefreshViews()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Formula"));

            clausesSource.Source = Formula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClausesView"));

            variablesSource.Source = Formula.VariablesDict;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VariablesView"));

            RefreshFormulaViews();
            RefreshEditorViews();
            //RefreshAlgorithmViews();
        }

        /// <summary>
        /// Refresh the formula tab views
        /// </summary>
        private void RefreshFormulaViews()
        {
            formulaClausesSource.Source = Formula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormulaClausesView"));

            formulaRelatedClausesSource.Source = Formula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormulaRelatedClausesView"));
        }

        /// <summary>
        /// Refresh the editor tab views
        /// </summary>
        private void RefreshEditorViews()
        {
            try
            {
                if (Formula.SelectedVariable != null)
                {
                    editorClausesWithPositiveReferencesSource.Source = Formula.SelectedVariable.PositiveLiteral.ClausesWithAppearances;
                    editorClausesWithNegativeReferencesSource.Source = Formula.SelectedVariable.NegativeLiteral.ClausesWithAppearances;

                    EditorClausesWithReferencesCollection.Clear();
                    EditorClausesWithReferencesCollection.Add(new CollectionContainer() { Collection = Formula.SelectedVariable.PositiveLiteral.ClausesWithAppearances });
                    EditorClausesWithReferencesCollection.Add(new CollectionContainer() { Collection = Formula.SelectedVariable.NegativeLiteral.ClausesWithAppearances });
                }
                else
                {
                    EditorClausesWithReferencesCollection.Clear();
                    editorClausesWithPositiveReferencesSource.Source = null;
                    editorClausesWithNegativeReferencesSource.Source = null;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesWithReferencesCollection"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesWithPositiveReferencesView"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesWithNegativeReferencesView"));
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        /// <summary>
        /// filter the related clauses view
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool RelatedClausesFilter(object item)
        {
            Clause clause = item as Clause;

            if (clause.Literals.Any(p => p.Variable == SelectedVariable))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Resolution the selected clauses in the positive and negative lists of the selected variable
        /// </summary>
        private void ResolutionSelectedClauses()
        {
            try
            {
                var positiveClause = EditorClausesWithPositiveReferencesView.CurrentItem as Clause;
                var negativeClause = EditorClausesWithNegativeReferencesView.CurrentItem as Clause;
                var selectedVariable = Formula.SelectedVariable;
                EditorResolutionResults.Clear();

                if (positiveClause != null && negativeClause != null)
                {
                    Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                    // remove old clauses from the formula
                    Formula.RemoveClause(positiveClause);
                    Formula.RemoveClause(negativeClause);

                    // add the new clause in the formula
                    Formula.AddClause(newClause);

                    // create a new resolution formula
                    Formula = Formula.Copy();

                    RefreshViews();
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        /// <summary>
        /// Resolution the selected clauses in the positive and negative lists of the selected variable
        /// without updating the formula 
        /// </summary>
        private void ResolutionSelectedClausesTest()
        {
            try
            {
                EditorResolutionResults.Clear();

                if (EditorClausesWithPositiveReferencesView.CurrentItem != null && EditorClausesWithNegativeReferencesView.CurrentItem != null)
                {
                    // get the selected items from the lists to apply resolution
                    var positiveClause = EditorClausesWithPositiveReferencesView.CurrentItem as Clause;
                    var negativeClause = EditorClausesWithNegativeReferencesView.CurrentItem as Clause;
                    var selectedVariable = Formula.SelectedVariable;

                    if (positiveClause != null && negativeClause != null)
                    {
                        Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                        // add the new clause to the results
                        EditorResolutionResults.Add(newClause);
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        /// <summary>
        /// ResolutioA all clauses in the positive and negative lists of the selected variables
        /// without updating the formula
        /// </summary>
        private void ResolutionAllClausesTest()
        {
            try
            {
                var selectedVariable = Formula.SelectedVariable;
                EditorResolutionResults.Clear();

                int pairsCount = selectedVariable.Contrasts;
                for (int i = 0; i < pairsCount; i++)
                {
                    var positiveClause = selectedVariable.PositiveLiteral.ClausesWithAppearances[i];
                    var negativeClause = selectedVariable.NegativeLiteral.ClausesWithAppearances[i];

                    if (positiveClause != null && negativeClause != null)
                    {
                        Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                        // add the new clause to the results
                        EditorResolutionResults.Add(newClause);
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        /// <summary>
        /// Resolution all clauses in the positive and negative lists of the selected variables
        /// </summary>
        private void ResolutionAllClauses()
        {
            try
            {
                var selectedVariable = Formula.SelectedVariable;
                EditorResolutionResults.Clear();

                int pairsCount = selectedVariable.Contrasts;
                for (int i = 0; i < pairsCount; i++)
                {
                    var positiveClause = selectedVariable.PositiveLiteral.ClausesWithAppearances[i];
                    var negativeClause = selectedVariable.NegativeLiteral.ClausesWithAppearances[i];

                    if (positiveClause != null && negativeClause != null)
                    {
                        Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                        // remove old clauses from the formula
                        Formula.Clauses.Remove(positiveClause);
                        Formula.Clauses.Remove(negativeClause);

                        // add the new clause in the formula
                        Formula.Clauses.Add(newClause);
                    }
                }

                // create a new resolution formula
                Formula = Formula.Copy();

                RefreshViews();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        /// <summary>
        /// Create a new formula
        /// </summary>
        private void CreateNewFormula()
        {
            NewFormulaWindow newFormulaWindow = new NewFormulaWindow();
            newFormulaWindow.ShowDialog();

            if (newFormulaWindow.FormulaCnfLines.Count > 0)
            {
                formulaOriginal = SATFormula.CreateFromCnfLines(newFormulaWindow.FormulaCnfLines);
                Formula = formulaOriginal.Copy();

                SelectedVariable = null;
                RefreshViews();
            }
        }

        /// <summary>
        /// Save a formula in a file at cnf format
        /// </summary>
        /// <param name="formula"></param>
        private void SaveFormulaAsCNF(SATFormula formula)
        {
            List<string> lines = formula.GetCNFLines();
            SaveLines(lines, "cnf");
        }

        /// <summary>
        /// Save a list of lines in a file
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="extension"></param>
        private void SaveLines(List<string> lines, string extension)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = $"{extension} file|*.{extension}";
                saveFileDialog.Title = $"Save a {extension} file";
                saveFileDialog.ShowDialog();

                // If the file name is not an empty string open it for saving.
                if (saveFileDialog.FileName != "")
                {
                    StreamWriter file = new StreamWriter($"{saveFileDialog.FileName}");

                    foreach (string line in lines)
                    {
                        file.WriteLine(line);
                    }

                    file.Close();
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }
        #endregion


    }
}
