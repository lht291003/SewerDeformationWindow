namespace SewerDeformationSoftware.Logics.ViewModels;

public class Basis : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] String? PropertyName = null)

               => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
}

public class RelayCommand<T>(Predicate<T> Activation, Action<T> Function) : ICommand
{
    Predicate<T> Condition = Activation;

    Action<T> Execution = Function;

    public bool CanExecute(Object? Parameter)

                      => Condition == null || Condition((T)Parameter!);

    public void Execute(Object? Parameter) => Execution((T)Parameter!);

    public event EventHandler? CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
}