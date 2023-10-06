// ------------------------------------------------------------
// The MIT License (MIT)
//
// Copyright (c) 2022 Neoxio
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------

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
