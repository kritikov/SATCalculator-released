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

        public Reduction Reduction { get; set; } = new Reduction();

        private SATFormula reductionFormula;
        public SATFormula ReductionFormula {
            get => reductionFormula;
            set {
                reductionFormula = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReductionFormula"));
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

        private readonly CollectionViewSource reductionVariablesSource = new CollectionViewSource();
        public ICollectionView ReductionVariablesView {
            get {
                return this.reductionVariablesSource.View;
            }
        }

        private readonly CollectionViewSource reductionClausesWithPositiveReferencesSource = new CollectionViewSource();
        public ICollectionView ReductionClausesWithPositiveReferencesView {
            get {
                return this.reductionClausesWithPositiveReferencesSource.View;
            }
        }

        private readonly CollectionViewSource reductionClausesWithNegativeReferencesSource = new CollectionViewSource();
        public ICollectionView ReductionClausesWithNegativeReferencesView {
            get {
                return this.reductionClausesWithNegativeReferencesSource.View;
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

        private Variable selectedReductionVariable;
        public Variable SelectedReductionVariable {
            get => selectedReductionVariable;
            set {
                selectedReductionVariable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedReductionVariable"));
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

        private void ReductionRelatedClausesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Reduction.Formula != null) {
                var grid = sender as DataGrid;

                if (grid.SelectedItem != null) {
                    var selectedItem = (KeyValuePair<string, Variable>)grid.SelectedItem;

                    Reduction.SelectedVariable= selectedItem.Value;
                    if (Reduction.SelectedVariable != null) {
                        reductionClausesWithPositiveReferencesSource.Source = Reduction.SelectedVariable.ClausesWithPositiveAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReductionClausesWithPositiveReferencesView"));

                        reductionClausesWithNegativeReferencesSource.Source = Reduction.SelectedVariable.ClausesWithNegativeAppearance;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReductionClausesWithNegativeReferencesView"));
                    }
                }
            }
        }

        
        private void TestReductionSelectedClauses(object sender, RoutedEventArgs e)
        {
            var positiveClause = ReductionClausesWithPositiveReferencesView.CurrentItem as Clause;
            var negativeClause = ReductionClausesWithNegativeReferencesView.CurrentItem as Clause;
            var selectedVariable = Reduction.SelectedVariable;
            Reduction.Results.Clear();

            if (positiveClause != null && negativeClause != null)
            {
                Clause newClause = new Clause();

                // combine the selected clauses into a new clause
                foreach (var literal in positiveClause.Literals)
                {
                    if (literal.Variable != selectedVariable)
                    {
                        Literal existingLiteral = newClause.Literals.Where(p => p.Variable == literal.Variable).FirstOrDefault();
                        if (existingLiteral == null)
                        {
                            newClause.Literals.Add(literal);
                        }
                        else
                        {
                            if (existingLiteral.IsPositive != literal.IsPositive)
                            {
                                newClause.Literals.Remove(existingLiteral);
                            }
                        }
                    }
                }
                foreach (var literal in negativeClause.Literals)
                {
                    if (literal.Variable != selectedVariable)
                    {
                        Literal existingLiteral = newClause.Literals.Where(p => p.Variable == literal.Variable).FirstOrDefault();
                        if (existingLiteral == null)
                        {
                            newClause.Literals.Add(literal);
                        }
                        else
                        {
                            if (existingLiteral.IsPositive != literal.IsPositive)
                            {
                                newClause.Literals.Remove(existingLiteral);
                            }
                        }
                    }
                }

                // add the new clause to the results
                if (newClause.Literals.Count>0)
                    Reduction.Results.Add(newClause);
            }
        }

        private void ReductionSelectedClauses(object sender, RoutedEventArgs e)
        {
            var positiveClause = ReductionClausesWithPositiveReferencesView.CurrentItem as Clause;
            var negativeClause = ReductionClausesWithNegativeReferencesView.CurrentItem as Clause;
            var selectedVariable = Reduction.SelectedVariable;
            Reduction.Results.Clear();

            if (positiveClause != null && negativeClause != null)
            {
                Clause newClause = new Clause();

                // combine the selected clauses into a new clause
                foreach (var literal in positiveClause.Literals)
                {
                    if (literal.Variable != selectedVariable)
                    {
                        Literal existingLiteral = newClause.Literals.Where(p => p.Variable == literal.Variable).FirstOrDefault();
                        if (existingLiteral == null)
                        {
                            newClause.Literals.Add(literal);
                        }
                        else
                        {
                            if (existingLiteral.IsPositive != literal.IsPositive)
                            {
                                newClause.Literals.Remove(existingLiteral);
                            }
                        }
                    }
                }
                foreach (var literal in negativeClause.Literals)
                {
                    if (literal.Variable != selectedVariable)
                    {
                        Literal existingLiteral = newClause.Literals.Where(p => p.Variable == literal.Variable).FirstOrDefault();
                        if (existingLiteral == null)
                        {
                            newClause.Literals.Add(literal);
                        }
                        else
                        {
                            if (existingLiteral.IsPositive != literal.IsPositive)
                            {
                                newClause.Literals.Remove(existingLiteral);
                            }
                        }
                    }
                }

                // remove old clauses from the formula
                Reduction.Formula.Clauses.Remove(positiveClause);
                Reduction.Formula.Clauses.Remove(negativeClause);
                if (newClause.Literals.Count > 0)
                    Reduction.Results.Add(newClause);

                // create a new reduction formula
                Reduction.Formula = Reduction.Formula.CopyAsSATFormula();

                reductionVariablesSource.Source = Reduction.Formula.Variables;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReductionVariablesView"));

            }
        }

        private void TestAllVariableClauses(object sender, RoutedEventArgs e)
        {
            var selectedVariable = Reduction.SelectedVariable;
            Reduction.Results.Clear();

            int pairsCount = Math.Min(selectedVariable.ClausesWithPositiveReferencesCount, selectedVariable.ClausesWithNegativeReferencesCount);
            for (int i=0; i < pairsCount; i++)
            {
                var positiveClause = selectedVariable.ClausesWithPositiveAppearance[i];
                var negativeClause = selectedVariable.ClausesWithNegativeAppearance[i];

                if (positiveClause != null && negativeClause != null)
                {
                    Clause newClause = new Clause();

                    // combine the selected clauses into a new clause
                    foreach (var literal in positiveClause.Literals)
                    {
                        if (literal.Variable != selectedVariable)
                        {
                            Literal existingLiteral = newClause.Literals.Where(p => p.Variable == literal.Variable).FirstOrDefault();
                            if (existingLiteral == null)
                            {
                                newClause.Literals.Add(literal);
                            }
                            else
                            {
                                if (existingLiteral.IsPositive != literal.IsPositive)
                                {
                                    newClause.Literals.Remove(existingLiteral);
                                }
                            }
                        }
                    }
                    foreach (var literal in negativeClause.Literals)
                    {
                        if (literal.Variable != selectedVariable)
                        {
                            Literal existingLiteral = newClause.Literals.Where(p => p.Variable == literal.Variable).FirstOrDefault();
                            if (existingLiteral == null)
                            {
                                newClause.Literals.Add(literal);
                            }
                            else
                            {
                                if (existingLiteral.IsPositive != literal.IsPositive)
                                {
                                    newClause.Literals.Remove(existingLiteral);
                                }
                            }
                        }
                    }

                    // add the new clause to the results
                    if (newClause.Literals.Count > 0)
                        Reduction.Results.Add(newClause);
                }
            }
        }

        private void ReductionAllVariableClauses(object sender, RoutedEventArgs e)
        {
            var selectedVariable = Reduction.SelectedVariable;
            Reduction.Results.Clear();

            int pairsCount = Math.Min(selectedVariable.ClausesWithPositiveReferencesCount, selectedVariable.ClausesWithNegativeReferencesCount);
            for (int i = 0; i < pairsCount; i++)
            {
                var positiveClause = selectedVariable.ClausesWithPositiveAppearance[i];
                var negativeClause = selectedVariable.ClausesWithNegativeAppearance[i];

                if (positiveClause != null && negativeClause != null)
                {
                    Clause newClause = new Clause();

                    // combine the selected clauses into a new clause
                    foreach (var literal in positiveClause.Literals)
                    {
                        if (literal.Variable != selectedVariable)
                        {
                            Literal existingLiteral = newClause.Literals.Where(p => p.Variable == literal.Variable).FirstOrDefault();
                            if (existingLiteral == null)
                            {
                                newClause.Literals.Add(literal);
                            }
                            else
                            {
                                if (existingLiteral.IsPositive != literal.IsPositive)
                                {
                                    newClause.Literals.Remove(existingLiteral);
                                }
                            }
                        }
                    }
                    foreach (var literal in negativeClause.Literals)
                    {
                        if (literal.Variable != selectedVariable)
                        {
                            Literal existingLiteral = newClause.Literals.Where(p => p.Variable == literal.Variable).FirstOrDefault();
                            if (existingLiteral == null)
                            {
                                newClause.Literals.Add(literal);
                            }
                            else
                            {
                                if (existingLiteral.IsPositive != literal.IsPositive)
                                {
                                    newClause.Literals.Remove(existingLiteral);
                                }
                            }
                        }
                    }

                    // remove old clauses from the formula
                    Reduction.Formula.Clauses.Remove(positiveClause);
                    Reduction.Formula.Clauses.Remove(negativeClause);
                    if (newClause.Literals.Count > 0)
                        Reduction.Results.Add(newClause);
                }
            }

            // create a new reduction formula
            Reduction.Formula = Reduction.Formula.CopyAsSATFormula();

            reductionVariablesSource.Source = Reduction.Formula.Variables;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReductionVariablesView"));
        }

        private void CombineFinalClauses(object sender, RoutedEventArgs e)
        {
            Clause newClause = new Clause();

            foreach (var clause in Reduction.Formula.Clauses)
            {
                foreach (var literal in clause.Literals)
                {
                    Literal existingLiteral = newClause.Literals.Where(p => p.Variable == literal.Variable).FirstOrDefault();
                    if (existingLiteral == null)
                    {
                        newClause.Literals.Add(literal);
                    }
                    else
                    {
                        if (existingLiteral.IsPositive != literal.IsPositive)
                        {
                            newClause.Literals.Remove(existingLiteral);
                        }
                    }
                }
            }

            Reduction.Formula.Clauses.Clear();
            Reduction.Formula.Clauses.Add(newClause);

            // create a new reduction formula
            Reduction.Formula = Reduction.Formula.CopyAsSATFormula();

            reductionVariablesSource.Source = Reduction.Formula.Variables;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReductionVariablesView"));
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
                Reduction.Formula = Formula.CopyAsSATFormula();

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

                reductionVariablesSource.Source = Reduction.Formula.Variables;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReductionVariablesView"));

            }
            else {
                ReductionFormula = new SATFormula();
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
        private bool RelatedClausesFilter(object item) {
            Clause clause = item as Clause;

            if (clause.Literals[0].Variable == SelectedVariable ||
                clause.Literals[1].Variable == SelectedVariable ||
                clause.Literals[2].Variable == SelectedVariable) {
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

            foreach(Clause clause in relatedClauses)
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

        
    }
}
