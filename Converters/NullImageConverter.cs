using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RimWorldModManager.Converters
{
    public class NullImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] is string path && !string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    return new BitmapImage(new Uri(path));
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
