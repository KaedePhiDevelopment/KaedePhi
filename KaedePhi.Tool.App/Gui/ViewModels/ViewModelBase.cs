using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KaedePhi.Tool.App.Gui.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected void OnPropertiesChanged(params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
            OnPropertyChanged(propertyName);
    }

    // 值未变化时不触发通知，返回值指示是否发生了变更
    protected bool SetProperty<T>(
        ref T storage,
        T value,
        [CallerMemberName] string? propertyName = null
    )
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // 属性值存放在外部模型对象上时使用，由 assign 负责写回模型
    protected bool SetProperty<T>(
        T current,
        T value,
        Action<T> assign,
        [CallerMemberName] string? propertyName = null
    )
    {
        if (EqualityComparer<T>.Default.Equals(current, value))
            return false;

        assign(value);
        OnPropertyChanged(propertyName);
        return true;
    }
}
