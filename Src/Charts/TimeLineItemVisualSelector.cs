using System;

namespace Neoxio.Charts
{
    /// <summary>
    /// Provides a way to customize the visual of items on a <see cref="Timeline"/> control.
    /// </summary>
    public abstract class TimeLineItemVisualSelector
    {
        /// <summary>
        /// Creates or retrieves the visual that should be used to display the specified item
        /// and extracts the date represented by the item.
        /// </summary>
        /// <param name="item">Item to display.</param>
        /// <param name="visual">Visual to use for the specified item.</param>
        /// <param name="date">Date represented by the specified item.</param>
        public abstract void GetItemVisual(object item, out object visual, out DateTime? date);
    }
}
