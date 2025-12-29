using System.Collections.ObjectModel;
using System.Diagnostics;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Nightmare.UI;

public class ProfilesDialog : Dialog
{
    private readonly ObservableCollection<string> _profilesNamesList;

    public string[] ProfilesNames
    {
        get;
        set
        {
            field = value;
            _profilesNamesList.Clear();

            foreach (var profile in value)
                _profilesNamesList.Add(profile);
        }
    }

    public ProfilesDialog()
    {
        Title = "Select profile";
        Width = Dim.Percent(50);
        Height = Dim.Percent(25);

        _profilesNamesList = [];
        var profilesListView = new ListView
        {
            Height = Dim.Fill(),
            Width = Dim.Fill()
        };
        profilesListView.SetSource(_profilesNamesList);

        Add(profilesListView);

        profilesListView.OpenSelectedItem += (_, args) =>
        {
            var selectedItem = args.Item;

            if (selectedItem is not null && selectedItem < _profilesNamesList.Count)
                SelectedProfileChanged?.Invoke(
                    this,
                    ProfilesNames![(int)selectedItem]
                );
        };
    }

    public event EventHandler<string> SelectedProfileChanged;
}