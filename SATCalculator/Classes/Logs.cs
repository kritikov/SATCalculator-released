using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SATCalculator.Classes
{
    public static class Logs
    {

        private static ObservableCollection<string> list = new ObservableCollection<string>();
        private static int MaxLenth = 5000;

        public static ObservableCollection<string> List
        {
            get => list;
        }

        public static void Write(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                Add(message);
            });
        }

        private static void Add(string message)
        {

            if (list.Count() == MaxLenth)
            {
                list.RemoveAt(0);
            }

            list.Add($@"{DateTime.Now:hh:mm:ss}:: {message}");
        }
    }
}
