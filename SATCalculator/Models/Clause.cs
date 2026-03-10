using SATCalculator.Core;
using System.Collections.Generic;
using System.Linq;

namespace SATCalculator.Models
{
    public class Clause
    {
        // we use this as the true and the false clauses
        public static Clause ClauseTrue = new Clause();
        public static Clause ClauseFalse = new Clause();

        #region Fields

        // the clause contains literals and not variables
        public List<Literal> Literals { get; set; } = new List<Literal>();

        public string Name
        {
            get
            {
                if (this == ClauseTrue)
                {
                    return "TRUE";
                }
                else if (this == ClauseFalse)
                {
                    return "FALSE";
                }
                else
                {
                    string name = "";
                    foreach (var literal in Literals)
                    {
                        if (name != "")
                            name += " ∨ ";

                        name += literal.Name;
                    }

                    return name;
                }
            }
        }

        /// <summary>
        /// Gets the overall valuation of the collection based on the valuations of its literals. 
        /// If one literal is true then the overal valuation is true.
        /// </summary>
        public ValuationEnum Valuation
        {
            get
            {
                if (this == ClauseTrue)
                    return ValuationEnum.True;

                if (this == ClauseFalse)
                    return ValuationEnum.False;

                if (Literals.Any(p => p.Valuation == ValuationEnum.True))
                    return ValuationEnum.True;

                if (Literals.Count(p => p.Valuation == ValuationEnum.False) == Literals.Count)
                    return ValuationEnum.False;

                return ValuationEnum.Null;
            }
        }

        #endregion


        #region Constructors

        public Clause()
        {

        }

        #endregion


        #region Methods

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Generates a string representation of the clause in Conjunctive Normal Form (CNF) by concatenating the names
        /// of all literals, separated by spaces, and appending a terminating zero.
        /// </summary>
        public string GetCNFLine()
        {

            string line = "";
            foreach (var literal in Literals)
            {
                if (line != "")
                    line += " ";

                line += literal.Name;
            }
            line += $" 0";

            return line;
        }

        /// <summary>
        /// Adds the specified literal to this clause and updates the literal's collection of associated clauses.
        /// </summary>
        public void AddLiteral(Literal literal)
        {
            if (!Literals.Contains(literal))
            {
                Literals.Add(literal);
                literal.RegisterClause(this);
            }
            literal.RegisterClause(this);
        }

        /// <summary>
        /// Removes the current clause from all literals that reference it.
        /// </summary>
        public void UnregisterFromLiterals()
        {
            foreach (var literal in Literals)
            {
                literal.UnregisterClause(this);
            }
        }

        /// <summary>
        /// Ensures that this clause is included in the list of clauses for each literal it contains.
        /// </summary>
        public void UpdateLiterals()
        {
            foreach (var literal in Literals)
            {
                literal.RegisterClause(this);
            }
        }

        /// <summary>
        /// Sorts the collection of literals in ascending order based on the CNF index of their associated variables.
        /// </summary>
        public void SortLiterals()
        {
            Literals = Literals.OrderBy(p => p.Variable.CnfIndex).ToList();
        }

        ///  <summary>
        /// Return a new clause from a variable and the resolution of two clauses contain it. 
        /// The new clause is 'free', meaning that it has not update its literal references
        /// because it is not added in a formula. If we want to do that we must
        /// update its literals references  
        /// </summary>
        public static Clause GetFromResolution(Variable variable, Clause positiveClause, Clause negativeClause)
        {
            Clause newClause = new Clause();

            // if the two clauses has one literal each and in contrast values then we have a contradiction
            if (positiveClause.Literals.Count == 1 && negativeClause.Literals.Count == 1 &&
                positiveClause.Literals[0].Variable == variable &&
                negativeClause.Literals[0].Variable == variable)
            {
                return Clause.ClauseFalse;
            }

            // check the literals in the clause with the positive reference of the variable
            foreach (var literal in positiveClause.Literals)
            {
                if (literal.Variable != variable)
                {
                    // if the literal exists in the new clause but with opposite value
                    // then the clause is always true and can be discarded
                    if (newClause.Literals.Contains(literal.Opposite))
                        return Clause.ClauseTrue;

                    // if the literal doesnt allready exists in the new clause then add it
                    if (!newClause.Literals.Contains(literal))
                        newClause.Literals.Add(literal);
                }
            }

            // check the literals in the clause with the negative reference of the variable
            foreach (var literal in negativeClause.Literals)
            {
                if (literal.Variable != variable)
                {
                    // if the literal exists in the new clause but with opposite value
                    // then the clause is always true and can be discarded
                    if (newClause.Literals.Contains(literal.Opposite))
                        return Clause.ClauseTrue;

                    // if the literal doesnt allready exists in the new clause then add it
                    if (!newClause.Literals.Contains(literal))
                        newClause.Literals.Add(literal);
                }
            }

            newClause.SortLiterals();

            return newClause;
        }

        /// <summary>
        /// Generates a list of new clauses by resolving the specified variable's positive and negative literals across
        /// all contrasts. The new clause are 'free', meaning that they have not update its literal references
        /// because they are not added in a formula. If we want to do that we must update its literals references  
        /// </summary>
        public static List<Clause> GetFromResolution(Variable variable)
        {
            var newClauses = new List<Clause>();

            for (int i = 0; i < variable.Contrasts; i++)
            {
                var positiveClause = variable.PositiveLiteral.ClausesContainingIt[i];
                var negativeClause = variable.NegativeLiteral.ClausesContainingIt[i];

                if (positiveClause != null && negativeClause != null)
                {
                    Clause newClause = Clause.GetFromResolution(variable, positiveClause, negativeClause);
                    newClauses.Add(newClause);
                }
            }

            return newClauses;
        }

        #endregion

    }
}
