using Avalonia.Media.Imaging;
using KaedePhi.Tool.App.Gui.Models;
using KaedePhi.Tool.App.Gui.Services;
using Serilog;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.App.Gui.ViewModels;

public sealed class BackgroundViewModel : ViewModelBase
{
    private static readonly TimeSpan AutoRefreshInterval = TimeSpan.FromMinutes(30);

    private readonly ILogger _log = Log.ForContext<BackgroundViewModel>();
    private readonly BackgroundSceneService _scene;
    private CancellationTokenSource? _cancellation = new();
    private Task? _autoRefresh;
    private BackgroundImageResource? _current;
    private bool _stopped;
    private bool _isSaving;

    public BackgroundViewModel()
        : this(new BackgroundSceneService()) { }

    internal BackgroundViewModel(BackgroundSceneService scene)
    {
        _scene = scene;
    }

    public Bitmap? Image
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(HasImage));
        }
    }

    public bool HasImage => Image != null;

    public Bitmap? PendingImage
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(HasPendingImage));
        }
    }

    public bool HasPendingImage => PendingImage != null;

    public bool IsRefreshing
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public string Message
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(HasMessage));
        }
    } = string.Empty;

    public bool HasMessage => !string.IsNullOrEmpty(Message);

    // 新图片就绪后、正式替换前触发，供视图播放揭示动画
    internal event Func<CancellationToken, Task>? TransitionRequested;

    public void StartAutoRefresh()
    {
        if (_stopped || _autoRefresh != null)
            return;

        _autoRefresh = RefreshAutomaticallyAsync(_cancellation!.Token);
    }

    public void Stop()
    {
        if (_stopped)
            return;

        _stopped = true;
        _cancellation?.Cancel();
        _cancellation?.Dispose();
        _cancellation = null;
        PendingImage = null;
        Image = null;
        _current?.Dispose();
        _current = null;
    }

    public async Task<bool> RefreshOnceAsync()
    {
        if (_stopped || IsRefreshing)
            return false;

        var cancellationToken = _cancellation!.Token;
        IsRefreshing = true;
        Message = ui_background_refreshing;
        BackgroundImageResource? incoming = null;
        try
        {
            incoming = await _scene.FetchResourceAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (incoming == null)
            {
                Message = ui_background_refresh_failed;
                return false;
            }

            PendingImage = incoming.Image;
            if (TransitionRequested is { } transition)
                await transition(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            var previous = _current;
            _current = incoming;
            incoming = null;
            Image = _current.Image;
            PendingImage = null;
            previous?.Dispose();
            Message = string.Empty;
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception ex)
        {
            _log.Error(ex, ui_background_refresh_failed);
            Message = ui_background_refresh_failed;
            return false;
        }
        finally
        {
            PendingImage = null;
            incoming?.Dispose();
            IsRefreshing = false;
        }
    }

    public async Task<string?> SaveAsync(string? directory = null)
    {
        if (_stopped || _isSaving)
            return null;
        if (_current == null)
        {
            Message = ui_background_unavailable;
            return null;
        }

        var cancellationToken = _cancellation!.Token;
        // 保存原图数据的快照，允许刷新完成后释放旧位图而不影响正在写入的文件。
        var data = _current.ImageData;
        var extension = _current.Extension;
        _isSaving = true;
        try
        {
            var path = await BackgroundSceneService.SaveAsync(
                data,
                extension,
                cancellationToken,
                directory
            );
            Message = string.Format(ui_background_saved, path);
            return path;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (Exception ex)
        {
            _log.Error(ex, ui_background_save_failed);
            Message = ui_background_save_failed;
            return null;
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task RefreshAutomaticallyAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await RefreshOnceAsync();

            try
            {
                await Task.Delay(AutoRefreshInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
