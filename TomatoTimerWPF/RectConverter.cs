using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows;

namespace TomatoTimerWPF
{
    class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values != null && values[0] != null && values[1] != null && values[2] != null)
            {
                double progressBarFillPercentage = (double)values[0];
                double textBlockActualyWidth = (double)values[1];
                double textBlockHeight = (double)values[2];
                return new Rect(0, 0, progressBarFillPercentage / 100.0 * textBlockActualyWidth, textBlockHeight); // ProgressBarFillWidth is calculated by multiplying Fill 
                // percentage with actual width
            }
            return new Rect(0, 0, 0, 0); // Default Zero size rectangle
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
