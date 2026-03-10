using Microsoft.Win32;
using SATCalculator.Core;
using SATCalculator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SATCalculator.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region VARIABLES AND NESTED CLASSES

        private bool SearchingValuationsRunning = false;
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        public static List<VariableValue> VariableValues { get; set; } = new List<VariableValue>
        {
            new VariableValue(){Value = ValuationEnum.Null, ValueAsString="null" },
            new VariableValue(){Value = ValuationEnum.True, ValueAsString="true" },
            new VariableValue(){Value = ValuationEnum.False, ValueAsString="false" }
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

        private int selectedTab = 0;
        public int SelectedTab
        {
            get { return selectedTab; }
            set
            {
                selectedTab = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedTab"));
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

        private ObservableCollection<Clause> resolutionResolutionResults = new ObservableCollection<Clause>();
        public ObservableCollection<Clause> ResolutionResolutionResults
        {
            get => resolutionResolutionResults;
            set
            {
                resolutionResolutionResults = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionResolutionResults"));
            }
        }

        private SolverResults solverResults = new SolverResults();
        public SolverResults SolverResults
        {
            get => solverResults;
            set
            {
                solverResults = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SolverResults"));
            }
        }

        #endregion


        #region VIEWS

        private readonly CollectionViewSource clausesSource = new CollectionViewSource();
        public ICollectionView ClausesView
        {
            get
            {
                return this.clausesSource.View;
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

        private readonly CollectionViewSource formulaRelatedClausesSource = new CollectionViewSource();
        public ICollectionView FormulaRelatedClausesView
        {
            get
            {
                return this.formulaRelatedClausesSource.View;
            }
        }

        private readonly CollectionViewSource formulaClausesSource = new CollectionViewSource();
        public ICollectionView FormulaClausesView
        {
            get
            {
                return this.formulaClausesSource.View;
            }
        }

        private readonly CollectionViewSource resolutionClausesWithReferencesSource = new CollectionViewSource();
        public ICollectionView ResolutionClausesWithReferencesView
        {
            get
            {
                return this.resolutionClausesWithReferencesSource.View;
            }
        }

        private readonly CollectionViewSource resolutionClausesWithPositiveReferencesSource = new CollectionViewSource();
        public ICollectionView ResolutionClausesWithPositiveReferencesView
        {
            get
            {
                return this.resolutionClausesWithPositiveReferencesSource.View;
            }
        }

        private readonly CollectionViewSource resolutionClausesWithNegativeReferencesSource = new CollectionViewSource();
        public ICollectionView ResolutionClausesWithNegativeReferencesView
        {
            get
            {
                return this.resolutionClausesWithNegativeReferencesSource.View;
            }
        }
        //public CompositeCollection ResolutionClausesWithReferencesCollection { get; set; } = new CompositeCollection();

        private readonly CollectionViewSource solverSolutionsSource = new CollectionViewSource();
        public ICollectionView SolverSolutionsView
        {
            get
            {
                return this.solverSolutionsSource.View;
            }
        }

        private readonly CollectionViewSource solverSelectedSolutionSource = new CollectionViewSource();
        public ICollectionView SolverSelectedSolutionView
        {
            get
            {
                return this.solverSelectedSolutionSource.View;
            }
        }

        private readonly CollectionViewSource solverStatisticsSource = new CollectionViewSource();
        public ICollectionView SolverStatisticsView
        {
            get
            {
                return this.solverStatisticsSource.View;
            }
        }

        #endregion


        #region CONSTRUCTORS

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            CreateInitialFormula();
        }

        #endregion


        #region EVENTS

        public event PropertyChangedEventHandler PropertyChanged;

        private void ExitProgram(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void LoadFormula(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                LoadFormula();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void NewFormula(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                CreateNewFormula();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void SaveResolutionFormulaAsCNF(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                SaveFormulaAsCNF(Formula);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        public void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
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
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }
        
        private void FormulaVariablesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Message = "";

            try
            {
                if (Formula != null)
                {
                    var grid = sender as DataGrid;

                    if (grid.SelectedItem != null)
                    {
                        SelectedVariable = (Variable)grid.SelectedItem;
                        if (SelectedVariable != null && FormulaRelatedClausesView != null && FormulaClausesView != null)
                        {
                            FormulaRelatedClausesView.Filter = RelatedClausesFilter;
                            FormulaClausesView.Refresh();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void ResolutionVariablesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Message = "";

            try
            {
                if (Formula != null)
                {
                    var grid = sender as DataGrid;

                    if (grid.SelectedItem != null)
                    {
                        var selectedVariable = (Variable)grid.SelectedItem;

                        SelectedVariable = selectedVariable;
                        if (SelectedVariable != null)
                            RefreshResolutionViews();
                    }
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void ResolutionSelectedClauseChanged(object sender, SelectionChangedEventArgs e)
        {
            Message = "";

            try
            {
                if (ResolutionClausesWithPositiveReferencesView != null && ResolutionClausesWithNegativeReferencesView != null)
                {
                    ResolveSelectedClausesTest();
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void ResolutionResolutionSelectedClauses(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                ResolveSelectedClauses();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void ResolutionResolutionAllClausesTest(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                ResolveAllClausesTest();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void ResolutionResolutionAllClauses(object sender, RoutedEventArgs e)
        {
            Message = "";

            try
            {
                ResolveAllClauses();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void SolverSelectedSolutionChanged(object sender, SelectionChangedEventArgs e)
        {
            Message = "";

            try
            {
                if (SolverSolutionsView != null && SolverSolutionsView.CurrentItem != null)
                {
                    SolverResults.SelectedSolution = SolverSolutionsView.CurrentItem as FormulaSolution;
                    RefreshSolverViews();
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        #endregion


        #region COMMANDS

        private void NewFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;

        }
        private void NewFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                CreateNewFormula();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void LoadFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;

        }
        private void LoadFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                LoadFormula();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void SaveFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Formula != null ? true : false;

        }
        private void SaveFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                SaveFormulaAsCNF(Formula);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void RemoveClause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ClausesView?.CurrentItem != null ? true : false;
        }
        private void RemoveClause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                if (ClausesView.CurrentItem != null)
                {
                    var clause = ClausesView.CurrentItem as Clause;
                    RemoveSelectedClause(clause);
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void AddClause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Formula != null ? true : false;

        }
        private void AddClause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                AddNewClause();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void EditClause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ClausesView?.CurrentItem != null ? true : false;

        }
        private void EditClause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                if (ClausesView.CurrentItem != null)
                {
                    var clause = ClausesView.CurrentItem as Clause;
                    EditClause(clause);
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void ResetFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Formula != null ? true : false;

        }
        private void ResetFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                Formula = formulaOriginal.Copy();
                SelectedVariable = null;
                SolverResults = null;
                RefreshViews();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void CopyFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Formula != null ? true : false;

        }
        private void CopyFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                Clipboard.SetText(Formula.Name);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void SolveFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Formula != null && SearchingValuationsRunning == false) ? true : false;

        }
        private async void SolveFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                SearchingValuationsRunning = true;

                await SolveFormula();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void StopSearchingValuations_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Formula != null && SearchingValuationsRunning == true) ? true : false;

        }
        private void StopSearchingValuations_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                cancellationToken.Cancel();
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        private void ApplyToFormula_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Formula != null && SolverSolutionsView != null && SolverSolutionsView.CurrentItem != null) ? true : false;
        }
        private void ApplyToFormula_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Message = "";

            try
            {
                ApplyValuationToFormula(SolverResults.SelectedSolution, Formula);
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
        }

        #endregion


        #region METHODS

        /// <summary>
        /// Sort a ListView by a field
        /// </summary>
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
        /// Refresh all the views
        /// </summary>
        private void RefreshViews()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Formula"));

            clausesSource.Source = Formula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClausesView"));

            variablesSource.Source = Formula.Variables;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VariablesView"));
            VariablesView?.SortDescriptions.Add(new SortDescription("CnfIndex", ListSortDirection.Ascending));
            VariablesView.Refresh();

            RefreshFormulaViews();
            RefreshResolutionViews();
            RefreshSolverViews();
        }

        /// <summary>
        /// Refresh the formula tab views
        /// </summary>
        private void RefreshFormulaViews()
        {
            formulaClausesSource.Source = Formula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormulaClausesView"));

            formulaRelatedClausesSource.Source = Formula.Clauses;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormulaRelatedClausesView"));
        }

        /// <summary>
        /// Refreshes the solver tab views.
        /// </summary>
        private void RefreshSolverViews()
        {
            solverSolutionsSource.Source = SolverResults?.Solutions;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SolverSolutionsView"));

            solverSelectedSolutionSource.Source = SolverResults?.SelectedSolution?.ValuationsList;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SolverSelectedSolutionView"));

            solverStatisticsSource.Source = SolverResults?.Statistics;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SolverStatisticsView"));

        }

        /// <summary>
        /// Refresh the Resolution tab views
        /// </summary>
        private void RefreshResolutionViews()
        {
            if (SelectedVariable != null)
            {
                resolutionClausesWithPositiveReferencesSource.Source = SelectedVariable.PositiveLiteral.ClausesContainingIt;
                resolutionClausesWithNegativeReferencesSource.Source = SelectedVariable.NegativeLiteral.ClausesContainingIt;
            }
            else
            {
                resolutionClausesWithPositiveReferencesSource.Source = null;
                resolutionClausesWithNegativeReferencesSource.Source = null;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesWithPositiveReferencesView"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ResolutionClausesWithNegativeReferencesView"));
        }

        /// <summary>
        /// filter the related clauses view
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool RelatedClausesFilter(object item)
        {
            Clause clause = item as Clause;
            return clause.Literals.Any(p => p.Variable == SelectedVariable);
        }
       
        /// <summary>
        /// Resolution the selected clauses in the positive and negative lists of the selected variable
        /// without updating the formula 
        /// </summary>
        private void ResolveSelectedClausesTest()
        {
            ResolutionResolutionResults.Clear();

            if (ResolutionClausesWithPositiveReferencesView.CurrentItem != null && ResolutionClausesWithNegativeReferencesView.CurrentItem != null)
            {
                // get the selected items from the lists to apply resolution
                var positiveClause = ResolutionClausesWithPositiveReferencesView.CurrentItem as Clause;
                var negativeClause = ResolutionClausesWithNegativeReferencesView.CurrentItem as Clause;

                if (positiveClause != null && negativeClause != null)
                {
                    Clause newClause = Clause.GetFromResolution(SelectedVariable, positiveClause, negativeClause);

                    // add the new clause to the results
                    ResolutionResolutionResults.Add(newClause);
                }
            }
        }

        /// <summary>
        /// Resolution all clauses in the positive and negative lists of the selected variables
        /// without updating the formula
        /// </summary>
        private void ResolveAllClausesTest()
        {
            ResolutionResolutionResults.Clear();

            var newClauses = Clause.GetFromResolution(SelectedVariable);

            foreach (var clause in newClauses)
            {
                ResolutionResolutionResults.Add(clause);
            }
        }

        /// <summary>
        /// Resolution the selected clauses in the positive and negative lists of the selected variable
        /// </summary>
        private void ResolveSelectedClauses()
        {
            var positiveClause = ResolutionClausesWithPositiveReferencesView.CurrentItem as Clause;
            var negativeClause = ResolutionClausesWithNegativeReferencesView.CurrentItem as Clause;
            ResolutionResolutionResults.Clear();

            if (positiveClause != null && negativeClause != null)
            {
                Formula.Resolve(SelectedVariable, positiveClause, negativeClause);
                RefreshViews();
            }
        }

        /// <summary>
        /// Resolution all clauses in the positive and negative lists of the selected variables
        /// </summary>
        private void ResolveAllClauses()
        {
            ResolutionResolutionResults.Clear();

            Formula.Resolve(SelectedVariable);

            RefreshViews();
        }

        /// <summary>
        /// Create a new formula
        /// </summary>
        private void CreateNewFormula()
        {
            NewFormulaWindow newFormulaWindow = new NewFormulaWindow();
            newFormulaWindow.ShowDialog();

            if (newFormulaWindow.FormulaCnfLines.Count > 0)
            {
                formulaOriginal = SATFormula.CreateFromCnfLines(newFormulaWindow.FormulaCnfLines);
                Formula = formulaOriginal.Copy();

                SelectedVariable = null;
                SolverResults = null;
                RefreshViews();
            }
        }

        /// <summary>
        /// Create the initial formula to display as example
        /// </summary>
        private void CreateInitialFormula()
        {
            List<string> lines = new List<string>()
                {
                    "1 2 3 0",
                    "1 -2 3 0",
                    "1 2 -3 0",
                };

            formulaOriginal = SATFormula.CreateFromCnfLines(lines);
            Formula = formulaOriginal.Copy();

            SelectedVariable = null;
            SolverResults = null;
            RefreshViews();
        }

        /// <summary>
        /// Open a window to select a cnf file to load a formula
        /// </summary>
        private void LoadFormula()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Resources";

            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = path,
                FileName = "file",
                DefaultExt = ".cnf",
                Filter = "SAT files (.cnf)|*.cnf"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Αν ο χρήστης επέλεξε αρχείο, φορτώνουμε τη φόρμουλα
                string filename = openFileDialog.FileName;

                formulaOriginal = FileHelper.GetFormulaFromCnfFile(filename);
                Formula = formulaOriginal.Copy();

                SelectedVariable = null;
                SolverResults = null;

                RefreshViews();
            }
            else
            {
                Formula = new SATFormula();
            }
        }

        /// <summary>
        /// Save a formula in a file at cnf format
        /// </summary>
        /// <param name="formula"></param>
        private void SaveFormulaAsCNF(SATFormula formula)
        {
            string extension = "cnf";

            var saveFileDialog = new SaveFileDialog
            {
                Filter = $"{extension} file|*.{extension}",
                Title = $"Save a {extension} file"
            };

            // Ανοίγει το dialog και εκτελείται μόνο αν ο χρήστης επιλέξει file
            if (saveFileDialog.ShowDialog() == true)
            {
                FileHelper.SaveFormulaAsCNF(formula, saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// Delete the selected clause from the formula
        /// </summary>
        /// <param name="clause"></param>
        private void RemoveSelectedClause(Clause clause)
        {
            Formula.RemoveClause(clause);
            RefreshViews();
        }

        /// <summary>
        /// Add a new clause in the formula
        /// </summary>
        private void AddNewClause()
        {
            ClauseEditorWindow newClauseWindow = new ClauseEditorWindow();
            newClauseWindow.ShowDialog();

            if (newClauseWindow.LiteralsCnf.Count > 0)
            {
                Formula.CreateClauseFromCnfs(newClauseWindow.LiteralsCnf);
                SelectedVariable = null;
                RefreshViews();
            }

            RefreshViews();
        }

        /// <summary>
        /// Displays a dialog that allows the user to edit a clause and updates the formula based on the user's input.
        /// </summary>
        private void EditClause(Clause clause)
        {
            ClauseEditorWindow newClauseWindow = new ClauseEditorWindow(clause);
            newClauseWindow.ShowDialog();

            if (newClauseWindow.LiteralsCnf.Count > 0)
            {
                Formula.RemoveClause(clause);
                Formula.CreateClauseFromCnfs(newClauseWindow.LiteralsCnf);
                SelectedVariable = null;
                RefreshViews();
            }

        }

        /// <summary>
        /// Find the valuations that solve the formula
        /// </summary>
        /// <param name="formula"></param>
        private async Task SolveFormula()
        {
            try
            {
                // reset results
                SolverResults = new SolverResults();
                BindSolverMessages();
                RefreshSolverViews();

                cancellationToken = new CancellationTokenSource();

                await Task.Run(() => {
                    SolverDeterministic.Solve(Formula, cancellationToken.Token, SolverResults);
                });

                SearchingValuationsRunning = false;
                RefreshSolverViews();
            }
            finally
            {
                SearchingValuationsRunning = false;
            }
        }

        /// <summary>
        /// Binds SolverResults updates to the general Message property of MainWindow
        /// </summary>
        private void BindSolverMessages()
        {
            if (SolverResults != null)
            {
                SolverResults.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "Message")
                    {
                        Message = SolverResults.Message;
                    }
                };
            }
        }

        /// <summary>
        /// Set the valuations of a solution to the variables of the formula
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="formula"></param>
        private void ApplyValuationToFormula(FormulaSolution solution, SATFormula formula)
        {
            formula.ApplyValuation(solution);

            SelectedTab = 0;

            //RefreshFormulaViews();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #endregion
    }
}
