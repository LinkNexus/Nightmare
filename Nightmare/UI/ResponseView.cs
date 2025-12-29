using System.Data;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using System.Net.Http;

namespace Nightmare.UI;

public class ResponseView : FrameView
{
    private readonly Label _statusLabel;

    private readonly DataTable _headersTable = new();
    private readonly DataTable _cookiesTable = new();
    private readonly View _bodyContainerView;
    private View _bodyView = new Label { Text = "(No body)" };

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
        _bodyContainerView.Add(_bodyView);

        Add(_statusLabel, tabView);
    }

    public async Task OnResponseReceived(HttpResponseMessage response)
    {
        _statusLabel.Text = $"{(int)response.StatusCode} {response.ReasonPhrase}";

        _headersTable.Clear();
        _cookiesTable.Clear();

        foreach (var (key, value) in response.Headers)
            _headersTable.Rows.Add(key, string.Join(", ", value));

        foreach (var (key, value) in response.Content.Headers)
            _headersTable.Rows.Add(key, string.Join(", ", value));

        // Parse Set-Cookie headers
        if (response.Headers.TryGetValues("set-cookie", out var setCookies))
            foreach (var cookie in setCookies)
            {
                var parts = cookie.Split(';')[0].Split('=', 2);
                var key = parts[0].Trim();
                var value = parts.Length > 1 ? parts[1].Trim() : "";
                _cookiesTable.Rows.Add(key, value);
            }

        // Display body
        await DisplayBody(response.Content);
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
                // For multipart, show a text view
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