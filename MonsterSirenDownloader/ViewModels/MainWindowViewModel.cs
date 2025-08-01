using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;

namespace MonsterSirenDownloader.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region 状态字段

    private string _statusText = "准备就绪";
    private string _progressText = "";
    private double _progressValue;
    private bool _isIndeterminate;
    private string _consoleText = "";
    private SolidColorBrush _statusColor = new(Colors.Gray);
    private string _detailedStatus = "点击开始下载按钮下载曲库";
    private bool _canCancel;
    private bool _canStart = true;
    private string _startButtonText = "开始下载";

    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning;
    private bool _done;

    #endregion

    #region View绑定

    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            OnPropertyChanged();
        }
    }

    public string ProgressText
    {
        get => _progressText;
        set
        {
            _progressText = value;
            OnPropertyChanged();
        }
    }

    public double ProgressValue
    {
        get => _progressValue;
        set
        {
            _progressValue = value;
            OnPropertyChanged();
        }
    }

    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        set
        {
            _isIndeterminate = value;
            OnPropertyChanged();
        }
    }

    public string ConsoleText
    {
        get => _consoleText;
        set
        {
            _consoleText = value;
            OnPropertyChanged();
        }
    }

    public SolidColorBrush StatusColor
    {
        get => _statusColor;
        set
        {
            _statusColor = value;
            OnPropertyChanged();
        }
    }

    public string DetailedStatus
    {
        get => _detailedStatus;
        set
        {
            _detailedStatus = value;
            OnPropertyChanged();
        }
    }

    public bool CanCancel
    {
        get => _canCancel;
        set
        {
            _canCancel = value;
            OnPropertyChanged();
        }
    }

    public bool CanStart
    {
        get => _canStart;
        set
        {
            _canStart = value;
            OnPropertyChanged();
        }
    }

    public string StartButtonText
    {
        get => _startButtonText;
        set
        {
            _startButtonText = value;
            OnPropertyChanged();
        }
    }

    public ICommand StartCommand { get; }
    public ICommand CancelCommand { get; }

    #endregion

    #region 初始化

    public MainWindowViewModel()
    {
        StartCommand = new RelayCommand(async void () =>
        {
            try
            {
                await Start();
            }
            catch (Exception e)
            {
                Cancel();
                AppendToConsole($"因异常中断的下载：{e.Message}");
            }
        }, () => CanStart);

        CancelCommand = new RelayCommand(Cancel, () => CanCancel);
    }

    #endregion

    #region 任务

    private async Task Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();


        // 模拟过程
        // 更新UI状态
        CanStart = false;
        CanCancel = true;
        StartButtonText = "安装中...";
        StatusText = "正在安装";
        DetailedStatus = "安装过程正在进行中...";
        StatusColor = new SolidColorBrush(Colors.Orange);
        IsIndeterminate = false;

        AppendToConsole("=== 开始安装过程 ===");

        try
        {
            // 模拟安装过程的各个阶段
            var tasks = new[]
            {
                ("检查系统要求", 10),
                ("下载必要文件", 25),
                ("解压安装包", 15),
                ("配置环境变量", 10),
                ("注册系统服务", 15),
                ("创建快捷方式", 10),
                ("完成安装配置", 15)
            };

            double currentProgress = 0;

            foreach (var (taskName, duration) in tasks)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    AppendToConsole("安装已被用户取消");
                    return;
                }

                StatusText = taskName;
                AppendToConsole($"正在执行: {taskName}...");

                // 模拟任务执行时间
                for (var i = 0; i <= duration; i++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        return;

                    await Task.Delay(200, _cancellationTokenSource.Token);

                    var taskProgress = (double)i / duration * (100.0 / tasks.Length);
                    ProgressValue = currentProgress + taskProgress;
                    ProgressText = $"{ProgressValue:F0}%";

                    // 添加一些详细的控制台输出
                    if (i % 5 == 0 && i > 0)
                    {
                        AppendToConsole($"  - {taskName} 进度: {(double)i / duration * 100:F0}%");
                    }
                }

                currentProgress += 100.0 / tasks.Length;
                AppendToConsole($"✓ {taskName} 完成");
            }

            // 安装完成
            _done = true;
            ProgressValue = 100;
            ProgressText = "100%";
            StatusText = "安装完成";
            DetailedStatus = "程序已成功安装";
            StatusColor = new SolidColorBrush(Colors.Green);
            CanStart = true;
            AppendToConsole("=== 安装成功完成 ===");
            AppendToConsole("程序已成功安装到您的系统中。");
        }
        catch (OperationCanceledException)
        {
            StatusText = "安装已取消";
            DetailedStatus = "用户取消了安装过程";
            StatusColor = new SolidColorBrush(Colors.Red);
            AppendToConsole("安装过程已被取消");
        }
        catch (Exception ex)
        {
            StatusText = "安装失败";
            DetailedStatus = $"安装过程中发生错误: {ex.Message}";
            StatusColor = new SolidColorBrush(Colors.Red);
            AppendToConsole($"错误: {ex.Message}");
        }
        finally
        {
            CanStart = true;
            CanCancel = false;
            _isRunning = false;
            StartButtonText = _done ? "更新" : "重试安装";
        }
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        _isRunning = false;
        AppendToConsole("下载任务已中断");
    }

    #endregion

    #region 辅助方法

    private void AppendToConsole(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var logEntry = $"[{timestamp}] {message}\n";

        Dispatcher.UIThread.InvokeAsync(() => { ConsoleText += logEntry; });
    }

    #endregion
}