using SATCalculator.Core;
using System.Collections.ObjectModel;

namespace SATCalculator.Models
{
    public class Literal
    {

        #region Fields

        public Variable Variable { get; set; }
        public Sign Sign { get; set; }
        public string SignToString { get
            {
                if (this.Sign == Sign.Negative)
                    return "-";
                else
                    return "+";
            } 
        }
        public string Name
        {
            get
            {
                if (Variable.CnfIndex == -1)
                {
                    if (Sign == Sign.Positive)
                        return "TRUE";
                    else
                        return "FALSE";
                }
                else
                {
                    if (Sign == Sign.Positive)
                        return Variable.Name;
                    else
                        return "-" + Variable.Name;
                }
            }
        }
        public ValuationEnum Valuation
        {
            get
            {
                if (Variable == null || Variable.Valuation == ValuationEnum.Null)
                    return ValuationEnum.Null;

                // if literal is positive
                if (Sign == Sign.Positive)
                {
                    return Variable.Valuation;
                }

                // if literal is negative
                if (Variable.Valuation == ValuationEnum.True)
                    return ValuationEnum.False;
                else
                    return ValuationEnum.True;
            }
        }

        public ObservableCollection<Clause> ClausesContainingIt { get; set; } = new ObservableCollection<Clause>();

        public Literal Opposite { 
            get
            {
                if (this.Sign == Sign.Positive)
                    return this.Variable.NegativeLiteral;
                else
                    return this.Variable.PositiveLiteral;
            } 
        }

        #endregion


        #region Constructors

        public Literal(Variable variable, Sign sign)
        {
            Variable = variable;
            Sign = sign;
        }

        #endregion


        #region Methods

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Inform the literal that a clause contains it
        /// </summary>
        public void RegisterClause(Clause clause)
        {
            if (!ClausesContainingIt.Contains(clause))
                ClausesContainingIt.Add(clause);
        }

        /// <summary>
        /// Unmark a clause as container from this literal
        /// </summary>
        public void UnregisterClause(Clause clause)
        {
            ClausesContainingIt.Remove(clause);
        }

        #endregion
    }
}
