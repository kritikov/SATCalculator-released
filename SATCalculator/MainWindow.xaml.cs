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

        private SAT3Formula formula;
        public SAT3Formula Formula {
            get => formula;
            set {
                formula = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Formula"));
            }
        }

        public SATFormula EditorFormula { get; set; } = new SATFormula();
        public SATFormula ResolutionFormula { get; set; } = new SATFormula();

        //public Resolution Resolution { get; set; } = new Resolution();

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

        private readonly CollectionViewSource resolutionVariablesSource = new CollectionViewSource();
        public ICollectionView ResolutionVariablesView {
            get {
                return this.resolutionVariablesSource.View;
            }
        }

        private readonly CollectionViewSource resolutionClausesSource = new CollectionViewSource();
        public ICollectionView ResolutionClausesView
        {
            get
            {
                return this.resolutionClausesSource.View;
            }
        }

        private readonly CollectionViewSource resolutionClausesWithPositiveReferencesSource = new CollectionViewSource();
        public ICollectionView ResolutionClausesWithPositiveReferencesView {
            get {
                return this.resolutionClausesWithPositiveReferencesSource.View;
            }
        }

        private readonly CollectionViewSource resolutionClausesWithNegativeReferencesSource = new CollectionViewSource();
        public ICollectionView ResolutionClausesWithNegativeReferencesView {
            get {
                return this.resolutionClausesWithNegativeReferencesSource.View;
            }
        }

        private ObservableCollection<Clause> resolutionResults = new ObservableCollection<Clause>();
        public ObservableCollection<Clause> ResolutionResults
        {
            get => resolutionResults;
            set
            {
                resolutionResults = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionResults"));
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

        private ObservableCollection<Clause> editorSimplificationResult = new ObservableCollection<Clause>();
        public ObservableCollection<Clause> EditorSimplificationResult
        {
            get => editorSimplificationResult;
            set
            {
                editorSimplificationResult = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorSimplificationResult"));
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

        private List<Variable> allowedVariables = new List<Variable>();

        public bool ResolutionKeepTrueClauses { get; set; } = true;
        public bool EditorDeleteDuplicateClausesAfterSimplification { get; set; } = true;
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

        private void ResolutionRelatedClausesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ResolutionFormula != null) {
                var grid = sender as DataGrid;

                if (grid.SelectedItem != null) {
                    var selectedItem = (KeyValuePair<string, Variable>)grid.SelectedItem;

                    ResolutionFormula.SelectedVariable= selectedItem.Value;
                    if (ResolutionFormula.SelectedVariable != null) {
                        resolutionClausesWithPositiveReferencesSource.Source = ResolutionFormula.SelectedVariable.ClausesWithPositiveAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesWithPositiveReferencesView"));

                        resolutionClausesWithNegativeReferencesSource.Source = ResolutionFormula.SelectedVariable.ClausesWithNegativeAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesWithNegativeReferencesView"));
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
                        editorClausesWithPositiveReferencesSource.Source = EditorFormula.SelectedVariable.ClausesWithPositiveAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesWithPositiveReferencesView"));

                        editorClausesWithNegativeReferencesSource.Source = EditorFormula.SelectedVariable.ClausesWithNegativeAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesWithNegativeReferencesView"));
                    }
                }
            }
        }

        private void ResolutionSelectedClauseChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResolutionClausesWithPositiveReferencesView != null && ResolutionClausesWithNegativeReferencesView != null)
            {
                ResolutionSelectedClausesTest();
            }
        }

        private void EditorSelectedClauseChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditorClausesWithPositiveReferencesView != null && EditorClausesWithNegativeReferencesView != null)
            {
                EditorSimplifyClausesTest();
            }
        }

        private void EditorSimplifySelectedClauses(object sender, RoutedEventArgs e)
        {
            EditorSimplifyClauses();
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

        private void ResetResolutionFormula(object sender, RoutedEventArgs e)
        {
            ResolutionFormula = Formula.CopyAsSATFormula();
            resolutionVariablesSource.Source = ResolutionFormula.VariablesDict;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionVariablesView"));
        }

        private void ResetEditorFormula(object sender, RoutedEventArgs e)
        {
            EditorFormula = Formula.CopyAsSATFormula();

            editorClausesSource.Source = EditorFormula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesView"));

            editorVariablesSource.Source = EditorFormula.VariablesDict;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorVariablesView"));
        }

        private void SaveResolutionFormulaAsSAT(object sender, RoutedEventArgs e)
        {
            SaveFormulaAsSAT(ResolutionFormula);
        }

        private void SaveEditorFormulaAsSAT(object sender, RoutedEventArgs e)
        {
            SaveFormulaAsSAT(EditorFormula);
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
                Formula = SAT3Formula.GetFromCnfFile(filename);
                ResolutionFormula = Formula.CopyAsSATFormula();
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

                resolutionClausesSource.Source = ResolutionFormula.Clauses;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesView"));

                resolutionVariablesSource.Source = ResolutionFormula.VariablesDict;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionVariablesView"));

                editorClausesSource.Source = EditorFormula.Clauses;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesView"));

                editorVariablesSource.Source = EditorFormula.VariablesDict;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorVariablesView"));

                allowedVariables = new List<Variable>
                {
                    EditorFormula.VariablesDict["x1"],
                    EditorFormula.VariablesDict["x2"],
                    EditorFormula.VariablesDict["x3"],
                    EditorFormula.VariablesDict["x4"]
                };

            }
            else
            {
                ResolutionFormula = new SATFormula();
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

        private void ResolutionSelectedClausesTest()
        {
            ResolutionResults.Clear();

            if (ResolutionClausesWithPositiveReferencesView.CurrentItem != null && ResolutionClausesWithNegativeReferencesView.CurrentItem != null)
            {
                // get the selected items from the lists to apply resolution
                var positiveClause = ResolutionClausesWithPositiveReferencesView.CurrentItem as Clause;
                var negativeClause = ResolutionClausesWithNegativeReferencesView.CurrentItem as Clause;
                var selectedVariable = ResolutionFormula.SelectedVariable;

                if (positiveClause != null && negativeClause != null)
                {
                    Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                    // add the new clause to the results
                    if (newClause.Literals.Count > 0)
                        ResolutionResults.Add(newClause);
                }
            }
        }

        private void EditorSimplifyClausesTest()
        {
            EditorSimplificationResult.Clear();

            if (EditorClausesWithPositiveReferencesView.CurrentItem != null && EditorClausesWithNegativeReferencesView.CurrentItem != null)
            {
                // get the selected items from the lists to apply resolution
                var positiveClause = EditorClausesWithPositiveReferencesView.CurrentItem as Clause;
                var negativeClause = EditorClausesWithNegativeReferencesView.CurrentItem as Clause;
                var selectedVariable = EditorFormula.SelectedVariable;

                if (positiveClause != null && negativeClause != null)
                {
                    var newClauses = Clause.Simplification(selectedVariable, positiveClause, negativeClause, allowedVariables);

                    EditorSimplificationResult.Add(newClauses.Item1);
                    EditorSimplificationResult.Add(newClauses.Item2);
                }
            }
        }

        private void EditorSimplifyClauses()
        {
            EditorSimplificationResult.Clear();

            if (EditorClausesWithPositiveReferencesView.CurrentItem != null && EditorClausesWithNegativeReferencesView.CurrentItem != null)
            {
                // get the selected items from the lists to apply resolution
                var positiveClause = EditorClausesWithPositiveReferencesView.CurrentItem as Clause;
                var negativeClause = EditorClausesWithNegativeReferencesView.CurrentItem as Clause;
                var selectedVariable = EditorFormula.SelectedVariable;

                if (positiveClause != null && negativeClause != null)
                {
                    var newClauses = Clause.Simplification(selectedVariable, positiveClause, negativeClause, allowedVariables);

                    // remove old clauses from the formula
                    EditorFormula.Clauses.Remove(positiveClause);
                    EditorFormula.VariablesPerClauseDict.Remove(positiveClause.VariablesCollection.Name);
                    EditorFormula.Clauses.Remove(negativeClause);
                    EditorFormula.VariablesPerClauseDict.Remove(negativeClause.VariablesCollection.Name);

                    //add new clauses to the formula
                    if (EditorDeleteDuplicateClausesAfterSimplification)
                    {
                        if (!EditorFormula.VariablesPerClauseDict.ContainsKey(newClauses.Item1.VariablesCollection.Name))
                            EditorFormula.Clauses.Add(newClauses.Item1);
                        if (!EditorFormula.VariablesPerClauseDict.ContainsKey(newClauses.Item2.VariablesCollection.Name))
                            EditorFormula.Clauses.Add(newClauses.Item2);
                    }
                    else
                    {
                        EditorFormula.Clauses.Add(newClauses.Item1);
                        EditorFormula.Clauses.Add(newClauses.Item2);
                    }

                    // create a new resolution formula
                    EditorFormula = EditorFormula.CopyAsSATFormula();

                    editorClausesSource.Source = EditorFormula.Clauses;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesView"));

                    editorVariablesSource.Source = EditorFormula.VariablesDict;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorVariablesView"));
                }
            }
        }

        private void EditorTestSimplifyAllClauses(object sender, RoutedEventArgs e)
        {
            var selectedVariable = EditorFormula.SelectedVariable;
            EditorSimplificationResult.Clear();

            int pairsCount = Math.Min(selectedVariable.ClausesWithPositiveReferencesCount, selectedVariable.ClausesWithNegativeReferencesCount);
            for (int i = 0; i < pairsCount; i++)
            {
                var positiveClause = selectedVariable.ClausesWithPositiveAppearance[i];
                var negativeClause = selectedVariable.ClausesWithNegativeAppearance[i];

                if (positiveClause != null && negativeClause != null)
                {
                    var newClauses = Clause.Simplification(selectedVariable, positiveClause, negativeClause, allowedVariables);

                    EditorSimplificationResult.Add(newClauses.Item1);
                    EditorSimplificationResult.Add(newClauses.Item2);
                }
            }
        }

        private void EditorSimplifyAllClauses(object sender, RoutedEventArgs e)
        {
            var selectedVariable = EditorFormula.SelectedVariable;
            EditorSimplificationResult.Clear();

            int pairsCount = Math.Min(selectedVariable.ClausesWithPositiveReferencesCount, selectedVariable.ClausesWithNegativeReferencesCount);
            for (int i = 0; i < pairsCount; i++)
            {
                var positiveClause = selectedVariable.ClausesWithPositiveAppearance[i];
                var negativeClause = selectedVariable.ClausesWithNegativeAppearance[i];

                if (positiveClause != null && negativeClause != null)
                {
                    var newClauses = Clause.Simplification(selectedVariable, positiveClause, negativeClause, allowedVariables);

                    // remove old clauses from the formula
                    EditorFormula.Clauses.Remove(positiveClause);
                    EditorFormula.VariablesPerClauseDict.Remove(positiveClause.VariablesCollection.Name);
                    EditorFormula.Clauses.Remove(negativeClause);
                    EditorFormula.VariablesPerClauseDict.Remove(negativeClause.VariablesCollection.Name);

                    //add new clauses to the formula
                    if (EditorDeleteDuplicateClausesAfterSimplification)
                    {
                        if (!EditorFormula.VariablesPerClauseDict.ContainsKey(newClauses.Item1.VariablesCollection.Name))
                            EditorFormula.Clauses.Add(newClauses.Item1);
                        if (!EditorFormula.VariablesPerClauseDict.ContainsKey(newClauses.Item2.VariablesCollection.Name))
                            EditorFormula.Clauses.Add(newClauses.Item2);
                    }
                    else
                    {
                        EditorFormula.Clauses.Add(newClauses.Item1);
                        EditorFormula.Clauses.Add(newClauses.Item2);
                    }

                }
            }

            // create a new resolution formula
            EditorFormula = EditorFormula.CopyAsSATFormula();

            editorClausesSource.Source = EditorFormula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorClausesView"));

            editorVariablesSource.Source = EditorFormula.VariablesDict;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorVariablesView"));
        }

        private void ResolutionSelectedClauses(object sender, RoutedEventArgs e)
        {
            var positiveClause = ResolutionClausesWithPositiveReferencesView.CurrentItem as Clause;
            var negativeClause = ResolutionClausesWithNegativeReferencesView.CurrentItem as Clause;
            var selectedVariable = ResolutionFormula.SelectedVariable;
            ResolutionResults.Clear();

            if (positiveClause != null && negativeClause != null)
            {
                Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                // remove old clauses from the formula
                ResolutionFormula.Clauses.Remove(positiveClause);
                ResolutionFormula.Clauses.Remove(negativeClause);

                // add the new clause in the formula
                if (newClause.Literals.Count > 0)
                    // if we dont keep the TRUE clauses in the formula then exit
                    if (newClause.Literals.Count == 1 && newClause.Literals[0].Value == "TRUE")
                    {
                        if (ResolutionKeepTrueClauses)
                        {
                            ResolutionFormula.Clauses.Add(newClause);
                        }
                    }
                    else
                    {
                        ResolutionFormula.Clauses.Add(newClause);
                    }
                        

                // create a new resolution formula
                ResolutionFormula = ResolutionFormula.CopyAsSATFormula();

                resolutionClausesSource.Source = ResolutionFormula.Clauses;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesView"));

                resolutionVariablesSource.Source = ResolutionFormula.VariablesDict;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionVariablesView"));

            }
        }

        private void ResolutionTestAllVariableClauses(object sender, RoutedEventArgs e)
        {
            var selectedVariable = ResolutionFormula.SelectedVariable;
            ResolutionResults.Clear();

            int pairsCount = Math.Min(selectedVariable.ClausesWithPositiveReferencesCount, selectedVariable.ClausesWithNegativeReferencesCount);
            for (int i=0; i < pairsCount; i++)
            {
                var positiveClause = selectedVariable.ClausesWithPositiveAppearance[i];
                var negativeClause = selectedVariable.ClausesWithNegativeAppearance[i];

                if (positiveClause != null && negativeClause != null) {
                    Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                    // add the new clause to the results
                    if (newClause.Literals.Count > 0)
                        ResolutionResults.Add(newClause);
                }
            }
        }

        private void ResolutionAllVariableClauses(object sender, RoutedEventArgs e)
        {
            var selectedVariable = ResolutionFormula.SelectedVariable;
            ResolutionResults.Clear();

            int pairsCount = Math.Min(selectedVariable.ClausesWithPositiveReferencesCount, selectedVariable.ClausesWithNegativeReferencesCount);
            for (int i = 0; i < pairsCount; i++)
            {
                var positiveClause = selectedVariable.ClausesWithPositiveAppearance[i];
                var negativeClause = selectedVariable.ClausesWithNegativeAppearance[i];

                if (positiveClause != null && negativeClause != null) {
                    Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                    // remove old clauses from the formula
                    ResolutionFormula.Clauses.Remove(positiveClause);
                    ResolutionFormula.Clauses.Remove(negativeClause);
                    if (newClause.Literals.Count > 0)
                        ResolutionFormula.Clauses.Add(newClause);
                }
            }

            // create a new resolution formula
            ResolutionFormula = ResolutionFormula.CopyAsSATFormula();

            resolutionClausesSource.Source = ResolutionFormula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesView"));

            resolutionVariablesSource.Source = ResolutionFormula.VariablesDict;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionVariablesView"));
        }

        /// <summary>
        /// Save a formula in a file at cnf format
        /// </summary>
        /// <param name="formula"></param>
        private void SaveFormulaAsSAT(SATFormula formula)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "sat file|*.sat";
            saveFileDialog.Title = "Save a sat file";
            saveFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog.FileName != "")
            {
                StreamWriter file = new StreamWriter($"{saveFileDialog.FileName}");

                List<string> cnfLines = formula.GetSATLines();

                foreach (string line in cnfLines)
                {
                    file.WriteLineAsync(line);
                }

                file.Close();
            }
        }





        #endregion

        
    }
}
