using System.Data;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using System.Net.Http;

namespace Nightmare.UI;

public class RequestView : FrameView
{
    private readonly Label _methodLabel;
    private readonly Label _requestUrlLabel;

    private readonly DataTable _headersTable = new();
    private readonly DataTable _cookiesTable = new();
    private readonly DataTable _queryTable = new();
    private readonly View? _bodyContainerView;
    private View _bodyView = new Label { Text = "(No body)" };

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

        Add(_methodLabel, _requestUrlLabel, tabView);
    }

    public async Task OnRequestSelected(HttpRequestMessage request)
    {
        _methodLabel.Text = request.Method.ToString();
        if (request.RequestUri is not null)
            _requestUrlLabel.Text = request.RequestUri.GetLeftPart(UriPartial.Path);

        _headersTable.Clear();
        _cookiesTable.Clear();
        _queryTable.Clear();

        foreach (var (key, value) in request.Headers.Where(h =>
                     !h.Key.Equals("cookie", StringComparison.InvariantCultureIgnoreCase)))
            _headersTable.Rows.Add(key, string.Join(", ", value));

        if (request.Content != null)
            foreach (var (key, value) in request.Content.Headers)
                _headersTable.Rows.Add(key, string.Join(", ", value));

        if (request.Headers.TryGetValues("cookie", out var cookieString))
        {
            var cookies = cookieString
                .Select(c => c.Split('='))
                .ToDictionary(c => c[0], c => c[1]);

            foreach (var (key, value) in cookies)
                _cookiesTable.Rows.Add(key, value);
        }

        var query = request.RequestUri?.Query.TrimStart('?');
        if (!string.IsNullOrEmpty(query))
        {
            var queryParams = query.Split('&');
            foreach (var param in queryParams)
            {
                var parts = param.Split('=');
                _queryTable.Rows.Add(parts[0], parts[1]);
            }
        }

        // Display body
        await DisplayBody(request.Content);
    }

    private async Task DisplayBody(HttpContent? content)
    {
        // Clear previous body view
        if (_bodyView != null && _bodyContainerView != null)
        {
            _bodyContainerView.Remove(_bodyView);
            _bodyView.Dispose();
        }

        if (content == null)
        {
            _bodyView = new Label { Text = "(No body)" };
            _bodyContainerView?.Add(_bodyView);
            return;
        }

        var contentType = content.Headers.ContentType?.MediaType ?? "text/plain";

        try
        {
            if (contentType.Contains("application/x-www-form-urlencoded", StringComparison.InvariantCultureIgnoreCase))
            {
                var body = await content.ReadAsStringAsync();
                DisplayFormData(body);
            }
            else if (contentType.Contains("multipart/form-data", StringComparison.InvariantCultureIgnoreCase))
            {
                // For multipart, show a simple text view with parsed parts info
                var body = await content.ReadAsStringAsync();
                _bodyView = new TextView
                {
                    Text = body,
                    ReadOnly = true,
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };
                _bodyContainerView?.Add(_bodyView);
            }
            else if (contentType.Contains("application/json", StringComparison.InvariantCultureIgnoreCase) ||
                     contentType.Contains("text/", StringComparison.InvariantCultureIgnoreCase))
            {
                var body = await content.ReadAsStringAsync();
                _bodyView = new TextView
                {
                    Text = body,
                    ReadOnly = true,
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };
                _bodyContainerView?.Add(_bodyView);
            }
            else
            {
                _bodyView = new Label { Text = $"(Binary content: {contentType})" };
                _bodyContainerView?.Add(_bodyView);
            }
        }
        catch (Exception ex)
        {
            _bodyView = new Label { Text = $"Error reading body: {ex.Message}" };
            _bodyContainerView?.Add(_bodyView);
        }
    }

    private void DisplayFormData(string body)
    {
        var formTable = new DataTable();
        formTable.Columns.Add("Key", typeof(string));
        formTable.Columns.Add("Value", typeof(string));

        var pairs = body.Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
            formTable.Rows.Add(key, value);
        }

        var tableView = new TableView
        {
            Table = new DataTableSource(formTable),
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _bodyView = tableView;
    }
}