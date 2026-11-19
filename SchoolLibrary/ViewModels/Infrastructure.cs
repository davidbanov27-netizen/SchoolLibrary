using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SchoolLibrary.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(name);
        return true;
    }
}

public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();
}

public enum MessageKind { None, Info, Success, Warning, Error }

public interface INavigator
{
    void Navigate(string key);
}

/// <summary>Основа за екраните, които показват съобщение (успех, грешка, предупреждение).</summary>
public abstract class PageViewModel : ViewModelBase
{
    private string _message = "";
    private MessageKind _kind = MessageKind.None;

    public string Message
    {
        get => _message;
        private set => Set(ref _message, value);
    }

    public MessageKind Kind
    {
        get => _kind;
        private set => Set(ref _kind, value);
    }

    protected void Show(MessageKind kind, string text)
    {
        Message = text;
        Kind = kind;
    }

    protected void ClearMessage()
    {
        Message = "";
        Kind = MessageKind.None;
    }
}
