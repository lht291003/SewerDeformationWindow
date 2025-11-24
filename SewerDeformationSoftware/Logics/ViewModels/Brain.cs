namespace SewerDeformationSoftware.Logics.ViewModels;

public class Brain : Basis
{
    public ICommand WorkFormShowCommand { get; set; } = null!;

    public ICommand ClosedWindowCommand { get; set; } = null!;

    public ICommand LoadedWindowCommand { get; set; } = null!;

    public Brain()
    {
        WorkFormShowCommand = new RelayCommand<List<Object>>(Object => Object != null, Package => ShowWorkForm(Package));

        LoadedWindowCommand = new RelayCommand<Object>(OBj => OBj == null, OBj => PrepareAfterLoaded(DirPath.Warehouse));

        ClosedWindowCommand = new RelayCommand<Object>(OBj => OBj == null, OBj => CleanupAfterClosed(DirPath.Warehouse));
    }

    Task LoadControlWithName(Grid MainForm, UserControl UC)
    {
        UserControl? Existing = MainForm.Children.OfType<UserControl>().FirstOrDefault(E => E.GetType() == UC.GetType());

        if (Existing != null)
        {
            Int32 GetExistingZIndex = Panel.GetZIndex(Existing);

            Panel.SetZIndex(Existing, MainForm.Children.Count);

            foreach (UIElement UserScreen in MainForm.Children)
            {
                if (UserScreen.GetType() != Existing.GetType())
                {
                    Int32 CurentZIndex = Panel.GetZIndex(UserScreen);

                    if (CurentZIndex >= GetExistingZIndex)
                    {
                        Panel.SetZIndex(UserScreen, CurentZIndex - 1);
                    }
                }
            }
        }
        else
        {
            MainForm.Children.Add(UC);

            Panel.SetZIndex(UC, MainForm.Children.Count);
        }

        return Task.CompletedTask;
    }

    Task ShowWorkForm(List<Object> Pack)
    {
        Grid MainScreen = (Grid)Pack[0];

        ListView LV = (ListView)Pack[1];

        String GetTabName = ((ListViewItem)LV.SelectedItem).Name;

        switch (GetTabName)
        {
            case "BuildUC":

                LoadControlWithName(MainScreen, new BuildUC());

                break;

            case "PhotoUC":

                LoadControlWithName(MainScreen, new PhotoUC());

                break;

            case "VideoUC":

                LoadControlWithName(MainScreen, new VideoUC());

                break;
        }

        return Task.CompletedTask;
    }

    Task CleanupAfterClosed(String GetRoot)
    {
        ClearFolderSpace(GetRoot);

        return Task.CompletedTask;
    }

    Task PrepareAfterLoaded(String GetRoot)
    {
        if (Directory.Exists(GetRoot))
        {
            ClearFolderSpace(GetRoot);
        }

        Directory.CreateDirectory(GetRoot);

        return Task.CompletedTask;
    }

    Task ClearFolderSpace(String P)
    {
        foreach (String Entry in Directory.EnumerateFileSystemEntries(P))
        {
            if (File.Exists(Entry))
            {
                if (!Interop.IsFileLocked(Entry)) { File.Delete(Entry); }
            }

            else

            if (Directory.Exists(Entry))
            {
                ClearFolderSpace(Entry);

                if (!Directory.EnumerateFileSystemEntries(Entry).Any()) { Directory.Delete(Entry); }
            }
        }

        return Task.CompletedTask;
    }
}