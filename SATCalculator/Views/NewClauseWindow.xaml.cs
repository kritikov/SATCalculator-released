using SATCalculator.NewClasses;
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
using System.Windows.Shapes;
using static SATCalculator.Views.MainWindow;

namespace SATCalculator.Views
{
    /// <summary>
    /// Interaction logic for NewClauseWindow.xaml
    /// </summary>
    public partial class NewClauseWindow : Window, INotifyPropertyChanged
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

        public class SignValue
        {
            public Sign Value { get; set; }
            public string ValueAsString { get; set; }
        }

        public static List<SignValue> SignValues { get; set; } = new List<SignValue>
        {
            new SignValue(){Value = Sign.Positive, ValueAsString="+" },
            new SignValue(){Value = Sign.Negative, ValueAsString="-" }
        };

        public ObservableCollection<LiteralCreation> LiteralsList { get; set; }

        private readonly CollectionViewSource literalsListSource = new CollectionViewSource();
        public ICollectionView LiteralsListView
        {
            get
            {
                return this.literalsListSource.View;
            }
        }

        #endregion


        #region CONSTRUCTORS

        public NewClauseWindow()
        {
            InitializeComponent();
            DataContext = this;

            LiteralsList = new ObservableCollection<LiteralCreation>();
            LiteralsList.Add(new LiteralCreation(Sign.Negative, Variable.DefaultVariableName, 1));
            LiteralsList.Add(new LiteralCreation(Sign.Positive, Variable.DefaultVariableName, 2));
            LiteralsList.Add(new LiteralCreation(Sign.Negative, Variable.DefaultVariableName, 3));

            literalsListSource.Source = LiteralsList;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LiteralsListView"));
        }

        #endregion


        #region EVENTS

        public event PropertyChangedEventHandler PropertyChanged;

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Message = "";
            Close();
        }

        #endregion


        #region COMMANDS

        private void RemoveLiteral_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LiteralsListView?.CurrentItem != null ? true : false;
        }
        private void RemoveLiteral_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var literal = LiteralsListView.CurrentItem as LiteralCreation;

            RemoveLiteral(literal);
        }

        private void AddLiteral_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void AddLiteral_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddLiteral();
        }

        private void CreateClause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LiteralsList?.Count > 0 ? true : false;
        }
        private void CreateClause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CreateClause();
        }

        #endregion


        #region METHODS

        private void AddLiteral()
        {
            LiteralsList.Add(new LiteralCreation());
        }

        private void CreateClause()
        {

        }

        #endregion

        private void RemoveLiteral(LiteralCreation literal)
        {
            LiteralsList.Remove(literal);
        }
    }

    public class LiteralCreation{
        public Sign Sign { get; set; } = Sign.Positive;
        public string Prefix { get; set; } = Variable.DefaultVariableName;

        public int CnfIndex { get; set; } = 1;

        public LiteralCreation()
        {

        }

        public LiteralCreation(Sign sign, string prefix, int cnfIndex)
        {
            Sign = sign;
            Prefix = prefix;
            CnfIndex = cnfIndex;
        }
    }


}
