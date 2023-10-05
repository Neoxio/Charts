using System.Windows.Controls;
using System.Windows.Input;

namespace Neoxio.Charts
{
    /// <summary>
    /// A <see cref="ScrollViewer"/> that scrolls horizontally when mouse wheel is used.
    /// </summary>
    public class HorizontalScrollViewer : ScrollViewer
    {
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Handled) { return; }

            if (ScrollInfo != null && ScrollInfo.ExtentWidth > ScrollInfo.ViewportWidth)
            {
                if (e.Delta < 0) { ScrollInfo.MouseWheelRight(); }
                else { ScrollInfo.MouseWheelLeft(); }

                e.Handled = true;
            }
        }
    }
}
