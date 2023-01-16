using Microsoft.Win32;
using SATCalculator.Classes;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SATCalculator
{
    public class VariableValue
    {
        public VariableValueEnum Value { get; set; }
        public string ValueAsString { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region VARIABLES AND NESTED CLASSES

        public event PropertyChangedEventHandler PropertyChanged;

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

        public SATFormula AlgorithmFormula { get; set; } = new SATFormula();

        private AnalysisResults algorithmAnalysisResults = new AnalysisResults();
        public AnalysisResults AlgorithmAnalysisResults
        {
            get => algorithmAnalysisResults;
            set
            {
                algorithmAnalysisResults = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlgorithmAnalysisResults"));
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

        public static List<VariableValue> VariableValues { get; set; } = new List<VariableValue>
        {
            new VariableValue(){Value = VariableValueEnum.Null, ValueAsString="null" },
            new VariableValue(){Value = VariableValueEnum.True, ValueAsString="true" },
            new VariableValue(){Value = VariableValueEnum.False, ValueAsString="false" }
        };

        public bool ResolutionKeepTrueClauses { get; set; } = true;

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

        private readonly CollectionViewSource formulaRelatedClausesSource = new CollectionViewSource();
        public ICollectionView FormulaRelatedClausesView
        {
            get
            {
                return this.formulaRelatedClausesSource.View;
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

        private readonly CollectionViewSource algorithmFlowSource = new CollectionViewSource();
        public ICollectionView AlgorithmFlowView
        {
            get
            {
                return this.algorithmFlowSource.View;
            }
        }

        private readonly CollectionViewSource algorithmAppearancesSource = new CollectionViewSource();
        public ICollectionView AlgorithmAppearancesView
        {
            get
            {
                return this.algorithmAppearancesSource.View;
            }
        }

        private readonly CollectionViewSource algorithmConflictsSource = new CollectionViewSource();
        public ICollectionView AlgorithmConflictsView
        {
            get
            {
                return this.algorithmConflictsSource.View;
            }
        }

        private readonly CollectionViewSource algorithmProblemsSource = new CollectionViewSource();
        public ICollectionView AlgorithmProblemsView
        {
            get
            {
                return this.algorithmProblemsSource.View;
            }
        }

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

        private void NewFormula(object sender, RoutedEventArgs e)
        {
            CreateNewFormula();
        }

        private void LoadFormula(object sender, RoutedEventArgs e)
        {
            LoadFormula();
        }

        private void ResetFormula(object sender, RoutedEventArgs e)
        {
            Formula = formulaOriginal.CopyAsSATFormula();

            SelectedVariable = null;

            RefreshViews();
        }

        private void AnalyzeFormula(object sender, RoutedEventArgs e)
        {
            AnalyzeFormula(Formula);
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

                    //if (direction == ListSortDirection.Ascending)
                    //{
                    //    headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    //}
                    //else
                    //{
                    //    headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    //}

                    //// Remove arrow from previously sorted header
                    //if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    //{
                    //    _lastHeaderClicked.Column.HeaderTemplate = null;
                    //}
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

        private void DeleteSelectedClause(object sender, RoutedEventArgs e)
        {
            if (ClausesView.CurrentItem != null)
            {
                var clause = ClausesView.CurrentItem as Clause;
                Formula.Clauses.Remove(clause);

                Formula = Formula.CopyAsSATFormula();

                RefreshViews();
            }
        }

        private void SaveEditorFormulaAsCNF(object sender, RoutedEventArgs e)
        {
            SaveFormulaAsCNF(Formula);
        }
        
        #endregion


        #region METHODS

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
                    Formula = formulaOriginal.CopyAsSATFormula();
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
                Message = ex.Message;
            }
        }

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

            if (clause.Literals.Any(p=>p.Variable == SelectedVariable))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Refresh all the views
        /// </summary>
        private void RefreshViews()
        {
            clausesSource.Source = Formula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClausesView"));

            variablesSource.Source = Formula.VariablesDict;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VariablesView"));

            RefreshFormulaViews();
            RefreshEditorViews();
            RefreshAlgorithmViews();
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
                    editorClausesWithReferencesSource.Source = Formula.SelectedVariable.ClausesWithAppearance;
                    editorClausesWithPositiveReferencesSource.Source = Formula.SelectedVariable.ClausesWithPositiveAppearance;
                    editorClausesWithNegativeReferencesSource.Source = Formula.SelectedVariable.ClausesWithNegativeAppearance;
                }
                else
                {
                    editorClausesWithReferencesSource.Source = null;
                    editorClausesWithPositiveReferencesSource.Source = null;
                    editorClausesWithNegativeReferencesSource.Source = null;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesWithReferencesView"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesWithPositiveReferencesView"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesWithNegativeReferencesView"));
            }
            catch (Exception ex) {
                Message = ex.Message;
            }
        }

         /// <summary>
        /// Refresh the algorithm tab views
        /// </summary>
        private void RefreshAlgorithmViews()
        {
            algorithmFlowSource.Source = AlgorithmAnalysisResults.VariableSelectionStepsList;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlgorithmFlowView"));

            algorithmAppearancesSource.Source = AlgorithmAnalysisResults.EndVariableAppearancesTable;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlgorithmAppearancesView"));

            algorithmConflictsSource.Source = AlgorithmAnalysisResults.ConflictsTable;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlgorithmConflictsView"));

            algorithmProblemsSource.Source = AlgorithmAnalysisResults.ProblemsList;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlgorithmProblemsView"));
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
                    Formula.Clauses.Remove(positiveClause);
                    Formula.Clauses.Remove(negativeClause);

                    // add the new clause in the formula
                    if (newClause.Literals.Count > 0)
                        // if we dont keep the TRUE clauses in the formula then exit
                        if (newClause.Literals.Count == 1 && newClause.Literals[0].Value == "TRUE")
                        {
                            if (ResolutionKeepTrueClauses)
                            {
                                Formula.Clauses.Add(newClause);
                            }
                        }
                        else
                        {
                            Formula.Clauses.Add(newClause);
                        }


                    // create a new resolution formula
                    Formula = Formula.CopyAsSATFormula();

                    RefreshViews();
                }
            }
            catch (Exception ex)
            {
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
                        if (newClause.Literals.Count > 0)
                            EditorResolutionResults.Add(newClause);
                    }
                }
            }
            catch (Exception ex)
            {
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

                int pairsCount = Math.Min(selectedVariable.ClausesWithPositiveReferencesCount, selectedVariable.ClausesWithNegativeReferencesCount);
                for (int i = 0; i < pairsCount; i++)
                {
                    var positiveClause = selectedVariable.ClausesWithPositiveAppearance[i];
                    var negativeClause = selectedVariable.ClausesWithNegativeAppearance[i];

                    if (positiveClause != null && negativeClause != null)
                    {
                        Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                        // add the new clause to the results
                        if (newClause.Literals.Count > 0)
                            EditorResolutionResults.Add(newClause);
                    }
                }
            }
            catch (Exception ex)
            {
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
                    var positiveClause = selectedVariable.ClausesWithPositiveAppearance[i];
                    var negativeClause = selectedVariable.ClausesWithNegativeAppearance[i];

                    if (positiveClause != null && negativeClause != null)
                    {
                        Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                        // remove old clauses from the formula
                        Formula.Clauses.Remove(positiveClause);
                        Formula.Clauses.Remove(negativeClause);
                        if (newClause.Literals.Count > 0)
                            Formula.Clauses.Add(newClause);
                    }
                }

                // create a new resolution formula
                Formula = Formula.CopyAsSATFormula();

                RefreshViews();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
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
            catch(Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void CreateNewFormula()
        {
            NewFormulaWindow newFormulaWindow = new NewFormulaWindow();
            newFormulaWindow.ShowDialog();

            if (newFormulaWindow.FormulaCnfLines.Count > 0)
            {
                formulaOriginal = SATFormula.GetFromCnf(newFormulaWindow.FormulaCnfLines);
                Formula = formulaOriginal.CopyAsSATFormula();
                SelectedVariable = null;
                RefreshViews();
            }
        }

        /// <summary>
        /// Analyze a formula with the algorithm
        /// </summary>
        /// <param name="formula"></param>
        public void AnalyzeFormula(SATFormula formula)
        {
            AlgorithmAnalysisResults = AnalysisResults.Analyze(formula);

            RefreshAlgorithmViews();
        }
        #endregion

    }
}
