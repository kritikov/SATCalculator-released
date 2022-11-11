﻿using System;
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

        private readonly CollectionViewSource participationClausesSource = new CollectionViewSource();
        public ICollectionView ParticipationClausesView {
            get {
                return this.participationClausesSource.View;
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

        //public Value[] VariableValues { get; set; } = { Value.Null, Value.True, Value.False };
        //public List<string> VariableValues = new List<string>(){"string1", "string2"};

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



                //TEST
                //Formula.SelectedVariable = Formula.Variables[0];
                participationClausesSource.Source = Formula.Clauses;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ParticipationClausesView"));
                ParticipationClausesView.Refresh();
            }
            else {
                // error
            }
        }

        
        /// <summary>
        /// Sort a grid by a field
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
        /// filter the participating clauses view
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool ParticipationClausesFilter(object item) {
            Clause clause = item as Clause;

            if (clause.Literals[0].Variable == Formula.SelectedVariable ||
                clause.Literals[1].Variable == Formula.SelectedVariable ||
                clause.Literals[2].Variable == Formula.SelectedVariable) {
                return true;
            }
            else
                return false;

            return true;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //var item = (e.OriginalSource as DataGrid).SelectedItem as Variable;

            if (Formula != null) {

                var grid = sender as DataGrid;

                if (grid.SelectedItem != null) {
                    var selectedItem = (KeyValuePair<string, Variable>)grid.SelectedItem;

                    Formula.SelectedVariable = selectedItem.Value;
                    if (Formula.SelectedVariable != null) {
                        ParticipationClausesView.Filter = ParticipationClausesFilter;
                        participationClausesSource.View.Refresh();
                        ParticipationClausesView.Refresh();
                    }
                }
            }

        }
    }
}
