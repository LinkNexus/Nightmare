using System.Data;
using System.Diagnostics;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using System.Net.Http;
using Nightmare.Parser.TemplateExpressions;
using Terminal.Gui.Drawing;

namespace Nightmare.UI;

public class ResponseView : FrameView
{
    private readonly Label _statusLabel;

    private readonly DataTable _headersTable = new();
    private readonly DataTable _cookiesTable = new();
    private readonly TextView _bodyTextView;
    private readonly Label _responseTimeLabel;

    public ResponseView()
    {
        Title = "Response";
        Width = Dim.Fill();
        Height = Dim.Fill();
        Arrangement = ViewArrangement.Resizable;

        _statusLabel = new Label();

        _responseTimeLabel = new Label
        {
            Y = Pos.Top(_statusLabel),
            X = Pos.Right(_statusLabel) + 1
        };
        _responseTimeLabel.SetScheme(new Scheme
        {
            Normal = new Terminal.Gui.Drawing.Attribute
            {
                Foreground = Color.Cyan
            }
        });

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

        _bodyTextView = new TextView
        {
            Title = "Body",
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        tabView.AddTab(
            new Tab
            {
                DisplayText = "Body",
                View = _bodyTextView
            },
            false
        );

        Add(_statusLabel, _responseTimeLabel, tabView);
    }

    public void OnResponseReceived(Response response)
    {
        _statusLabel.Text = $"{response.StatusCode} {response.ReasonPhrase}";
        _responseTimeLabel.Text = $"{response.ResponseTimeMs} ms";

        _headersTable.Clear();
        _cookiesTable.Clear();

        foreach (var (key, value) in response.Headers)
            _headersTable.Rows.Add(key, string.Join(", ", value));

        foreach (var (key, value) in response.Cookies)
            _cookiesTable.Rows.Add(key, value);

        _bodyTextView.Text = response.Content;
    }
}