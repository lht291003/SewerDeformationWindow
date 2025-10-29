namespace SewerDeformationSoftware.Logics.ViewModels;

public class Basis : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void NotifyToTarget([CallerMemberName] String? PropertyName = null)

                                      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

    protected virtual Boolean SetAndNotify<T>(T Value, ref T Field, [CallerMemberName] String? PropertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(Field, Value))
        {
            return false;
        }
        else
        {
            Field = Value;

            NotifyToTarget(PropertyName);

            return true;
        }
    }
}

public class RelayCommand<T>(Predicate<T> Activation, Action<T> Function) : ICommand
{
    Predicate<T> Condition = Activation;

    Action<T> Execution = Function;

    public bool CanExecute(Object? Parameter)

                      => Condition == null || Condition((T)Parameter!);

    public void Execute(Object? Parameter) => Execution((T)Parameter!);

    public event EventHandler? CanExecuteChanged { add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value; }
}