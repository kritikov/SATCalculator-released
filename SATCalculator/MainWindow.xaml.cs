using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private SAT3Formula formula;
        public SAT3Formula Formula {
            get => formula;
            set {
                formula = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Formula"));
            }
        }

        public Resolution Resolution { get; set; } = new Resolution();

        private SATFormula resolutionFormula;
        public SATFormula ResolutionFormula {
            get => resolutionFormula;
            set {
                resolutionFormula = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionFormula"));
            }
        }

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


        private Variable selectedVariable;
        public Variable SelectedVariable {
            get => selectedVariable;
            set {
                selectedVariable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedVariable"));
            }
        }

        private Variable selectedResolutionVariable;
        public Variable SelectedResolutionVariable {
            get => selectedResolutionVariable;
            set {
                selectedResolutionVariable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedResolutionVariable"));
            }
        }

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
            if (Resolution.Formula != null) {
                var grid = sender as DataGrid;

                if (grid.SelectedItem != null) {
                    var selectedItem = (KeyValuePair<string, Variable>)grid.SelectedItem;

                    Resolution.SelectedVariable= selectedItem.Value;
                    if (Resolution.SelectedVariable != null) {
                        resolutionClausesWithPositiveReferencesSource.Source = Resolution.SelectedVariable.ClausesWithPositiveAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesWithPositiveReferencesView"));

                        resolutionClausesWithNegativeReferencesSource.Source = Resolution.SelectedVariable.ClausesWithNegativeAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesWithNegativeReferencesView"));
                    }
                }
            }
        }

        private void SelectedClauseChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResolutionClausesWithPositiveReferencesView != null && ResolutionClausesWithNegativeReferencesView != null)
            {
                ResolutionSelectedClausesTest();
            }
        }

        #endregion

        #region METHODS

        private void LoadFormula(object sender, RoutedEventArgs e)
        {

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
            if (result == true)
            {
                // Open document
                string filename = dialog.FileName;
                Formula = SAT3Formula.GetFromFile(filename);
                Resolution.Formula = Formula.CopyAsSATFormula();

                // update the source of the views
                clausesSource.Source = Formula.Clauses;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClausesView"));

                relatedClausesSource.Source = Formula.Clauses;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RelatedClausesView"));

                SelectedVariable = null;

                relatedVariablesSource.Source = Formula.Variables;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RelatedVariablesView"));

                variablesSource.Source = Formula.Variables;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VariablesView"));

                resolutionVariablesSource.Source = Resolution.Formula.Variables;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionVariablesView"));

            }
            else
            {
                ResolutionFormula = new SATFormula();
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
            Resolution.Results.Clear();

            if (ResolutionClausesWithPositiveReferencesView.CurrentItem != null && ResolutionClausesWithNegativeReferencesView.CurrentItem != null)
            {
                // get the selected items from the lists to apply resolution
                var positiveClause = ResolutionClausesWithPositiveReferencesView.CurrentItem as Clause;
                var negativeClause = ResolutionClausesWithNegativeReferencesView.CurrentItem as Clause;
                var selectedVariable = Resolution.SelectedVariable;

                if (positiveClause != null && negativeClause != null)
                {
                    Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                    // add the new clause to the results
                    if (newClause.Literals.Count > 0)
                        Resolution.Results.Add(newClause);
                }
            }
        }

        private void ResolutionSelectedClauses(object sender, RoutedEventArgs e)
        {
            var positiveClause = ResolutionClausesWithPositiveReferencesView.CurrentItem as Clause;
            var negativeClause = ResolutionClausesWithNegativeReferencesView.CurrentItem as Clause;
            var selectedVariable = Resolution.SelectedVariable;
            Resolution.Results.Clear();

            if (positiveClause != null && negativeClause != null)
            {
                Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                // remove old clauses from the formula
                Resolution.Formula.Clauses.Remove(positiveClause);
                Resolution.Formula.Clauses.Remove(negativeClause);
                if (newClause.Literals.Count > 0)
                    Resolution.Formula.Clauses.Add(newClause);

                // create a new resolution formula
                Resolution.Formula = Resolution.Formula.CopyAsSATFormula();

                resolutionVariablesSource.Source = Resolution.Formula.Variables;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionVariablesView"));

            }
        }

        private void TestAllVariableClauses(object sender, RoutedEventArgs e)
        {
            var selectedVariable = Resolution.SelectedVariable;
            Resolution.Results.Clear();

            int pairsCount = Math.Min(selectedVariable.ClausesWithPositiveReferencesCount, selectedVariable.ClausesWithNegativeReferencesCount);
            for (int i=0; i < pairsCount; i++)
            {
                var positiveClause = selectedVariable.ClausesWithPositiveAppearance[i];
                var negativeClause = selectedVariable.ClausesWithNegativeAppearance[i];

                if (positiveClause != null && negativeClause != null) {
                    Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                    // add the new clause to the results
                    if (newClause.Literals.Count > 0)
                        Resolution.Results.Add(newClause);
                }
            }
        }

        private void ResolutionAllVariableClauses(object sender, RoutedEventArgs e)
        {
            var selectedVariable = Resolution.SelectedVariable;
            Resolution.Results.Clear();

            int pairsCount = Math.Min(selectedVariable.ClausesWithPositiveReferencesCount, selectedVariable.ClausesWithNegativeReferencesCount);
            for (int i = 0; i < pairsCount; i++)
            {
                var positiveClause = selectedVariable.ClausesWithPositiveAppearance[i];
                var negativeClause = selectedVariable.ClausesWithNegativeAppearance[i];

                if (positiveClause != null && negativeClause != null) {
                    Clause newClause = Clause.Resolution(selectedVariable, positiveClause, negativeClause);

                    // remove old clauses from the formula
                    Resolution.Formula.Clauses.Remove(positiveClause);
                    Resolution.Formula.Clauses.Remove(negativeClause);
                    if (newClause.Literals.Count > 0)
                        Resolution.Formula.Clauses.Add(newClause);
                }
            }

            // create a new resolution formula
            Resolution.Formula = Resolution.Formula.CopyAsSATFormula();

            resolutionVariablesSource.Source = Resolution.Formula.Variables;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionVariablesView"));
        }




        #endregion

       
    }
}
