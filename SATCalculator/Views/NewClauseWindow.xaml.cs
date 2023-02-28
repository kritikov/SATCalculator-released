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

        internal ObservableCollection<LiteralCreation> LiteralsList { get; set; }

        #endregion


        #region CONSTRUCTORS

        public NewClauseWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        #endregion


        #region EVENTS

        public event PropertyChangedEventHandler PropertyChanged;



        #endregion

        #region METHODS

        #endregion

        private void CreateFormula(object sender, RoutedEventArgs e)
        {

        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Message = "";
            Close();
        }
    }

    internal class LiteralCreation{
        Sign Sign { get; set; }
        string VariablePrefix { get; set; }

        int CnfIndex { get; set; }

        public LiteralCreation(Sign sign, string variablePrefix, int cnfIndex)
        {
            Sign = sign;
            VariablePrefix = variablePrefix;
            CnfIndex = cnfIndex;
        }
    }


}
