using Microsoft.Win32;
using SATCalculator.NewClasses;
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

namespace SATCalculator.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

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

        }

        private void SaveEditorFormulaAsCNF(object sender, RoutedEventArgs e)
        {

        }

        private void ResetFormula(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteSelectedClause(object sender, RoutedEventArgs e)
        {

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
        /// Refresh all the views
        /// </summary>
        private void RefreshViews()
        {
            clausesSource.Source = Formula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClausesView"));

            RefreshFormulaViews();
        }

        /// <summary>
        /// Refresh the formula tab views
        /// </summary>
        private void RefreshFormulaViews()
        {
        }
        #endregion

    }
}
