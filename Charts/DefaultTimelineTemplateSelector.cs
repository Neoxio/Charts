using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Neoxio.Charts
{
    internal class DefaultTimelineTemplateSelector : DataTemplateSelector
    {
        private DataTemplate _eventTemplate;
        private DataTemplate _rangeTemplate;

        public DefaultTimelineTemplateSelector()
        {
            SolidColorBrush eventBg = new SolidColorBrush(Colors.Green);
            SolidColorBrush rangeBg = new SolidColorBrush(Color.FromArgb(0x99, 0x00, 0x80, 0x00));

            var ellipseFactory = new FrameworkElementFactory(typeof(Ellipse));
            ellipseFactory.SetValue(Ellipse.WidthProperty, 6.0);
            ellipseFactory.SetValue(Ellipse.HeightProperty, 6.0);
            ellipseFactory.SetValue(Ellipse.FillProperty, eventBg);

            _eventTemplate = new DataTemplate() { VisualTree = ellipseFactory };

            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, rangeBg);
            borderFactory.SetValue(Border.HeightProperty, 10.0);

            _rangeTemplate = new DataTemplate() { VisualTree = borderFactory };
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is TimelineItem timeItem && timeItem.EventDate.HasValue)
            {
                if (timeItem.EventDuration.HasValue)
                    return _rangeTemplate;
                else
                    return _eventTemplate;
            }

            return null;
        }
    }
}
