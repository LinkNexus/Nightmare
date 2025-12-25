using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Nightmare.UI;

public class ErrorPopup : Dialog
{
    public readonly Label ErrorLabel;

    public ErrorPopup()
    {
        Height = Dim.Auto();
        Width = Dim.Auto();

        ErrorLabel = new Label();

        Add(ErrorLabel);
    }
}