using SATCalculator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace SATCalculator
{
    /// <summary>
    /// Interaction logic for NewFormula.xaml
    /// </summary>
    public partial class NewFormulaWindow : Window, INotifyPropertyChanged
    {


        #region VARIABLES AND NESTED CLASSES

        internal class Literal
        {
            internal int CnfIndex = 0;
            internal string Sign = "";

        }

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

        //private string formulaString = "(1 ∨ 4) ∧ (2 ∨ -4) ∧ (-2 ∨ 5) ∧ (1 ∨ -5) ∧ (-1 ∨ 6) ∧ (3 ∨ -6) ∧ (-1 ∨ 7) ∧ (-7 ∨ -3)";
        private string formulaString = "(a ∨ b) ∧ (c ∨ -a) ∧ (d ∨ b)";
        public string FormulaString {
            get => formulaString;
            set
            {
                formulaString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormulaString"));
            } 
        }

        private string resultFormulaString = "";
        public string ResultFormulaString {
            get => resultFormulaString;
            set
            {
                resultFormulaString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResultFormulaString"));
            } 
        }

        private string andSymbol = "∧";
        public string AndSymbol {
            get => andSymbol; 
            set
            {
                if (AndSymbolIsValid(value))
                {
                    andSymbol = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AndSymbol"));
                }
            }
        }

        private string orSymbol = "∨";
        public string OrSymbol {
            get => orSymbol; 
            set
            {
                if (OrSymbolIsValid(value))
                {
                    orSymbol = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OrSymbol"));
                }
            }
        }

        private string minusSymbol = "-";
        public string MinusSymbol {
            get => minusSymbol; 
            set
            {
                if (MinusSymbolIsValid(value))
                {
                    minusSymbol = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MinusSymbol"));
                }
            }
        }

        public readonly string AndSymbolOriginal = "∧";
        public readonly string OrSymbolOriginal = "∨";
        public readonly string MinusSymbolOriginal = "-";

        public List<string> FormulaCnfLines { get; set; } = new List<string>();


        #endregion

        
        #region CONSTRUCTORS

        public NewFormulaWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        #endregion


        #region EVENTS

        public event PropertyChangedEventHandler PropertyChanged;

        private void TestFormula(object sender, RoutedEventArgs e)
        {
            Message = "";
            try
            {
                TestFormula(FormulaString);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void CreateFormula(object sender, RoutedEventArgs e)
        {
            Message = "";
            try
            {
                FormulaCnfLines = FormulaToCnf(FormulaString);
                Close();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Message = "";
            FormulaString = "";
            Close();
        }

        #endregion


        #region METHODS

        /// <summary>
        /// Check the given AND symbol for syntax errors
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool AndSymbolIsValid(string value)
        {
            if (value == OrSymbol)
                throw new Exception("The And symbol cannot be the same as the Or symbol");

            if (value == String.Empty)
                throw new Exception("The And symbol cannot be empty");

            return true;
        }

        /// <summary>
        /// Check the given OR symbol for syntax errors
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool OrSymbolIsValid(string value)
        {
            if (value == AndSymbol)
                throw new Exception("The Or symbol cannot be the same as the And symbol");

            if (value == String.Empty)
                throw new Exception("The Or symbol cannot be empty");

            return true;
        }

        /// <summary>
        /// Check the given Minus symbol for syntax errors
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool MinusSymbolIsValid(string value)
        {
            if (value == String.Empty)
                throw new Exception("The Or symbol cannot be empty");

            if (value.Length != 1)
                throw new Exception("The Minus symbol has not proped length");

            return true;
        }

        /// <summary>
        /// Check the given formula for syntax errors
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        private bool FormulaIsValid(string formula)
        {
            if (formula == string.Empty)
            {
                throw new Exception("The Or symbol cannot be empty");
            }

            int leftParenthesisCount = formula.Count(f => (f == '('));
            int rightParenthesisCount = formula.Count(f => (f == ')'));
            if (leftParenthesisCount != rightParenthesisCount)
            {
                throw new Exception("Some parenthesis are missing");
            }

            return true;
        }

        /// <summary>
        /// Split a formula string to a list with components
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        private List<List<Literal>> SplitFormula(string formula)
        {
            List<List<Literal>> clausesList = new List<List<Literal>>();
            Dictionary<string, int> variables = new Dictionary<string, int>();
            int cnfIndex = 0;

            // create the clauses
            var clausesArray = formula.Split(new string[] { AndSymbol }, StringSplitOptions.None);

            // edit each clause
            foreach (var clause in clausesArray)
            {
                // clear all parenthesis end empty spaces
                string clauseFormatted = clause.Replace(" ", "");
                clauseFormatted = clauseFormatted.Replace("(", "");
                clauseFormatted = clauseFormatted.Replace(")", "");

                if (clauseFormatted == string.Empty)
                    continue;

                // create the literals of the clause
                var literals = clauseFormatted.Split(new string[] { OrSymbol }, StringSplitOptions.None).ToList();
                List<Literal> LiteralNames = new List<Literal>();

                // rename the literals to a valid number
                foreach (var literal in literals)
                {
                    string variableName;
                    Literal literalName;
                    string sign = "";

                    // get the sign and the variable name
                    if (literal.StartsWith("+"))
                    {
                        if (literal.Length == 1)
                            throw new Exception($"the name of the literal {literal} in clause {clause} cannot be resolved");
                        else
                            variableName = literal.Substring(1, literal.Length - 1);

                    }
                    else if (literal.StartsWith(MinusSymbol))
                    {
                        if (literal.Length == 1)
                            throw new Exception($"the name of the literal {literal} in clause {clause} cannot be resolved");
                        else
                        {
                            sign = MinusSymbolOriginal;
                            variableName = literal.Substring(1, literal.Length - 1);
                        }
                    }
                    else
                    {
                        if (literal.Length == 0)
                            throw new Exception($"the name of the literal {literal} in clause {clause} cannot be resolved");
                        else
                            variableName = literal;
                    }

                    // create a variable name based on cnf indexes
                    if (variables.ContainsKey(variableName))
                    {
                        literalName = new Literal()
                        {
                            CnfIndex = variables[variableName],
                            Sign = sign
                        };
                    }
                    else
                    {
                        variables[variableName] = ++cnfIndex;

                        literalName = new Literal()
                        {
                            CnfIndex = cnfIndex,
                            Sign = sign
                        };
                    }

                    // add the literal to the list
                    LiteralNames.Add(literalName);
                }


                // add the clause with its literals in the list
                if (LiteralNames.Count > 0)
                    clausesList.Add(LiteralNames);
            }

            return clausesList;
        }

        /// <summary>
        /// Compose the components of the formula to a string as the expected result 
        /// </summary>
        /// <param name="clausesList"></param>
        /// <returns></returns>
        private string ComposeToString(List<List<Literal>> clausesList)
        {
            string formula = "";

            foreach (var clause in clausesList)
            {
                if (formula != "")
                    formula += $" {AndSymbolOriginal} ";

                formula += "(";
                foreach (var literal in clause)
                {
                    if (!formula.EndsWith("("))
                        formula += $" {OrSymbolOriginal} ";

                    formula += $"{literal.Sign}{Variable.DefaultVariableName}{literal.CnfIndex}";
                }
                formula += ")";
            }

            return formula;
        }

        /// <summary>
        /// Create formula lines in cnf format
        /// </summary>
        /// <returns></returns>
        private List<string> ComposeToCnfLines(List<List<Literal>> clausesList)
        {
            List<string> lines = new List<string>();

            foreach (var clause in clausesList)
            {
                string line = "";

                foreach (var literal in clause)
                {
                    if (line != "")
                        line += " ";

                    string newLiteral = $"{literal.Sign}{literal.CnfIndex}";
                    line += newLiteral;
                }
                line += " 0";
                lines.Add(line);
            }

            return lines;
        }

        /// <summary>
        /// Convert a given formula to the expected result
        /// </summary>
        private void TestFormula(string formula)
        {
            if (FormulaIsValid(formula))
            {
                var clausesList = SplitFormula(formula);
                var formulaFormatted = ComposeToString(clausesList);

                ResultFormulaString = formulaFormatted;
            }
        }

        /// <summary>
        /// Converts the given formula to a list with cnf lines
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        private List<string> FormulaToCnf(string formula)
        {
            List<string> lines = new List<string>();

            if (FormulaIsValid(formula))
            {
                var clausesList = SplitFormula(formula);
                lines = ComposeToCnfLines(clausesList);
            }

            return lines;
        }

        #endregion
    }

    
}
