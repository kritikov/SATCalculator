using System.Windows.Input;

namespace SATCalculator.Core
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

        public static readonly RoutedUICommand Edit = new RoutedUICommand(
            "edit",
            "edit",
            typeof(Commands)
        );

        public static readonly RoutedUICommand Create = new RoutedUICommand(
            "create",
            "create",
            typeof(Commands)
        );

        public static readonly RoutedUICommand Reset = new RoutedUICommand(
            "reset",
            "reset",
            typeof(Commands)
        );

        public static readonly RoutedUICommand Copy = new RoutedUICommand(
            "copy",
            "copy",
            typeof(Commands)
        );

        public static readonly RoutedUICommand Solve = new RoutedUICommand(
            "solve",
            "solve",
            typeof(Commands)
        );

        public static readonly RoutedUICommand StopProcess = new RoutedUICommand(
            "stop process",
            "stop process",
            typeof(Commands)
        );

        public static readonly RoutedUICommand ApplyToFormula = new RoutedUICommand(
            "apply to formula",
            "apply to formula",
            typeof(Commands)
        );

        public static readonly RoutedUICommand NewFormula = new RoutedUICommand(
            "new formula",
            "new formula",
            typeof(Commands)
        );

        public static readonly RoutedUICommand LoadFormula = new RoutedUICommand(
            "load formula",
            "load formula",
            typeof(Commands)
        );

        public static readonly RoutedUICommand SaveFormula = new RoutedUICommand(
            "save formula",
            "save formula",
            typeof(Commands)
        );

    }
}
