using System.Data;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Nightmare.UI;

public class RequestView : FrameView
{
    private readonly Label _methodLabel;
    private readonly Label _requestUrlLabel;

    private readonly DataTable _headersTable = new();
    private readonly DataTable _cookiesTable = new();
    private readonly TextView _bodyView = new();

    public RequestView()
    {
        Title = "Request";
        Width = Dim.Fill();
        Height = Dim.Percent(45);
        Arrangement = ViewArrangement.Resizable;

        _methodLabel = new Label();
        _methodLabel.SetScheme(
            new Scheme
            {
                Normal = new Terminal.Gui.Drawing.Attribute
                {
                    Foreground = Color.Green
                }
            }
        );

        _requestUrlLabel = new Label
        {
            X = Pos.Right(_methodLabel) + 1,
            Width = Dim.Fill()
        };
        _requestUrlLabel.SetScheme(
            new Scheme
            {
                Normal = new Terminal.Gui.Drawing.Attribute
                {
                    Foreground = Color.Cyan
                }
            }
        );

        var tabView = new TabView
        {
            Y = Pos.Bottom(_methodLabel) + 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var headersTableView = new TableView { Width = Dim.Fill(), Height = Dim.Fill() };
        tabView.AddTab(
            new Tab
            {
                DisplayText = "_Headers",
                View = headersTableView
            },
            false
        );

        var cookiesTableView = new TableView { Width = Dim.Fill(), Height = Dim.Fill() };
        tabView.AddTab(
            new Tab
            {
                DisplayText = "_Cookies",
                View = cookiesTableView
            },
            false
        );

        _headersTable.Columns.Add("Key", typeof(string));
        _headersTable.Columns.Add("Value", typeof(string));
        headersTableView.Table = new DataTableSource(_headersTable);

        _cookiesTable.Columns.Add("Key", typeof(string));
        _cookiesTable.Columns.Add("Value", typeof(string));
        cookiesTableView.Table = new DataTableSource(_cookiesTable);

        Add(_methodLabel, _requestUrlLabel, tabView);
    }
}