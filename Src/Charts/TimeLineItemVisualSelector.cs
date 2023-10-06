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
