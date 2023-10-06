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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Neoxio.Charts
{
    /// <summary>
    /// Represents the container for an item in a <see cref="Timeline"/> control.
    /// </summary>
    public class TimelineItem : ContentControl
    {
        #region Dependency properties

        /// <summary>
        /// Identifies the <see cref="EventDate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EventDateProperty = DependencyProperty.Register(
                  nameof(EventDate),
                  typeof(DateTime?),
                  typeof(TimelineItem),
                  new PropertyMetadata(null, OnEventDataChanged)
        );

        /// <summary>
        /// Identifies the <see cref="EventDate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EventDurationProperty = DependencyProperty.Register(
                  nameof(EventDuration),
                  typeof(TimeSpan?),
                  typeof(TimelineItem),
                  new PropertyMetadata(null, OnEventDataChanged)
        );

        #endregion

        public DateTime? EventDate
        {
            get { return (DateTime?)GetValue(EventDateProperty); }
            set { SetValue(EventDateProperty, value); }
        }

        public TimeSpan? EventDuration
        {
            get { return (TimeSpan?)GetValue(EventDurationProperty); }
            set { SetValue(EventDurationProperty, value); }
        }

        internal Timeline ParentTimeline { get; set; }

        public TimelineItem()
        { }

        private static void OnEventDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelineItem item = (TimelineItem)d;
            item.ParentTimeline?.InvalidateItemPosition(item);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            ParentTimeline?.OnTimelineItemPressed(this, e);
            base.OnPreviewMouseUp(e);
        }

        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        //{
        //    base.OnRenderSizeChanged(sizeInfo);
        //}
    }
}
