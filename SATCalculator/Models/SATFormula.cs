using SATCalculator.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SATCalculator.Models
{
    public class SATFormula
    {

        #region Fields

        public ObservableCollection<Clause> Clauses { get; set; } = new ObservableCollection<Clause>();
        public ObservableCollection<Variable> Variables { get; set; } = new ObservableCollection<Variable>();
        public ObservableCollection<FormulaSolution> Solutions { get; set; } = new ObservableCollection<FormulaSolution>();

        public Dictionary<string, Variable> VariablesDict { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, Clause> ClausesDict { get; set; } = new Dictionary<string, Clause>();

        public int ClausesCount => Clauses.Count;
        public int VariablesCount => Variables.Count;
        public int SolutionsCount => Solutions.Count;

        public ValuationEnum Valuation
        {
            get
            {
                // if one clause is false then the whole formula is false
                if (Clauses.Any(p => p.Valuation == ValuationEnum.False))
                    return ValuationEnum.False;

                // if one clause is null then the valuation of the formula is still unspecified
                if (Clauses.Any(p => p.Valuation == ValuationEnum.Null))
                    return ValuationEnum.Null;

                // in any other case the formula is true
                return ValuationEnum.True;
            }
        }

        public string Name
        {
            get{
                string value = "";

                foreach (Clause clause in Clauses)
                {
                    if (value != "")
                        value += " ^ ";

                    value += $"({clause})";
                }

                return value;
            }
        }

        public string DisplayValue => this.ToString();

        #endregion


        #region Constructors

        public SATFormula()
        {

        }

        #endregion


        #region Methods

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Creates a new SATFormula instance by parsing a list of strings representing clauses in Conjunctive Normal Form (CNF).
        /// </summary>
        public static SATFormula CreateFromCnfLines(List<string> cnfLines)
        {
            SATFormula formula = new SATFormula();

            foreach (string line in cnfLines)
            {
                var lineParts = line.Trim().Split(' ').ToList();

                formula.CreateClauseFromCnfs(lineParts);
            }

            return formula;

        }

        /// <summary>
        /// Convert the formula to a list of cnf lines
        /// </summary>
        public List<string> GetCNFLines()
        {
            List<string> cnfLines = new List<string>();

            foreach (var clause in this.Clauses)
            {
                cnfLines.Add(clause.GetCNFLine());
            }

            return cnfLines;

        }

        /// <summary>
        /// Create a clone of the formula. We convert the original formula to cnf and we create the new one from this
        /// </summary>
        public SATFormula Copy()
        {
            List<string> parts = this.GetCNFLines();
            SATFormula formula = CreateFromCnfLines(parts);

            return formula;
        }

        /// <summary>
        /// Get a variable from the dictionary or create a new one if it doesnt exists
        /// </summary>
        private Variable GetOrCreateVariable(string name)
        {
            // create the variable
            // TODO: can we use the name instead of variable.Name later when searching in the dict?
            Variable variable = new Variable(name);

            // use the stored variable if exists or add the new to the dictionary
            if (VariablesDict.ContainsKey(variable.Name))
                variable = VariablesDict[variable.Name];
            else
            {
                VariablesDict.Add(variable.Name, variable);
                Variables.Add(variable);
            }

            return variable;
        }

        private void RemoveVariable(Variable variable)
        {
            if (VariablesDict.ContainsKey(variable.Name))
            {
                VariablesDict.Remove(variable.Name);
                Variables.Remove(variable);
            }
        }

        /// <summary>
        /// Create a clause from a list of literals in cnf format and add it to the formula
        /// </summary>
        public void CreateClauseFromCnfs(List<string> literalsCnf)
        {
            // create the clause
            Clause clause = new Clause();

            if (literalsCnf.Count > 0)
            {
                if (literalsCnf[0].Length > 0)
                {
                    if (literalsCnf[0] == "c" || literalsCnf[0] == "p" || literalsCnf[0] == "0" || literalsCnf[0] == "%")
                        return;

                    foreach (var literalCnf in literalsCnf)
                    {
                        if (literalCnf != "0")
                        {
                            // create the variable
                            Variable variable = GetOrCreateVariable(literalCnf);

                            // add the literal in the clause
                            if (literalCnf[0] == '-')
                                clause.AddLiteral(variable.NegativeLiteral);
                            else
                                clause.AddLiteral(variable.PositiveLiteral);
                        }
                    }
                }
            }

            // sort the literals in the clause
            clause.SortLiterals();

            // add the clause in the dictionary and in the list with formula clauses if not exists
            if (!ClausesDict.ContainsKey(clause.Name))
            {
                Clauses.Add(clause);
                ClausesDict.Add(clause.Name, clause);
            }
        }

        /// <summary>
        /// Remove a clause from the formula and its lists
        /// </summary>
        public void RemoveClause(Clause clause)
        {
            // remove clause from the literals lists
            clause.UnregisterFromLiterals();

            // remove the clause from the dictionaries
            if (ClausesDict.ContainsKey(clause.Name))
                ClausesDict.Remove(clause.Name);

            if (Clauses.Contains(clause))
                Clauses.Remove(clause);

            ClearUnusedVariables(clause);
        }

        /// <summary>
        /// Add a clause to the formula if doesnt allready exists. The clause must allready use
        /// variables and literals declared in the formula
        /// </summary>
        public void AddClause(Clause clause)
        {
            // if the clause allready exists in the formula then exit
            if (ClausesDict.ContainsKey(clause.Name))
                return;

            ClausesDict.Add(clause.Name, clause);
            Clauses.Add(clause);

            clause.UpdateLiterals();
        }

        /// <summary>
        /// Apply the valuations from a solution to the variables of the formula
        /// </summary>
        public void ApplyValuation(FormulaSolution solution)
        {
            // reset all variables of the formula
            foreach(var variable in this.Variables)
            {
                variable.Valuation = ValuationEnum.Null;
            }


            foreach(var valuation in solution.ValuationsList)
            {
                if (VariablesDict.ContainsKey(valuation.VariableName))
                {
                    VariablesDict[valuation.VariableName].Valuation = valuation.Valuation;
                }
            }
        }

        private void ClearUnusedVariables(Clause clause)
        {
            foreach (var literal in clause.Literals)
            {
                if (literal.Variable.References == 0)
                    RemoveVariable(literal.Variable);
            }
        }

        /// <summary>
        /// Resolves the specified variable within the given positive and negative clauses, updating the formula
        /// </summary>
        public void Resolve(Variable variable, Clause positiveClause, Clause negativeClause)
        {
            Clause newClause = Clause.GetFromResolution(variable, positiveClause, negativeClause);

            // if the result is TRUE then we dont change the formula else we replace the initial clauses with the new one
            if (newClause != Clause.ClauseTrue && newClause != Clause.ClauseFalse)
            {
                // add the new clause in the formula
                AddClause(newClause);
            }
        }

        /// <summary>
        /// Resolution the clauses of a specific variable.
        /// </summary>
        public void Resolve(Variable variable)
        {
            var clausesToRemove = new List<Clause>();
            var clausesToAdd = new List<Clause>();

            for (int i = 0; i < variable.Contrasts; i++)
            {
                var positiveClause = variable.PositiveLiteral.ClausesContainingIt[i];
                var negativeClause = variable.NegativeLiteral.ClausesContainingIt[i];

                if (positiveClause != null && negativeClause != null)
                {
                    Clause newClause = Clause.GetFromResolution(variable, positiveClause, negativeClause);

                    if (newClause != Clause.ClauseTrue && newClause != Clause.ClauseFalse)
                    {
                        if (newClause != Clause.ClauseTrue)
                            clausesToAdd.Add(newClause);
                    }
                }
            }

            foreach (var clause in clausesToAdd)
                AddClause(clause);

        }

        #endregion
    }
}
