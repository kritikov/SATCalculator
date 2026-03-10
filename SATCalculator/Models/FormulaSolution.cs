using SATCalculator.Core;
using System.Collections.ObjectModel;

namespace SATCalculator.Models
{
    public class FormulaSolution
    {
        public ObservableCollection<VariableValuation> ValuationsList { get; set; } = new ObservableCollection<VariableValuation>();

        public string DisplayValue
        {
            get
            {
                string value = "";

                foreach (var variableValuation in ValuationsList)
                {
                    if (value != "")
                        value += ", ";

                    string valuation = variableValuation.Valuation == ValuationEnum.True ? "T" : "F";

                    value += $"{variableValuation.VariableName} = {valuation}";
                }

                return value;
            }
        }

        public override string ToString()
        {
            return DisplayValue;
        }


    }

    public class VariableValuation
    {
        public string VariableName { get; set; }
        public ValuationEnum Valuation { get; set; } = ValuationEnum.Null;

        public string DisplayValue
        {
            get
            {
                string value = $"{VariableName} = {Valuation}";

                return value;
            }
        }

        public override string ToString()
        {
            return DisplayValue;
        }
    }
}
