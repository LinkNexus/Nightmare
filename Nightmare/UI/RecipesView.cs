using Nightmare.Parser;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Nightmare.UI;

public class RecipesView : FrameView
{
    private readonly TextField _searchField;
    private readonly TreeView<JsonProperty> _requestsTreeView;

    public List<JsonProperty> Requests
    {
        get;
        set
        {
            field = value;
            _requestsTreeView.ClearObjects();
            _requestsTreeView.AddObjects(value);
        }
    }

    public RecipesView()
    {
        Title = "Recipes";
        Height = Dim.Fill();
        Width = Dim.Percent(30);
        Arrangement = ViewArrangement.Resizable;

        var searchLabel = new Label { Text = "Search: " };
        _searchField = new TextField
        {
            X = Pos.Right(searchLabel) + 1,
            Y = Pos.Top(searchLabel),
            Width = Dim.Fill()
        };

        _requestsTreeView = new TreeView<JsonProperty>
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Y = Pos.Bottom(searchLabel) + 1
        };
        _requestsTreeView.AspectGetter = prop => prop.Name;
        _requestsTreeView.TreeBuilder = new DelegateTreeBuilder<JsonProperty>(
            arg =>
            {
                if (arg.Value is JsonObject obj && obj.TryGetProperty<JsonObject>("requests", out var requestsProp))
                    return requestsProp
                        .Properties
                        .Select(p => new JsonProperty(p.Key, p.Value, p.Value.Span))
                        .ToList();

                return [];
            },
            arg => arg.Value is JsonObject obj
                   && obj.TryGetProperty<JsonObject>("requests", out _)
        );
        _requestsTreeView.ObjectActivated += (_, args) =>
        {
            var selected = args.ActivatedObject;

            if (
                selected?.Value is JsonObject obj
                && !obj.TryGetProperty("requests", out var _)
            )
                RequestSelected?.Invoke(this, selected);
        };

        var filter = new TreeViewTextFilter<JsonProperty>(_requestsTreeView);
        _requestsTreeView.Filter = filter;

        _searchField.TextChanged += (_, _) =>
        {
            filter.Text = _searchField.Text;
            if (_requestsTreeView.SelectedObject is not null)
                _requestsTreeView.EnsureVisible(_requestsTreeView.SelectedObject);
        };

        Add(searchLabel, _searchField, _requestsTreeView);
    }

    public event EventHandler<JsonProperty> RequestSelected;
}