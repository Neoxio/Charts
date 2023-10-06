// -----------------------------------------------------------------
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
// -----------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Neoxio.Charts
{
    /// <summary>
    /// Represents a control that can be used to display a collection of dated items on a timeline.
    /// </summary>
    [TemplatePart(Name = "PART_ScaleLine", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_LineScroller", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "PART_ScrollerContent", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_ItemsPresenter", Type = typeof(ItemsPresenter))]
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
    public class Timeline : ItemsControl
    {
        #region Dependency properties

        /// <summary>
        /// Identifies the <see cref="TimeUnit"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TimeUnitProperty = DependencyProperty.Register(
                  nameof(TimeUnit),
                  typeof(TimeSpan?),
                  typeof(Timeline),
                  new PropertyMetadata(null, OnTimeUnitChanged)
        );

        /// <summary>
        /// Identifies the <see cref="StartDate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StartDateProperty = DependencyProperty.Register(
                  nameof(StartDate),
                  typeof(DateTime?),
                  typeof(Timeline),
                  new PropertyMetadata(null, OnDatesChanged)
        );

        /// <summary>
        /// Identifies the <see cref="EndDate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EndDateProperty = DependencyProperty.Register(
                  nameof(EndDate),
                  typeof(DateTime?),
                  typeof(Timeline),
                  new PropertyMetadata(null, OnDatesChanged)
        );

        /// <summary>
        /// Identifies the <see cref="CurrentDate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentDateProperty = DependencyProperty.Register(
                  nameof(CurrentDate),
                  typeof(DateTime?),
                  typeof(Timeline),
                  new PropertyMetadata(null, OnCurrentDateChanged)
        );

        /// <summary>
        /// Identifies the <see cref="FlyoutTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FlyoutTemplateProperty = DependencyProperty.Register(
                nameof(FlyoutTemplate),
                typeof(DataTemplate),
                typeof(Timeline),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFlyoutTemplateChanged))
        );

        /// <summary>
        /// Identifies the <see cref="DateFormat"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DateFormatProperty = DependencyProperty.Register(
                nameof(DateFormat),
                typeof(string),
                typeof(Timeline),
                new FrameworkPropertyMetadata("{0:HH':'mm}", new PropertyChangedCallback(OnDateFormatChanged))
        );

        #endregion

        private const double MaxCanvasSize = 16 * 1e6;
        private const double MinMinuteUnit = 1;
        private const double MouseFlyoutOffset = 5;

        private const double TimeTickWidth = 1;
        private const double TimeTickHeight = 10;
        private double TimeLegendSize { get; set; }
        private double TimeLegendMidSize { get { return TimeLegendSize / 2.0; } } 
        private double DesiredChunkSize  { get { return Math.Max(TimeLegendSize + 30, 60); } }
        private double DisplayToleranceOnSides { get { return DesiredChunkSize * 4; } }


        private static DataTemplateSelector DefaultTemplateSelector = new DefaultTimelineTemplateSelector();

        private Canvas _scaleLine;
        private ScrollViewer _scrollViewer;
        private Canvas _scrollerContent;
        private ItemsPresenter _itemsPresenter;

        private BindingBase _eventDateBinding;
        private BindingBase _eventDurationBinding;

        private Popup _flyout;
        private ContentControl _flyoutContent;
        private bool _ignoreGraduationsUpdates = false;
        private bool _invalidTimeUnit = false;
        private bool _controlTemplateIsValid = false;
        private FrameworkElement _currentDateVisual;

        /// <summary>
        /// Gets or sets the prefered unit of time that a segment of line should represent on the timeline.
        /// </summary>
        /// <remarks>
        /// The time unit used may be greater than the time unit defined.
        /// See <see cref="ActualTimeUnit"/> to get the actual time unit.
        /// </remarks>
        public TimeSpan? TimeUnit
        {
            get { return (TimeSpan?)GetValue(TimeUnitProperty); }
            set { SetValue(TimeUnitProperty, value); }
        }

        /// <summary>
        /// Gets the minimum value that can be used as time unit (see <see cref="TimeUnit"/>).
        /// </summary>
        public TimeSpan MinTimeUnit { get { return TimeSpan.FromMinutes(MinMinuteUnit); } }

        /// <summary>
        /// Gets the unit of time that is represented by a segment of line in the timeline.
        /// </summary>
        public TimeSpan ActualTimeUnit { get; private set; }

        /// <summary>
        /// Gets or sets the start date of the time line.
        /// </summary>
        public DateTime? StartDate
        {
            get { return (DateTime?)GetValue(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the end date of the time line.
        /// </summary>
        public DateTime? EndDate
        {
            get { return (DateTime?)GetValue(EndDateProperty); }
            set { SetValue(EndDateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the date that should be marked as current on the timeline.
        /// </summary>
        public DateTime? CurrentDate
        {
            get { return (DateTime?)GetValue(CurrentDateProperty); }
            set { SetValue(CurrentDateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> used to display the flyout opened when an item is clicked.
        /// </summary>
        public DataTemplate FlyoutTemplate
        {
            get { return (DataTemplate)GetValue(FlyoutTemplateProperty); }
            set { SetValue(FlyoutTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the format that is used for each date along the timeline.
        /// </summary>
        /// <remarks>Format usage : string.Format(FORMAT, date)</remarks>
        public string DateFormat
        {
            get { return (string)GetValue(DateFormatProperty); }
            set { SetValue(DateFormatProperty, value); }
        }

        /// <summary>
        /// Indicates whether zoom is enabled on this timeline.
        /// </summary>
        public bool IsZoomEnabled { get; set; }

        public BindingBase EventDateBinding
        {
            get { return _eventDateBinding; }
            set
            {
                VerifyAccess();
                if (_eventDateBinding != value)
                {
                    _eventDateBinding = value;
                    OnEventDateBindingChanged();
                }
            }
        }

        public BindingBase EventDurationBinding
        {
            get { return _eventDurationBinding; }
            set
            {
                VerifyAccess();
                if (_eventDurationBinding != value)
                {
                    _eventDurationBinding = value;
                    OnEventDurationBindingChanged();
                }
            }
        }

        public TimeLineItemVisualSelector ItemVisualSelector { get; set; }

        /// <summary>
        /// Intializes a new instance of the <see cref="Timeline"/> class.
        /// </summary>
        public Timeline()
        {
            _flyoutContent = new ContentControl();
            SizeChanged += OnSizeChanged;
        }

        static Timeline()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(typeof(Timeline)));
            ForegroundProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(OnForegroundChanged));
        }

        #region On dependency property updates

        private static void OnTimeUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timeline = (Timeline)d;
            timeline.ComputeWidthForCurrentTimeUnit();
        }

        private static void OnDatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timeline = (Timeline)d;
            if (!timeline._controlTemplateIsValid)
                return;
            try
            {
                timeline._ignoreGraduationsUpdates = true;
                timeline.ComputeWidthForCurrentTimeUnit();
            }
            finally
            {
                timeline._ignoreGraduationsUpdates = false;
            }
            timeline.UpdateItemsPosition();
            timeline.ComputeGraduations(new Size(timeline._scrollerContent.ActualWidth, timeline._scrollerContent.ActualHeight));
        }

        private static void OnCurrentDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timeline = (Timeline)d;
            DateTime? currentDate = timeline.CurrentDate;
            if (currentDate.HasValue)
            {
                if (timeline._currentDateVisual == null)
                {
                    timeline._currentDateVisual = timeline.GetCurrentDateVisual();
                    if (timeline._controlTemplateIsValid)
                    {
                        timeline._scrollerContent.Children.Add(timeline._currentDateVisual);
                        timeline.UpdateCurrentDatePosition();
                    }
                }
                else if (timeline._controlTemplateIsValid)
                {
                    timeline.UpdateCurrentDatePosition();
                }
            }
            else
            {
                if (timeline._controlTemplateIsValid && timeline._currentDateVisual != null)
                {
                    timeline._scrollerContent.Children.Remove(timeline._currentDateVisual);
                }
                timeline._currentDateVisual = null;
            }
        }

        private static void OnFlyoutTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timeline = (Timeline)d;
            timeline.OnFlyoutTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
        }

        private static void OnDateFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timeline = (Timeline)d;
            timeline.ComputeLegendSize();
            if (timeline._controlTemplateIsValid)
            {
                timeline.ComputeGraduations(new Size(timeline._scrollerContent.ActualWidth, timeline._scrollerContent.ActualHeight));
            }
        }

        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timeline = (Timeline)d;
            if (!timeline._controlTemplateIsValid)
                return;
            timeline.ComputeGraduations(new Size(timeline._scrollerContent.ActualWidth, timeline._scrollerContent.ActualHeight));
        }

        #endregion

        /// <summary>
        /// Determines whether the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>true if the item is (or is eligible to be) its own container; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TimelineItem;
        }

        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>The element that is used to display the given item.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TimelineItem();
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">The element that's used to display the specified item.</param>
        /// <param name="item">The item to display.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var timelineItem = (TimelineItem)element;

            if (EventDateBinding != null)
                BindingOperations.SetBinding(timelineItem, TimelineItem.EventDateProperty, EventDateBinding);
            if (EventDurationBinding != null)
                BindingOperations.SetBinding(timelineItem, TimelineItem.EventDurationProperty, EventDurationBinding);

            if (!(item is UIElement))
            {
                timelineItem.Content = item;

                if (ItemTemplate != null)
                {
                    timelineItem.ContentTemplate = ItemTemplate;
                }
                else if (ItemTemplateSelector != null)
                {
                    timelineItem.ContentTemplate = ItemTemplateSelector.SelectTemplate(item, element);
                }
                else
                {
                    timelineItem.ContentTemplate = DefaultTemplateSelector.SelectTemplate(item, element);
                }
            }

            timelineItem.ParentTimeline = this;
            timelineItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            if (_controlTemplateIsValid && StartDate.HasValue && EndDate.HasValue && StartDate.Value < EndDate.Value)
            {
                DateTime lineStartDate = FloorDate(StartDate.Value);
                DateTime lineEndDate = CeilingDate(EndDate.Value);
                SetPositionForItem(timelineItem, timelineItem.DesiredSize.Width, timelineItem.DesiredSize.Height, lineStartDate, lineEndDate);
            }
        }

        protected override void OnItemsPanelChanged(ItemsPanelTemplate oldItemsPanel, ItemsPanelTemplate newItemsPanel)
        {
            base.OnItemsPanelChanged(oldItemsPanel, newItemsPanel);
        }

        /// <summary>
        /// Undoes the effects of the <see cref="PrepareContainerForItemOverride"/> method.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The item.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var timelineItem = (TimelineItem)element;
            timelineItem.ParentTimeline = null;
            if (EventDateBinding != null)
                BindingOperations.ClearBinding(timelineItem, TimelineItem.EventDateProperty);
            if (EventDurationBinding != null)
                BindingOperations.ClearBinding(timelineItem, TimelineItem.EventDurationProperty);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (IsZoomEnabled && Keyboard.Modifiers == ModifierKeys.Control)
            {
                double mouseDelta = e.Delta;
                double zoomFactor = -1 * Math.Sign(mouseDelta) * Math.Max(1, Math.Abs(mouseDelta / 120));

                TimeSpan newUnit;
                if (ActualTimeUnit.Hours > 1)
                {
                    newUnit = ActualTimeUnit.Add(TimeSpan.FromMinutes(15 * zoomFactor));
                }
                else
                {
                    newUnit = ActualTimeUnit.Add(TimeSpan.FromMilliseconds(MinTimeUnit.TotalMilliseconds * zoomFactor));
                }
                if (newUnit < MinTimeUnit)
                    newUnit = MinTimeUnit;
                TimeUnit = newUnit;
                e.Handled = true;
            }
            base.OnPreviewMouseWheel(e);
        }

        /// <summary>
        /// Called when an item displayed on the timeline is pressed.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Pointer event args</param>
        internal void OnTimelineItemPressed(TimelineItem item, MouseButtonEventArgs e)
        {
            if (item == null || _flyout == null || _flyoutContent.ContentTemplate == null)
                return;

            _flyoutContent.Content = item.Content;
            _flyout.PlacementTarget = item;
            e.Handled = true;
            _flyout.IsOpen = true;
        }

        internal void InvalidateItemPosition(TimelineItem timelineItem)
        {
            if (_controlTemplateIsValid && StartDate.HasValue && EndDate.HasValue && StartDate.Value < EndDate.Value)
            {
                DateTime lineStartDate = FloorDate(StartDate.Value);
                DateTime lineEndDate = CeilingDate(EndDate.Value);
                SetPositionForItem(timelineItem, timelineItem.DesiredSize.Width, timelineItem.DesiredSize.Height, lineStartDate, lineEndDate);
            }
        }

        private void OnFlyoutTemplateChanged(DataTemplate oldTemplate, DataTemplate newTemplate)
        {
            _flyoutContent.ContentTemplate = newTemplate;
        }

        private void OnFlyoutContentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_flyout.IsVisible)
                return;
            _flyout.HorizontalOffset = e.NewSize.Width / -2;
            _flyout.VerticalOffset = -1 * (e.NewSize.Height + MouseFlyoutOffset);
        }

        private void ComputeLegendSize()
        {
            string s = string.Format(DateFormat, DateTime.MaxValue);
            var legend = new TextBlock() { Text = s, FontSize = FontSize };
            legend.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            TimeLegendSize = legend.DesiredSize.Width;
        }

        /// <summary>
        /// Sets the position of a given item on the timeline.
        /// </summary>
        /// <param name="timeLineItem">Item to position.</param>
        /// <param name="itemWidth">Item width (used to center the item's visual).</param>
        /// <param name="itemHeight">Item height (used to center the item's visual).</param>
        /// <param name="lineStartDate">Start date of the timeline.</param>
        /// <param name="lineEndDate">End date of the timeline.</param>
        private void SetPositionForItem(TimelineItem timeLineItem, double itemWidth, double itemHeight, DateTime lineStartDate, DateTime lineEndDate)
        {
            DateTime time;
            if (!timeLineItem.EventDate.HasValue || _scrollerContent.ActualWidth <= 0)
            {
                time = lineStartDate;
            }
            else
            {
                time = timeLineItem.EventDate.Value;
            }

            if (time < lineStartDate || time > lineEndDate)
            {
                Canvas.SetLeft(timeLineItem, -2000);
                return;
            }

            double range = (lineEndDate - lineStartDate).TotalMinutes;
            double leftOffset = (time - lineStartDate).TotalMinutes * (_scrollerContent.ActualWidth) / range;
            if (timeLineItem.EventDuration.HasValue)
            {
                itemWidth = 0;//Ignore width
                double rightOffset = (time.Add(timeLineItem.EventDuration.Value) - lineStartDate).TotalMinutes * (_scrollerContent.ActualWidth) / range;
                timeLineItem.Width = Math.Max(1, rightOffset - leftOffset);
                timeLineItem.MaxWidth = Math.Max(1, rightOffset - leftOffset);
            }

            Canvas.SetLeft(timeLineItem, leftOffset - (itemWidth - TimeTickWidth) / 2);
            Canvas.SetTop(timeLineItem, (TimeTickHeight - itemHeight) / 2 + Padding.Top);
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (_controlTemplateIsValid)
            {
                _scrollerContent.SizeChanged -= OnScrollContentSizeChanged;
                _scrollViewer.ScrollChanged -= OnScrollViewChanged;
                if (_currentDateVisual != null)
                {
                    _scrollerContent.Children.Remove(_currentDateVisual);
                }
            }

            _scaleLine = GetTemplateChild("PART_ScaleLine") as Canvas;
            _scrollViewer = GetTemplateChild("PART_LineScroller") as ScrollViewer;
            _scrollerContent = GetTemplateChild("PART_ScrollerContent") as Canvas;
            _itemsPresenter = GetTemplateChild("PART_ItemsPresenter") as ItemsPresenter;
            _flyout = GetTemplateChild("PART_Popup") as Popup;
            if (_flyout != null)
            {
                _flyoutContent = new ContentControl() { ContentTemplate = FlyoutTemplate };
                _flyoutContent.SizeChanged += OnFlyoutContentSizeChanged;
                _flyout.HorizontalOffset = _flyoutContent.ActualWidth / -2;
                _flyout.VerticalOffset = -1 * (_flyoutContent.ActualHeight + MouseFlyoutOffset);
                _flyout.Child = _flyoutContent;
            }
            _controlTemplateIsValid = _scaleLine != null && _scrollViewer != null && _scrollerContent != null && _itemsPresenter != null;

            if (_controlTemplateIsValid)
            {
                _scrollerContent.SizeChanged += OnScrollContentSizeChanged;
                _scrollViewer.ScrollChanged += OnScrollViewChanged;
                if (_currentDateVisual != null)
                {
                    _scrollerContent.Children.Add(_currentDateVisual);
                }
            }
            if (TimeLegendSize == 0)
            {
                ComputeLegendSize();
            }

            ComputeWidthForCurrentTimeUnit();
        }

        /// <summary>
        /// Called when the size of the timeline has changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void OnScrollContentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ComputeGraduations(e.NewSize);
            UpdateItemsPosition();
            UpdateCurrentDatePosition();
        }

        /// <summary>
        /// Called when the size of this control has changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TimeUnit.HasValue && _controlTemplateIsValid)
            {
                ComputeGraduations(new Size(_scrollerContent.ActualWidth, _scrollerContent.ActualHeight));
                UpdateCurrentDatePosition();
            }
        }

        /// <summary>
        /// Called when the timeline is scrolled.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void OnScrollViewChanged(object sender, ScrollChangedEventArgs e)
        {
            ComputeGraduations(new Size(_scrollerContent.ActualWidth, _scrollerContent.ActualHeight));
        }

        /// <summary>
        /// Gets the visual representation of a time tick on the timeline.
        /// </summary>
        /// <returns>Visual representation of a time tick.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private FrameworkElement GetTimeTick()
        {
            return new Rectangle
            {
                Width = TimeTickWidth,
                Height = TimeTickHeight,
                Fill = Foreground
            };
        }

        /// <summary>
        /// Gets the text that should be displayed on the timeline for a given date.
        /// </summary>
        /// <param name="date">Date to display.</param>
        /// <returns>Visual representation of the date.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private FrameworkElement GetText(DateTime date)
        {
            return new TextBlock()
            {
                Text = string.Format(DateFormat, date)
            };
        }

        /// <summary>
        /// Gets the visual representation that should be used to display the current date on the timeline.
        /// </summary>
        /// <returns>Visual representation of the current date.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private FrameworkElement GetCurrentDateVisual()
        {
            var rect = new Rectangle
            {
                Width = 3,
                Height = 20,
                Fill = new SolidColorBrush(Colors.DarkOrange)
            };
            return rect;
        }

        /// <summary>
        /// Gets the largest date that has no seconds and is smaller or equal to the specified date.
        /// </summary>
        /// <param name="date">A date.</param>
        /// <returns>The largest date that has no seconds and is smaller or equal to the specified date.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateTime FloorDate(DateTime date)
        {
            return date.AddSeconds(-date.Second);
        }

        /// <summary>
        /// Gets the smallest date that has no seconds and is greater or equal to the specified date.
        /// </summary>
        /// <param name="date">A date.</param>
        /// <returns>The smallest date that has no seconds and is greater or equal to the specified date.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateTime CeilingDate(DateTime date)
        {
            if (date.Second > 0)
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0).AddMinutes(1);
            return date;
        }

        /// <summary>
        /// Computes the width that is required to display the timeline taking <see cref="TimeUnit"/>,
        /// <see cref="StartDate"/> and <see cref="EndDate"/> parameters into account.
        /// </summary>
        private void ComputeWidthForCurrentTimeUnit()
        {
            if (!_controlTemplateIsValid)
                return;
            if (TimeUnit.HasValue && StartDate.HasValue && EndDate.HasValue && StartDate.Value < EndDate.Value)
            {
                double timeUnitInMinutes = Math.Round(TimeUnit.Value.TotalMinutes);
                if (timeUnitInMinutes < MinMinuteUnit)
                    timeUnitInMinutes = MinMinuteUnit;

                DateTime lineStartDate = FloorDate(StartDate.Value);
                DateTime lineEndDate = CeilingDate(EndDate.Value);

                double minutesToSpread = (lineEndDate - lineStartDate).TotalMinutes;
                double nbChunk = minutesToSpread / timeUnitInMinutes;
                if (nbChunk < 1)
                {
                    nbChunk = 1;
                }

                double width = nbChunk * DesiredChunkSize;
                if (width > MaxCanvasSize)
                {
                    _invalidTimeUnit = true;
                    _scrollerContent.SetValue(Canvas.WidthProperty, MaxCanvasSize);
                }
                else
                {
                    _invalidTimeUnit = false;
                    _scrollerContent.SetValue(Canvas.WidthProperty, width);
                }
            }
            else
            {
                _scrollerContent.ClearValue(Canvas.WidthProperty);
            }
        }

        private void GetScrollInfo(Size elementSize, out double leftOffset, out double visibleWidth)
        {
            Point offset = _scrollerContent.TransformToVisual(_scrollViewer).Transform(new Point(0, 0));
            leftOffset = offset.X;
            visibleWidth = _scrollViewer.ActualWidth;
        }

        /// <summary>
        /// Updates the position of the visual representing the current date (<see cref="CurrentDate"/>).
        /// </summary>
        private void UpdateCurrentDatePosition()
        {
            if (_currentDateVisual == null || !StartDate.HasValue || !EndDate.HasValue || StartDate.Value > EndDate.Value)
            {
                return;
            }

            if (StartDate.Value <= CurrentDate.Value)
            {
                DateTime lineStartDate = FloorDate(StartDate.Value);
                DateTime lineEndDate = CeilingDate(EndDate.Value);
                double currentMinutes = (CurrentDate.Value - lineStartDate).TotalMinutes;
                double minutesToSpread = (lineEndDate - lineStartDate).TotalMinutes;
                double leftOffset = _scrollerContent.ActualWidth * currentMinutes / minutesToSpread;
                Canvas.SetLeft(_currentDateVisual, leftOffset);
                Canvas.SetTop(_currentDateVisual, (TimeTickHeight - _currentDateVisual.ActualHeight) / 2 + Padding.Top);
            }
            else
            {
                Canvas.SetLeft(_currentDateVisual, -200);
            }
        }

        private void OnEventDateBindingChanged()
        {
            if (_controlTemplateIsValid)
            {
                SetBindings(EventDateBinding, TimelineItem.EventDateProperty);
            }
        }

        private void OnEventDurationBindingChanged()
        {
            if (_controlTemplateIsValid)
            {
                SetBindings(EventDurationBinding, TimelineItem.EventDurationProperty);
            }
        }

        private void SetBindings(BindingBase binding, DependencyProperty property)
        {
            if (binding == null)
            {
                if (VisualTreeHelper.GetChild(_itemsPresenter, 0) is Panel itemsPanel)
                {
                    foreach (TimelineItem timelineItem in itemsPanel.Children)
                    {
                        BindingOperations.ClearBinding(timelineItem, property);
                    }
                }
            }
            else
            {
                if (VisualTreeHelper.GetChild(_itemsPresenter, 0) is Panel itemsPanel)
                {
                    foreach (TimelineItem timelineItem in itemsPanel.Children)
                    {
                        BindingOperations.SetBinding(timelineItem, property, binding);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the position of all items displayed on the timeline.
        /// </summary>
        private void UpdateItemsPosition()
        {
            if (_ignoreGraduationsUpdates || !StartDate.HasValue || !EndDate.HasValue || StartDate.Value > EndDate.Value)
            {
                return;
            }

            DateTime lineStartDate = FloorDate(StartDate.Value);
            DateTime lineEndDate = CeilingDate(EndDate.Value);

            if (VisualTreeHelper.GetChild(_itemsPresenter, 0) is Panel itemsPanel)
            {
                foreach (TimelineItem timeLineItem in itemsPanel.Children)
                {
                    SetPositionForItem(timeLineItem, timeLineItem.DesiredSize.Width, timeLineItem.DesiredSize.Height, lineStartDate, lineEndDate);
                }
            }
        }

        /// <summary>
        /// Computes the visual and position of the graduations on the timeline.
        /// </summary>
        /// <param name="size">Size of the timeline.</param>
        private void ComputeGraduations(Size size)
        {
            if (_ignoreGraduationsUpdates)
                return;

            _scaleLine.Children.Clear();

            if (!StartDate.HasValue || !EndDate.HasValue || StartDate.Value > EndDate.Value || size.Width <= 0)
            {
                return;
            }

            DateTime lineStartDate = FloorDate(StartDate.Value);
            DateTime lineEndDate = CeilingDate(EndDate.Value);

            int nbChunk = (int)(size.Width / DesiredChunkSize);
            double minutesToSpread = (lineEndDate - lineStartDate).TotalMinutes;

            double chunkTimeInMin;
            if (TimeUnit.HasValue && !_invalidTimeUnit)
                chunkTimeInMin = TimeUnit.Value.TotalMinutes;
            else
                chunkTimeInMin = Math.Ceiling(minutesToSpread / nbChunk);

            if (chunkTimeInMin < 1)
                chunkTimeInMin = 1;

            if (nbChunk > 0)
                ActualTimeUnit = TimeSpan.FromMinutes(chunkTimeInMin);
            else
                ActualTimeUnit = MinTimeUnit;

            GetScrollInfo(size, out double parentOffsetLeft, out double parentWidth);
            _scaleLine.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, _scrollViewer.ActualWidth, _scaleLine.ActualHeight)
            };

            // Draw horizontal line
            double lineWidth = parentWidth + DisplayToleranceOnSides;
            double lineLeftOffset = Math.Max(0, -1 * parentOffsetLeft);

            if (lineWidth + lineLeftOffset > size.Width)
            {
                lineWidth = Math.Max(0, size.Width - lineLeftOffset);
                //lineWidth -= lineWidth + lineLeftOffset - size.Width;
            }

            Rectangle line = new Rectangle() { Width = lineWidth, Height = 1, Fill = Foreground };
            Canvas.SetTop(line, (TimeTickHeight - 1) / 2 + Padding.Top);
            Canvas.SetLeft(line, parentOffsetLeft + lineLeftOffset);
            _scaleLine.Children.Add(line);

            // Start Date
            var tick = GetTimeTick();
            var txt = GetText(lineStartDate);
            Canvas.SetLeft(tick, parentOffsetLeft);
            Canvas.SetTop(tick, Padding.Top);
            Canvas.SetTop(txt, TimeTickHeight + Padding.Top);
            Canvas.SetLeft(txt, parentOffsetLeft - TimeLegendMidSize);
            _scaleLine.Children.Add(tick);
            _scaleLine.Children.Add(txt);

            double sizeRatio = size.Width / minutesToSpread;

            // Compute starting chunk to ignore hidden chunks
            double startingChunkMin;
            double maxOffset;
            if (parentOffsetLeft < 0)
            {
                double targetBeginOffset = -1 * (parentOffsetLeft + DisplayToleranceOnSides);
                double targetMin = targetBeginOffset / sizeRatio;
                startingChunkMin = Math.Ceiling(targetMin / chunkTimeInMin) * chunkTimeInMin;
                maxOffset = Math.Max(targetBeginOffset, 0) + parentWidth + DisplayToleranceOnSides;
            }
            else
            {
                startingChunkMin = 0;
                maxOffset = parentWidth + DisplayToleranceOnSides;
            }

            double currentChunkMin = Math.Max(0, startingChunkMin);
            DateTime date = lineStartDate.AddMinutes(currentChunkMin);
            double decalageLeft = 0;
            // Middle chunks
            while (currentChunkMin + chunkTimeInMin < minutesToSpread && decalageLeft < maxOffset)
            {
                currentChunkMin += chunkTimeInMin;
                date = date.AddMinutes(chunkTimeInMin);
                decalageLeft = Math.Ceiling(currentChunkMin) * sizeRatio;

                if (decalageLeft + TimeLegendSize > size.Width)
                    continue;

                tick = GetTimeTick();
                txt = GetText(date);
                Canvas.SetLeft(tick, parentOffsetLeft + decalageLeft);
                Canvas.SetTop(tick, Padding.Top);
                Canvas.SetLeft(txt, parentOffsetLeft + decalageLeft - TimeLegendMidSize);
                Canvas.SetTop(txt, TimeTickHeight + Padding.Top);
                _scaleLine.Children.Add(tick);
                _scaleLine.Children.Add(txt);
            }

            // End date
            tick = GetTimeTick();
            txt = GetText(lineEndDate);
            Canvas.SetTop(txt, TimeTickHeight + Padding.Top);
            Canvas.SetTop(tick, Padding.Top);
            Canvas.SetLeft(tick, parentOffsetLeft + size.Width - TimeTickWidth);
            Canvas.SetLeft(txt, parentOffsetLeft + size.Width - TimeLegendMidSize);
            _scaleLine.Children.Add(tick);
            _scaleLine.Children.Add(txt);
        }
    }
}
