using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SATCalculator.Core
{
    public class ValuationToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ValuationEnum? valuation = value as ValuationEnum?;

            if (valuation == ValuationEnum.True)
            {
                return Brushes.Green;
            }
            else if (valuation == ValuationEnum.False)
            {
                return Brushes.Red;
            }
            else
            {
                return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
