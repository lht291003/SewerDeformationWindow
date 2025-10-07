namespace SewerDeformationSoftware;

public partial class App : Application
{
    private void UnSelectedRowsWhenClickEmptyArea(Object Obj, MouseButtonEventArgs E)
    {
        if (Obj is DataGrid DG && VisualTreeHelper.HitTest(DG, Mouse.GetPosition(DG)).VisualHit is not DataGridRow)
        {
            DG.UnselectAll();
        }
    }
}