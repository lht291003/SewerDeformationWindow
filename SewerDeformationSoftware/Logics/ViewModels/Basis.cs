namespace SewerDeformationSoftware.Logics.ViewModels;

public class ARelayCommand<GenericType>(Predicate<GenericType> Trigger, Action<GenericType> ActionExecution) : ICommand
{
    public Boolean CanExecute(Object? Parameter)

                                => Trigger == null || Trigger((GenericType)Parameter!);

    public void Execute(Object? Parameter) => ActionExecution((GenericType)Parameter!);

    public event EventHandler? CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
}

public class FRelayCommand<GenericType>(Predicate<GenericType> Trigger, Func<GenericType, Task> FunExecution) : ICommand
{
    Boolean IsExecuting;

    public Boolean CanExecute(Object? Parameter)

                                                => !IsExecuting && (Trigger == null || Trigger((GenericType)Parameter!));

    public async void Execute(Object? Parameter)
    {
        if (CanExecute(Parameter))
        {
            IsExecuting = (0 == 0);

            CommandManager.InvalidateRequerySuggested();

            await FunExecution((GenericType)Parameter!);

            IsExecuting = (0 != 0);

            CommandManager.InvalidateRequerySuggested();
        }
    }

    public event EventHandler? CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
}

public class Basis : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyToUIAndSetIfChanged<GenericType>(GenericType InputValue, ref GenericType FieldValue, [CallerMemberName] String? OwnerName = null)
    {
        if (!EqualityComparer<GenericType>.Default.Equals(FieldValue, InputValue))
        {
            FieldValue = InputValue;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(OwnerName));
        }
    }
}