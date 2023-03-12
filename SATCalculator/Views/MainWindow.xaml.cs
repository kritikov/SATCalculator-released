using Microsoft.Win32;
using SATCalculator.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static SATCalculator.Views.NewClauseWindow;

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

        private bool SearchingValuationsRunning = false;
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

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

        private int selectedTab = 0;
        public int SelectedTab
        {
            get { return selectedTab; }
            set
            {
                selectedTab = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedTab"));
            }
        }

        private bool solverUpdateMessage = true;
        public bool SolverUpdateMessage
        {
            get { return solverUpdateMessage; }
            set
            {
                solverUpdateMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SolverUpdateMessage"));
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

        //private Solution selectedSolution = null;
        //public Solution SelectedSolution
        //{
        //    get => selectedSolution;
        //    set
        //    {
        //        selectedSolution = value;
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedSolution"));
        //    }
        //}

        private ObservableCollection<Clause> resolutionResolutionResults = new ObservableCollection<Clause>();
        public ObservableCollection<Clause> ResolutionResolutionResults
        {
            get => resolutionResolutionResults;
            set
            {
                resolutionResolutionResults = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionResolutionResults"));
            }
        }

        private SolverResults solverResults = new SolverResults();
        public SolverResults SolverResults
        {
            get => solverResults;
            set
            {
                solverResults = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SolverResults"));
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

        private readonly CollectionViewSource resolutionClausesWithReferencesSource = new CollectionViewSource();
        public ICollectionView ResolutionClausesWithReferencesView
        {
            get
            {
                return this.resolutionClausesWithReferencesSource.View;
            }
        }

        private readonly CollectionViewSource resolutionClausesWithPositiveReferencesSource = new CollectionViewSource();
        public ICollectionView ResolutionClausesWithPositiveReferencesView
        {
            get
            {
                return this.resolutionClausesWithPositiveReferencesSource.View;
            }
        }

        private readonly CollectionViewSource resolutionClausesWithNegativeReferencesSource = new CollectionViewSource();
        public ICollectionView ResolutionClausesWithNegativeReferencesView
        {
            get
            {
                return this.resolutionClausesWithNegativeReferencesSource.View;
            }
        }
        public CompositeCollection ResolutionClausesWithReferencesCollection { get; set; } = new CompositeCollection();

        private readonly CollectionViewSource solverSolutionsSource = new CollectionViewSource();
        public ICollectionView SolverSolutionsView
        {
            get
            {
                return this.solverSolutionsSource.View;
            }
        }

        private readonly CollectionViewSource solverSelectedSolutionSource = new CollectionViewSource();
        public ICollectionView SolverSelectedSolutionView
        {
            get
            {
                return this.solverSelectedSolutionSource.View;
            }
        }

        private readonly CollectionViewSource solverStatisticsSource = new CollectionViewSource();
        public ICollectionView SolverStatisticsView
        {
            get
            {
                return this.solverStatisticsSource.View;
            }
        }

        #endregion


        #region CONSTRUCTORS

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            logsSource.Source = Logs.List;

            CreateInitialFormula();

            Logs.Write("Application started");
        }

        #endregion


        #region EVENTS

        public event PropertyChangedEventHandler PropertyChanged;

        private void ExitProgram(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void DisplayAbout(object sender, RoutedEventArgs e)
        {
            DisplayAbout();
        }

        private void LoadFormula(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                LoadFormula();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void NewFormula(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                CreateNewFormula();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void SaveResolutionFormulaAsCNF(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                SaveFormulaAsCNF(Formula);
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        public void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
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
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }
        
        private void FormulaVariablesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Message = "";

            try
            {
                if (Formula != null)
                {
                    var grid = sender as DataGrid;

                    if (grid.SelectedItem != null)
                    {
                        var selectedVariable = (Variable)grid.SelectedItem;

                        SelectedVariable = selectedVariable;
                        if (SelectedVariable != null && FormulaRelatedClausesView != null && FormulaClausesView != null)
                        {
                            FormulaRelatedClausesView.Filter = RelatedClausesFilter;
                            FormulaClausesView.Refresh();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void ResolutionVariablesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Message = "";

            try
            {
                if (Formula != null)
                {
                    var grid = sender as DataGrid;

                    if (grid.SelectedItem != null)
                    {
                        var selectedVariable = (Variable)grid.SelectedItem;

                        Formula.SelectedVariable = selectedVariable;
                        if (Formula.SelectedVariable != null)
                            RefreshResolutionViews();
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void ResolutionSelectedClauseChanged(object sender, SelectionChangedEventArgs e)
        {
            Message = "";

            try
            {
                if (ResolutionClausesWithPositiveReferencesView != null && ResolutionClausesWithNegativeReferencesView != null)
                {
                    ResolutionSelectedClausesTest();
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void ResolutionResolutionSelectedClauses(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                ResolutionSelectedClauses();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void ResolutionResolutionAllClausesTest(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                ResolutionAllClausesTest();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void ResolutionResolutionAllClauses(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                ResolutionAllClauses();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void SolverSelectedSolutionChanged(object sender, SelectionChangedEventArgs e)
        {
            Message = "";

            try
            {
                if (SolverSolutionsView != null && SolverSolutionsView.CurrentItem != null)
                {
                    SolverResults.SelectedSolution = SolverSolutionsView.CurrentItem as Solution;
                    RefreshSolverViews();
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        #endregion


        #region COMMANDS

        private void NewFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;

        }
        private void NewFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                CreateNewFormula();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void LoadFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;

        }
        private void LoadFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                LoadFormula();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void SaveFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Formula != null ? true : false;

        }
        private void SaveFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                SaveFormulaAsCNF(Formula);
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void RemoveClause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = FormulaClausesView?.CurrentItem != null ? true : false;
        }
        private void RemoveClause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                if (FormulaClausesView.CurrentItem != null)
                {
                    var clause = FormulaClausesView.CurrentItem as Clause;
                    RemoveSelectedClause(clause);
                }
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void AddClause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Formula != null ? true : false;

        }
        private void AddClause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                AddNewClause();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void ResetFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Formula != null ? true : false;

        }
        private void ResetFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                Formula = formulaOriginal.Copy();
                SelectedVariable = null;
                SolverResults = null;
                RefreshViews();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void CopyFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Formula != null ? true : false;

        }
        private void CopyFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                Clipboard.SetText(Formula.Name);
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void SolveFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Formula != null && SearchingValuationsRunning == false) ? true : false;

        }
        private void SolveFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                SearchingValuationsRunning = true;

                SolveFormula(Formula);
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void StopSearchingValuations_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Formula != null && SearchingValuationsRunning == true) ? true : false;

        }
        private void StopSearchingValuations_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                cancellationToken.Cancel();
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
        }

        private void ApplyToFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Formula != null && SolverSolutionsView != null && SolverSolutionsView.CurrentItem != null) ? true : false;
        }
        private void ApplyToFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                ApplyValuationToFormula(SolverResults.SelectedSolution, Formula);
            }
            catch (Exception ex)
            {
                Logs.Write(ex.Message);
                Message = ex.Message;
            }
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
                throw ex;
            }
        }

        private void DisplayAbout()
        {
            AboutWindow window = new AboutWindow();
            window.ShowDialog();
        }

        private void LoadFormula()
        {
            try
            {
                // Configure open file dialog box
                string path = AppDomain.CurrentDomain.BaseDirectory + "Resources";

                var dialog = new OpenFileDialog();
                dialog.InitialDirectory = path;
                dialog.FileName = "file"; 
                dialog.DefaultExt = ".cnf";
                dialog.Filter = "SAT files (.cnf)|*.cnf";

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
                    SolverResults = null;

                    RefreshViews();
                }
                else
                {
                    Formula = new SATFormula();
                }
            }
            catch (Exception ex)
            {
                throw ex;
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

            variablesSource.Source = Formula.Variables;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VariablesView"));
            VariablesView?.SortDescriptions.Add(new SortDescription("CnfIndex", ListSortDirection.Ascending));
            VariablesView.Refresh();

            RefreshFormulaViews();
            RefreshResolutionViews();
            RefreshSolverViews();
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

        private void RefreshSolverViews()
        {
            solverSolutionsSource.Source = SolverResults?.Solutions;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SolverSolutionsView"));

            solverSelectedSolutionSource.Source = SolverResults?.SelectedSolution?.ValuationsList;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SolverSelectedSolutionView"));

            solverStatisticsSource.Source = SolverResults?.Statistics;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SolverStatisticsView"));

        }

        /// <summary>
        /// Refresh the Resolution tab views
        /// </summary>
        private void RefreshResolutionViews()
        {
            try
            {
                if (Formula.SelectedVariable != null)
                {
                    resolutionClausesWithPositiveReferencesSource.Source = Formula.SelectedVariable.PositiveLiteral.ClausesContainingIt;
                    resolutionClausesWithNegativeReferencesSource.Source = Formula.SelectedVariable.NegativeLiteral.ClausesContainingIt;

                    ResolutionClausesWithReferencesCollection.Clear();
                    ResolutionClausesWithReferencesCollection.Add(new CollectionContainer() { Collection = Formula.SelectedVariable.PositiveLiteral.ClausesContainingIt });
                    ResolutionClausesWithReferencesCollection.Add(new CollectionContainer() { Collection = Formula.SelectedVariable.NegativeLiteral.ClausesContainingIt });
                }
                else
                {
                    ResolutionClausesWithReferencesCollection.Clear();
                    resolutionClausesWithPositiveReferencesSource.Source = null;
                    resolutionClausesWithNegativeReferencesSource.Source = null;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesWithReferencesCollection"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesWithPositiveReferencesView"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesWithNegativeReferencesView"));
            }
            catch (Exception ex)
            {
                throw ex;
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
                var positiveClause = ResolutionClausesWithPositiveReferencesView.CurrentItem as Clause;
                var negativeClause = ResolutionClausesWithNegativeReferencesView.CurrentItem as Clause;
                var selectedVariable = Formula.SelectedVariable;
                ResolutionResolutionResults.Clear();

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
                throw ex;
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
                ResolutionResolutionResults.Clear();

                if (ResolutionClausesWithPositiveReferencesView.CurrentItem != null && ResolutionClausesWithNegativeReferencesView.CurrentItem != null)
                {
                    // get the selected items from the lists to apply resolution
                    var positiveClause = ResolutionClausesWithPositiveReferencesView.CurrentItem as Clause;
                    var negativeClause = ResolutionClausesWithNegativeReferencesView.CurrentItem as Clause;
                    var selectedVariable = Formula.SelectedVariable;

                    if (positiveClause != null && negativeClause != null)
                    {
                        Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                        // add the new clause to the results
                        ResolutionResolutionResults.Add(newClause);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
                ResolutionResolutionResults.Clear();

                int pairsCount = selectedVariable.Contrasts;
                for (int i = 0; i < pairsCount; i++)
                {
                    var positiveClause = selectedVariable.PositiveLiteral.ClausesContainingIt[i];
                    var negativeClause = selectedVariable.NegativeLiteral.ClausesContainingIt[i];

                    if (positiveClause != null && negativeClause != null)
                    {
                        Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                        // add the new clause to the results
                        ResolutionResolutionResults.Add(newClause);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
                ResolutionResolutionResults.Clear();

                int pairsCount = selectedVariable.Contrasts;
                for (int i = 0; i < pairsCount; i++)
                {
                    var positiveClause = selectedVariable.PositiveLiteral.ClausesContainingIt[i];
                    var negativeClause = selectedVariable.NegativeLiteral.ClausesContainingIt[i];

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
                throw ex;
            }
        }

        /// <summary>
        /// Create a new formula
        /// </summary>
        private void CreateNewFormula()
        {
            try
            {
                NewFormulaWindow newFormulaWindow = new NewFormulaWindow();
                newFormulaWindow.ShowDialog();

                if (newFormulaWindow.FormulaCnfLines.Count > 0)
                {
                    formulaOriginal = SATFormula.CreateFromCnfLines(newFormulaWindow.FormulaCnfLines);
                    Formula = formulaOriginal.Copy();

                    SelectedVariable = null;
                    SolverResults = null;
                    RefreshViews();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Create the initial formula to display as example
        /// </summary>
        private void CreateInitialFormula()
        {
            try
            {
                List<string> lines = new List<string>()
                {
                    "1 2 3 0",
                    "1 -2 3 0",
                    "1 2 -3 0",
                };

                formulaOriginal = SATFormula.CreateFromCnfLines(lines);
                Formula = formulaOriginal.Copy();

                SelectedVariable = null;
                SolverResults = null;
                RefreshViews();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Save a formula in a file at cnf format
        /// </summary>
        /// <param name="formula"></param>
        private void SaveFormulaAsCNF(SATFormula formula)
        {
            try
            {
                List<string> lines = formula.GetCNFLines();
                SaveLines(lines, "cnf");
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                throw ex;
            }
        }

        /// <summary>
        /// Delete the selected clause from the formula
        /// </summary>
        /// <param name="clause"></param>
        private void RemoveSelectedClause(Clause clause)
        {
            try
            {
                Formula.RemoveClause(clause);

                // create a new resolution formula
                Formula = Formula.Copy();

                RefreshViews();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Create a new formula
        /// </summary>
        private void AddNewClause()
        {
            try
            {
                NewClauseWindow newClauseWindow = new NewClauseWindow();
                newClauseWindow.ShowDialog();

                var lineParts = newClauseWindow.Literals;
                formulaOriginal.CreateAndAddClause(lineParts);
                Formula = formulaOriginal.Copy();
                SelectedVariable = null;

                RefreshViews();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Find the valuations that solve the formula
        /// </summary>
        /// <param name="formula"></param>
        private async Task SolveFormula(SATFormula formula)
        {
            try
            {
                SolverResults = new SolverResults();
                RefreshSolverViews();

                cancellationToken = new CancellationTokenSource();

                await Task.Run(() => {
                    SolverDeterministic.Solve(Formula, cancellationToken.Token, SolverResults, this);
                });

                SearchingValuationsRunning = false;
                RefreshSolverViews();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                SearchingValuationsRunning = false;
            }
        }

        private void ApplyValuationToFormula(Solution solution, SATFormula formula)
        {
            formula.ApplyValuation(solution);

            SelectedTab = 0;

            //RefreshFormulaViews();
        }

        #endregion

    }
}
