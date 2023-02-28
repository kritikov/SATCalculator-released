using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SATCalculator.NewClasses
{
    public class Commands
    {
        public static readonly RoutedUICommand Remove = new RoutedUICommand(
            "remove",
            "remove",
            typeof(Commands)
        );

        public static readonly RoutedUICommand Add = new RoutedUICommand(
            "add",
            "add",
            typeof(Commands)
        );

        public static readonly RoutedUICommand Create = new RoutedUICommand(
            "create",
            "create",
            typeof(Commands)
        );

    }
}
