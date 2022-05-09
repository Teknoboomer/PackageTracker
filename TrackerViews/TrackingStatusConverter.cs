using System;
using System.Windows.Data;
using System.Windows.Media;
using TrackerModel;

namespace PackageTrackerWPF
{
    public class TrackingStatusConverter : IValueConverter
    {
        private SolidColorBrush _deliveredColor = new SolidColorBrush(Colors.PaleGreen);
        private SolidColorBrush _inTransitColor = new SolidColorBrush(Colors.LightCyan);
        private SolidColorBrush _noRecordColor = new SolidColorBrush(Colors.LightPink);
        private SolidColorBrush _errorColor = new SolidColorBrush(Colors.Tomato);
        private SolidColorBrush _lostColor = new SolidColorBrush(Colors.Gray);

        public object Convert(object status, Type targetType, object target, System.Globalization.CultureInfo culture)
        {
            switch ((TrackingRequestStatus)status)
            {
                case TrackingRequestStatus.Delivered:
                    return _deliveredColor;
                case TrackingRequestStatus.InTransit:
                    return _inTransitColor;
                case TrackingRequestStatus.NoRecord:
                    return _noRecordColor;
                case TrackingRequestStatus.InternalError:
                    return _errorColor;
                case TrackingRequestStatus.Lost:
                    return _lostColor;
            }

            return _errorColor;
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
