using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
