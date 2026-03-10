using SATCalculator.Core;
using SATCalculator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SATCalculator.Views
{
    public class LiteralData
    {
        public Sign Sign { get; set; } = Sign.Positive;
        public string Prefix { get; set; } = Variable.DefaultVariableName;

        public int CnfIndex { get; set; } = 1;

        public LiteralData()
        {

        }

        public LiteralData(Sign sign, string prefix, int cnfIndex) : base()
        {
            Sign = sign;
            Prefix = prefix;
            CnfIndex = cnfIndex;
        }

        /// <summary>
        /// Returns a string that represents the current object, indicating its sign and index as cnf format
        /// </summary>
        public override string ToString()
        {
            string value = Sign == Sign.Positive ? "+" : "-";
            value += CnfIndex.ToString();

            return value;
        }
    }

    /// <summary>
    /// Interaction logic for NewClauseWindow.xaml
    /// </summary>
    public partial class ClauseEditorWindow : Window, INotifyPropertyChanged
    {
        #region VARIABLES AND NESTED CLASSES

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

        public ObservableCollection<LiteralData> LiteralsList { get; set; } = new ObservableCollection<LiteralData>();

        private readonly CollectionViewSource literalsListSource = new CollectionViewSource();
        public ICollectionView LiteralsListView
        {
            get
            {
                return this.literalsListSource.View;
            }
        }

        public List<string> LiteralsCnf { get; set; } = new List<string>();
        
        #endregion


        #region CONSTRUCTORS

        public ClauseEditorWindow()
        {
            InitializeComponent();
            DataContext = this;

            LiteralsList = new ObservableCollection<LiteralData>();

            literalsListSource.Source = LiteralsList;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LiteralsListView"));
        }

        public ClauseEditorWindow(Clause clause)
        {
            InitializeComponent();
            DataContext = this;

            LiteralsList = new ObservableCollection<LiteralData>();
            foreach (var literal in clause.Literals)
            {
                LiteralsList.Add(new LiteralData(literal.Sign, Variable.DefaultVariableName, literal.Variable.CnfIndex));
            }

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
            try
            {
                Message = "";
                var literal = LiteralsListView.CurrentItem as LiteralData;
                RemoveLiteral(literal);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }

        }

        private void AddLiteral_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void AddLiteral_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Message = "";
                AddLiteral();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void CreateClause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LiteralsList?.Count > 0 ? true : false;
        }
        private void CreateClause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Message = "";
                CreateClause();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        #endregion


        #region METHODS

        private void AddLiteral()
        {
            LiteralsList.Add(new LiteralData());
        }

        private void RemoveLiteral(LiteralData literal)
        {
            LiteralsList.Remove(literal);
        }

        private void CreateClause()
        {
            LiteralsCnf = LiteralsList.Select(p => p.ToString()).Distinct().ToList();
            Close();
        }

        #endregion

    }


}
