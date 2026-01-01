using System.Data;
using System.Diagnostics;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using System.Net.Http;
using Nightmare.Parser.TemplateExpressions;

namespace Nightmare.UI;

public class ResponseView : FrameView
{
    private readonly Label _statusLabel;

    private readonly DataTable _headersTable = new();
    private readonly DataTable _cookiesTable = new();
    private readonly View _bodyContainerView;
    private TextView? _bodyTextView;

    public ResponseView()
    {
        Title = "Response";
        Width = Dim.Fill();
        Height = Dim.Fill();
        Arrangement = ViewArrangement.Resizable;

        _statusLabel = new Label();

        var tabView = new TabView
        {
            Y = Pos.Bottom(_statusLabel) + 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };


        var headersTableView = new TableView { Width = Dim.Fill(), Height = Dim.Fill() };
        tabView.AddTab(
            new Tab
            {
                DisplayText = "Headers",
                View = headersTableView
            },
            false
        );

        var cookiesTableView = new TableView { Width = Dim.Fill(), Height = Dim.Fill() };
        tabView.AddTab(
            new Tab
            {
                DisplayText = "Cookies",
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

        _bodyContainerView = new View
        {
            Title = "Body",
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        tabView.AddTab(
            new Tab
            {
                DisplayText = "Body",
                View = _bodyContainerView
            },
            false
        );

        Add(_statusLabel, tabView);
    }

    public async Task OnResponseReceived(Response response)
    {
        _statusLabel.Text = $"{response.StatusCode} {response.ReasonPhrase}";

        _headersTable.Clear();
        _cookiesTable.Clear();

        foreach (var (key, value) in response.Headers)
            _headersTable.Rows.Add(key, string.Join(", ", value));

        foreach (var (key, value) in response.Body.Headers)
            _headersTable.Rows.Add(key, string.Join(", ", value));

        foreach (var (key, value) in response.Cookies)
            _cookiesTable.Rows.Add(key, value);

        if (_bodyTextView is not null)
        {
            _bodyContainerView.Remove(_bodyTextView);
            _bodyTextView.Dispose();
        }

        var body = response.Content;

        _bodyTextView = new TextView { Text = body, WordWrap = true, Width = Dim.Fill(), Height = Dim.Fill() };
        _bodyContainerView.Remove(_bodyContainerView);
        _bodyContainerView.Add(_bodyTextView);
    }
}