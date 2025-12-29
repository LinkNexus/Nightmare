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
    private readonly DataTable _queryTable = new();
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

        var queryTableView = new TableView { Width = Dim.Fill(), Height = Dim.Fill() };
        tabView.Add(
            new Tab
            {
                DisplayText = "_Query",
                View = queryTableView
            }
        );

        _headersTable.Columns.Add("Key", typeof(string));
        _headersTable.Columns.Add("Value", typeof(string));
        headersTableView.Table = new DataTableSource(_headersTable);

        _cookiesTable.Columns.Add("Key", typeof(string));
        _cookiesTable.Columns.Add("Value", typeof(string));
        cookiesTableView.Table = new DataTableSource(_cookiesTable);

        _queryTable.Columns.Add("Key", typeof(string));
        _queryTable.Columns.Add("Value", typeof(string));
        queryTableView.Table = new DataTableSource(_queryTable);

        Add(_methodLabel, _requestUrlLabel, tabView);
    }

    public void OnRequestSelected(HttpRequestMessage request)
    {
        _methodLabel.Text = request.Method.ToString();
        if (request.RequestUri is not null)
            _requestUrlLabel.Text = request.RequestUri.GetLeftPart(UriPartial.Path);

        _headersTable.Clear();
        _cookiesTable.Clear();
        _queryTable.Clear();

        foreach (var (key, value) in request.Headers.Where(h =>
                     !h.Key.Equals("cookie", StringComparison.InvariantCultureIgnoreCase)))
            _headersTable.Rows.Add(key, value);

        if (request.Content != null)
            foreach (var (key, value) in request.Content.Headers)
                _headersTable.Rows.Add(key, value);

        if (request.Headers.TryGetValues("cookie", out var cookieString))
        {
            var cookies = cookieString
                .Select(c => c.Split('='))
                .ToDictionary(c => c[0], c => c[1]);

            foreach (var (key, value) in cookies)
                _cookiesTable.Rows.Add(key, value);
        }

        var query = request.RequestUri?.Query.TrimStart('?');
        if (query is not null)
        {
            var queryParams = query.Split('&');
            foreach (var param in queryParams)
            {
                var parts = param.Split('=');
                _queryTable.Rows.Add(parts[0], parts[1]);
            }
        }
    }
}