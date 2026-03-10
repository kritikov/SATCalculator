using SATCalculator.Core;
using System;
using System.ComponentModel;
using System.Linq;

namespace SATCalculator.Models
{
    public class Variable : INotifyPropertyChanged
    {
        public static Variable FixedVariable = new Variable("FIXED");

        public event PropertyChangedEventHandler PropertyChanged;

        #region Fields

        public static string DefaultVariableName = "x";

        public Guid Id = Guid.NewGuid();

        public Literal PositiveLiteral { get; set; }
        public Literal NegativeLiteral { get; set; }

        public string Name
        {
            get
            {
                if (CnfIndex == -1)
                    return "T/F";
                else
                    return DefaultVariableName + CnfIndex.ToString();
            }
        }
        public int CnfIndex { get; set; } = 0;

        private ValuationEnum valuation = ValuationEnum.Null;
        public ValuationEnum Valuation
        {
            get
            {
                return valuation;
            }
            set
            {
                valuation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Valuation"));
            }
        }

        public int ClausesWithPositiveReferencesCount => PositiveLiteral.ClausesContainingIt.Count;
        public int ClausesWithNegativeReferencesCount => NegativeLiteral.ClausesContainingIt.Count;
        public int References => ClausesWithPositiveReferencesCount + ClausesWithNegativeReferencesCount;
        public int Contrasts => Math.Min(ClausesWithPositiveReferencesCount, ClausesWithNegativeReferencesCount);

        #endregion


        #region Constructors

        public Variable(string value) : base()
        {
            PositiveLiteral = new Literal(this, Sign.Positive);
            NegativeLiteral = new Literal(this, Sign.Negative);

            try
            {
                if (value == "FIXED")
                {
                    CnfIndex = -1;
                }
                else
                {
                    string numbers = new string(value.Where(Char.IsDigit).ToArray());

                    if (string.IsNullOrEmpty(numbers))
                        numbers = "0";

                    CnfIndex = Convert.ToInt32(numbers);
                }
            }
            catch
            {
                throw;
            }
        }

        #endregion


        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion
 
    }
}
