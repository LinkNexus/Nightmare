using System.Data;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using System.Net.Http;
using Nightmare.Parser.TemplateExpressions;

namespace Nightmare.UI;

public class RequestView : FrameView
{
    private readonly Label _methodLabel;
    private readonly Label _urlLabel;

    private readonly DataTable _headersTable = new();
    private readonly DataTable _cookiesTable = new();
    private readonly DataTable _queryTable = new();
    private readonly View _bodyContainerView;

    private View? _bodyView
    {
        get;
        set
        {
            field = value;
            _bodyContainerView.Add(value);
        }
    }

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

        _urlLabel = new Label
        {
            X = Pos.Right(_methodLabel) + 1,
            Width = Dim.Fill()
        };
        _urlLabel.SetScheme(
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

        var queryTableView = new TableView { Width = Dim.Fill(), Height = Dim.Fill() };
        tabView.AddTab(
            new Tab
            {
                DisplayText = "_Query",
                View = queryTableView
            },
            true
        );

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

        _queryTable.Columns.Add("Key", typeof(string));
        _queryTable.Columns.Add("Value", typeof(string));
        queryTableView.Table = new DataTableSource(_queryTable);

        _bodyContainerView = new View
        {
            Title = "Body",
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            CanFocus = true
        };
        tabView.AddTab(
            new Tab
            {
                DisplayText = "Body",
                View = _bodyContainerView
            },
            false
        );
        _bodyContainerView.Add(_bodyView);

        Add(_methodLabel, _urlLabel, tabView);
    }

    public void OnRequestSelected(Request request)
    {
        _methodLabel.Text = request.Method;
        _urlLabel.Text = request.Url;

        _headersTable.Clear();
        _cookiesTable.Clear();
        _queryTable.Clear();

        var headers = new Dictionary<string, string>();

        if (request.Body != null)
            foreach (var (key, value) in request.Body.Headers)
                headers[key] = string.Join("; ", value);

        foreach (var (key, value) in request.Headers.Where(h =>
                     !h.Key.Equals("cookie", StringComparison.InvariantCultureIgnoreCase)))
            headers[key] = string.Join("; ", value);

        foreach (var (k, v) in headers)
            _headersTable.Rows.Add(k, v);

        foreach (var (k, v) in request.Cookies) _cookiesTable.Rows.Add(k, v);

        foreach (var (k, v) in request.Query) _queryTable.Rows.Add(k, v);

        DisplayBody(request.Type, request.Content);
    }

    private void DisplayBody(string type, object? content)
    {
        if (_bodyView is not null)
        {
            _bodyContainerView.Remove(_bodyView);
            _bodyView.Dispose();
        }

        if (content is null)
            return;

        switch (type)
        {
            case "raw":
            {
                switch (content)
                {
                    case string str:
                        DisplayText(str);
                        break;
                    case FileReference fileRef:
                    {
                        DisplayText(
                            $"Raw file content.\n"
                            + $"File Path: {fileRef.Path}\n"
                        );

                        if (fileRef.FileName is not null)
                            DisplayText($"File Name: {fileRef.FileName}");

                        if (fileRef.ContentType is not null)
                            DisplayText($"Content Type: {fileRef.ContentType}");

                        break;
                    }
                }

                break;
            }

            case "text":
            case "json":
            {
                DisplayText((string)content);
                break;
            }

            case "form":
            {
                DisplayTableData((List<KeyValuePair<string, string>>)content);
                break;
            }

            case "multipart":
            {
                DisplayTableData(
                    ((Dictionary<string, object>)content)
                    .Select(i => new KeyValuePair<string, string>(
                            i.Key, i.Value.ToString()
                        )
                    )
                );
                break;
            }
        }

        return;

        void DisplayTableData(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            var table = new DataTable();
            table.Columns.Add("Key", typeof(string));
            table.Columns.Add("Value", typeof(string));

            foreach (var pair in pairs) table.Rows.Add(pair.Key, pair.Value);
            _bodyView = new TableView
            {
                Table = new DataTableSource(table),
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
        }

        void DisplayText(string text)
        {
            _bodyView = new TextView
            {
                Text = text,
                ReadOnly = true,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                WordWrap = true
            };
        }
    }
}