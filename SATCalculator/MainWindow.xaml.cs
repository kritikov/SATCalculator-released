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
using Microsoft.Win32;
using SATCalculator.Classes;

namespace SATCalculator {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        #region VARIABLES AND NESTED CLASSES

        public event PropertyChangedEventHandler PropertyChanged;

        private SATFormula formula;
        public SATFormula Formula {
            get => formula;
            set {
                formula = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Formula"));
            }
        }

        public SATFormula EditorFormula { get; set; } = new SATFormula();

        private readonly CollectionViewSource relatedClausesSource = new CollectionViewSource();
        public ICollectionView RelatedClausesView {
            get {
                return this.relatedClausesSource.View;
            }
        }

        private readonly CollectionViewSource clausesWithPositiveReferencesSource = new CollectionViewSource();
        public ICollectionView ClausesWithPositiveReferencesView {
            get {
                return this.clausesWithPositiveReferencesSource.View;
            }
        }

        private readonly CollectionViewSource clausesWithNegativeReferencesSource = new CollectionViewSource();
        public ICollectionView ClausesWithNegativeReferencesView {
            get {
                return this.clausesWithNegativeReferencesSource.View;
            }
        }

        private readonly CollectionViewSource relatedVariablesSource = new CollectionViewSource();
        public ICollectionView RelatedVariablesView
        {
            get
            {
                return this.relatedVariablesSource.View;
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

        private readonly CollectionViewSource clausesSource = new CollectionViewSource();
        public ICollectionView ClausesView
        {
            get
            {
                return this.clausesSource.View;
            }
        }

        private readonly CollectionViewSource editorVariablesSource = new CollectionViewSource();
        public ICollectionView EditorVariablesView
        {
            get
            {
                return this.editorVariablesSource.View;
            }
        }

        private readonly CollectionViewSource editorClausesSource = new CollectionViewSource();
        public ICollectionView EditorClausesView
        {
            get
            {
                return this.editorClausesSource.View;
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
        public Variable SelectedVariable {
            get => selectedVariable;
            set {
                selectedVariable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedVariable"));
            }
        }

        //private Variable selectedResolutionVariable;
        //public Variable SelectedResolutionVariable {
        //    get => selectedResolutionVariable;
        //    set {
        //        selectedResolutionVariable = value;
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedResolutionVariable"));
        //    }
        //}

        public class VariableValue
        {
            public VariableValueEnum Value { get; set; }
            public string ValueAsString { get; set; }
        }
        public static List<VariableValue> VariableValues { get; set; } = new List<VariableValue>
        {
            new VariableValue(){Value = VariableValueEnum.Null, ValueAsString="null" },
            new VariableValue(){Value = VariableValueEnum.True, ValueAsString="true" },
            new VariableValue(){Value = VariableValueEnum.False, ValueAsString="false" }
        };

        public bool ResolutionKeepTrueClauses { get; set; } = true;

        #endregion


        #region CONSTRUCTORS

        public MainWindow() {
            InitializeComponent();
            this.DataContext = this;
        }

        #endregion


        #region EVENTS

        private void AnalyzeFormula(object sender, RoutedEventArgs e) {
            //AnalysisResults = Formula.Analyze();
        }

        public void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            var control = (e.Source as ListView);
            ListSortDirection direction;

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

        private void RelatedClausesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Formula != null)
            {
                var grid = sender as DataGrid;

                if (grid.SelectedItem != null)
                {
                    var selectedItem = (KeyValuePair<string, Variable>)grid.SelectedItem;

                    SelectedVariable = selectedItem.Value;
                    if (SelectedVariable != null)
                    {
                        RelatedClausesView.Filter = RelatedClausesFilter;
                        RelatedVariablesView.Filter = RelatedVariablesFilter;

                        clausesWithPositiveReferencesSource.Source = SelectedVariable.ClausesWithPositiveAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClausesWithPositiveReferencesView"));

                        clausesWithNegativeReferencesSource.Source = SelectedVariable.ClausesWithNegativeAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClausesWithNegativeReferencesView"));

                    }
                }
            }
        }

        private void EditorRelatedClausesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (EditorFormula != null) {
                var grid = sender as DataGrid;

                if (grid.SelectedItem != null) {
                    var selectedItem = (KeyValuePair<string, Variable>)grid.SelectedItem;

                    EditorFormula.SelectedVariable= selectedItem.Value;
                    if (EditorFormula.SelectedVariable != null) {
                        editorClausesWithReferencesSource.Source = EditorFormula.SelectedVariable.ClausesWithAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesWithReferencesView"));

                        editorClausesWithPositiveReferencesSource.Source = EditorFormula.SelectedVariable.ClausesWithPositiveAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesWithPositiveReferencesView"));

                        editorClausesWithNegativeReferencesSource.Source = EditorFormula.SelectedVariable.ClausesWithNegativeAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesWithNegativeReferencesView"));
                    }
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

        private void EditorDeleteSelectedClauseFromFormula(object sender, RoutedEventArgs e)
        {
            if (EditorClausesView.CurrentItem != null)
            {
                // get the selected items from the lists to apply resolution
                var clause = EditorClausesView.CurrentItem as Clause;
                EditorFormula.Clauses.Remove(clause);

                // create a new resolution formula
                EditorFormula = EditorFormula.CopyAsSATFormula();

                editorClausesSource.Source = EditorFormula.Clauses;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesView"));

                editorVariablesSource.Source = EditorFormula.VariablesDict;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorVariablesView"));
            }
        }

        private void ResetEditorFormula(object sender, RoutedEventArgs e)
        {
            EditorFormula = Formula.CopyAsSATFormula();

            editorClausesSource.Source = EditorFormula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesView"));

            editorVariablesSource.Source = EditorFormula.VariablesDict;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorVariablesView"));
        }

        private void SaveEditorFormulaAsCNF(object sender, RoutedEventArgs e)
        {
            SaveFormulaAsCNF(EditorFormula);
        }
        #endregion


        #region METHODS

        private void LoadFormula(object sender, RoutedEventArgs e)
        {

            // Configure open file dialog box
            string path = AppDomain.CurrentDomain.BaseDirectory;
            path = "C:\\Users\\kritikov\\source\\repos\\SATCalculator\\SATCalculator\\Resources\\SAT3 formulas";

            var dialog = new Microsoft.Win32.OpenFileDialog();
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
                Formula = SATFormula.GetFromCnfFile(filename);
                EditorFormula = Formula.CopyAsSATFormula();

                // update the source of the views
                clausesSource.Source = Formula.Clauses;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClausesView"));

                relatedClausesSource.Source = Formula.Clauses;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RelatedClausesView"));

                SelectedVariable = null;

                relatedVariablesSource.Source = Formula.VariablesDict;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RelatedVariablesView"));

                variablesSource.Source = Formula.VariablesDict;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VariablesView"));

                RefreshEditorViews();
            }
            else
            {
                EditorFormula = new SATFormula();
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

        /// <summary>
        /// filter the related clauses view
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool RelatedClausesFilter(object item)
        {
            Clause clause = item as Clause;

            if (clause.Literals[0].Variable == SelectedVariable ||
                clause.Literals[1].Variable == SelectedVariable ||
                clause.Literals[2].Variable == SelectedVariable)
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// filter the related clauses view
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool RelatedVariablesFilter(object item)
        {

            Variable variable = ((KeyValuePair<string, Variable>)item).Value;
            var relatedClauses = RelatedClausesList.Items;

            foreach (Clause clause in relatedClauses)
            {
                if (clause.Literals[0].Variable == variable ||
                    clause.Literals[1].Variable == variable ||
                    clause.Literals[2].Variable == variable)
                {
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        /// Refresh the editor views
        /// </summary>
        private void RefreshEditorViews()
        {
            editorClausesSource.Source = EditorFormula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesView"));

            editorVariablesSource.Source = EditorFormula.VariablesDict;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorVariablesView"));
        }

        /// <summary>
        /// Resolution the selected clauses in the positive and negative lists of the selected variable
        /// </summary>
        private void ResolutionSelectedClauses()
        {
            var positiveClause = EditorClausesWithPositiveReferencesView.CurrentItem as Clause;
            var negativeClause = EditorClausesWithNegativeReferencesView.CurrentItem as Clause;
            var selectedVariable = EditorFormula.SelectedVariable;
            EditorResolutionResults.Clear();

            if (positiveClause != null && negativeClause != null)
            {
                Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                // remove old clauses from the formula
                EditorFormula.Clauses.Remove(positiveClause);
                EditorFormula.Clauses.Remove(negativeClause);

                // add the new clause in the formula
                if (newClause.Literals.Count > 0)
                    // if we dont keep the TRUE clauses in the formula then exit
                    if (newClause.Literals.Count == 1 && newClause.Literals[0].Value == "TRUE")
                    {
                        if (ResolutionKeepTrueClauses)
                        {
                            EditorFormula.Clauses.Add(newClause);
                        }
                    }
                    else
                    {
                        EditorFormula.Clauses.Add(newClause);
                    }


                // create a new resolution formula
                EditorFormula = EditorFormula.CopyAsSATFormula();

                RefreshEditorViews();
            }
        }

        /// <summary>
        /// Resolution the selected clauses in the positive and negative lists of the selected variable
        /// without updating the formula 
        /// </summary>
        private void ResolutionSelectedClausesTest()
        {
            EditorResolutionResults.Clear();

            if (EditorClausesWithPositiveReferencesView.CurrentItem != null && EditorClausesWithNegativeReferencesView.CurrentItem != null)
            {
                // get the selected items from the lists to apply resolution
                var positiveClause = EditorClausesWithPositiveReferencesView.CurrentItem as Clause;
                var negativeClause = EditorClausesWithNegativeReferencesView.CurrentItem as Clause;
                var selectedVariable = EditorFormula.SelectedVariable;

                if (positiveClause != null && negativeClause != null)
                {
                    Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                    // add the new clause to the results
                    if (newClause.Literals.Count > 0)
                        EditorResolutionResults.Add(newClause);
                }
            }
        }
 
        /// <summary>
        /// esolutionA all clauses in the positive and negative lists of the selected variables
        /// without updating the formula
        /// </summary>
        private void ResolutionAllClausesTest()
        {
            var selectedVariable = EditorFormula.SelectedVariable;
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

        /// <summary>
        /// Resolution all clauses in the positive and negative lists of the selected variables
        /// </summary>
        private void ResolutionAllClauses()
        {
            var selectedVariable = EditorFormula.SelectedVariable;
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
                    EditorFormula.Clauses.Remove(positiveClause);
                    EditorFormula.Clauses.Remove(negativeClause);
                    if (newClause.Literals.Count > 0)
                        EditorFormula.Clauses.Add(newClause);
                }
            }

            // create a new resolution formula
            EditorFormula = EditorFormula.CopyAsSATFormula();

            RefreshEditorViews();
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

        private List<Variable> GetAllowedVariables(SATFormula formula)
        {
            List<Variable> list = new List<Variable>();

            try
            {
                list = new List<Variable>
                    {
                        formula.VariablesDict["x1"],
                        formula.VariablesDict["x2"],
                        formula.VariablesDict["x3"],
                        formula.VariablesDict["x4"]
                    };
            }
            catch (Exception ex)
            {

            }
            return list;
        }



        #endregion


    }
}
