using Eto.Forms;

namespace OpenTabletDriver.UX
{
    public abstract class ChildDialog : Dialog
    {
        protected ChildDialog(Window parentWindow)
        {
            Owner = parentWindow;
            Title = App.ProductName;
            Icon = App.Logo.WithSize(App.Logo.Size);
        }
    }
}
