using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Data;
using WatchDog.Core.Context;
using WatchDog.Service.Contexts;
using WatchDog.Uno.WatchDogClient;

namespace WatchDog.Uno.ViewModels;

public sealed class WatchDogViewModel : INotifyPropertyChanged
{
    internal WatchDogProvider WatchDogProvider
        => _watchDogProvider ??= new WatchDogProvider(ServerHost);

    private WatchDogProvider? _watchDogProvider;

    private string _serverHost = "http://127.0.0.1:57725/";

    public string ServerHost
    {
        get => _serverHost;
        set
        {
            if (value == _serverHost)
            {
                return;
            }

            _serverHost = value;

            if (!_serverHost.EndsWith("/"))
            {
                _serverHost = $"{_serverHost}/";
            }

            _watchDogProvider = null;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 服务状态
    /// </summary>
    public string ServerStatus
    {
        get => _serverStatus;
        set
        {
            if (value == _serverStatus)
            {
                return;
            }

            _serverStatus = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<WangModel> WangInfoCollection { get; } = [];

    public async Task WatchAsync()
    {
        while (true)
        {
            try
            {
                GetWangResponse? getWangResponse = await WatchDogProvider.GetWangAsync(new GetWangInfo(_dogId));
                if (getWangResponse == null)
                {
                    ServerStatus = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss,fff} GetWangAsync 返回空";
                    continue;
                }

                var getWangResult = getWangResponse.GetWangResult;
                Debug.Assert(getWangResult.DogId == _dogId);

                foreach (WangInfo wangInfo in getWangResult.WangList)
                {
                    if (wangInfo.CheckShouldWangResult.ShouldWang)
                    {
                        NotifyWang?.Invoke(this, wangInfo);
                    }
                }

                // 减少刷新列表，防止界面闪烁
                var shouldUpdateList = WangInfoCollection.Count != getWangResult.WangList.Count;
                if (!shouldUpdateList)
                {
                    for (int i = 0; i < getWangResult.WangList.Count; i++)
                    {
                        if (!WangInfoCollection[i].Equals(getWangResult.WangList[i]))
                        {
                            shouldUpdateList = true;
                            break;
                        }
                    }
                }

                if (shouldUpdateList)
                {
                    WangInfoCollection.Clear();
                    foreach (WangInfo wangInfo in getWangResult.WangList)
                    {
                        WangInfoCollection.Add(new WangModel(wangInfo));
                    }
                }

                ServerStatus = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss,fff} 连接成功";
            }
            catch (Exception exception)
            {
                ServerStatus = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss,fff} {nameof(WatchAsync)} 异常 {exception}";
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }

    public event EventHandler<WangInfo>? NotifyWang;

    private readonly string _dogId = Guid.NewGuid().ToString();
    private string _serverStatus = "未连接";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public class WangModel
{
    // 这个类型只是用来解决 `XamlTypeInfo.g.cs(617,13,617,29): error CS8852: 只能在对象初始值设定项中或在实例构造函数或 "init" 访问器中的 "this" 或 "base" 上分配 init-only 属性或索引器 "WangInfo.FeedDogInfo"` 错误
    // 从 ViewModel 层面上来说，不应该有这个类型
    public WangModel(WangInfo wangInfo)
    {
        WangStatus = CheckShouldWangResultToTextConverter.ToText(wangInfo.CheckShouldWangResult);

        Id = wangInfo.FeedDogInfo.Id;
        LastUpdateTime = wangInfo.FeedDogInfo.LastUpdateTime.ToString($"yyyy-MM-dd HH:mm:ss,fff");

        var feedDogInfo = wangInfo.FeedDogInfo.FeedDogInfo;
        Name = feedDogInfo.Name;
        Status = feedDogInfo.Status;

        _wangInfo = wangInfo;
    }

    public string WangStatus { get; set; }

    public string Name { get; set; }

    public string Id { get; set; }
    public string Status { get; set; }
    public string LastUpdateTime { get; set; }

    private readonly WangInfo _wangInfo;

    public bool Equals(WangInfo wangInfo) => _wangInfo.Equals(wangInfo);
}

static class CheckShouldWangResultToTextConverter
{
    public static string ToText(CheckShouldWangResult result)
    {
        if (result.ShouldWang)
        {
            return "汪";
        }

        if (result.ShouldMute)
        {
            return "汪(静默)";
        }

        if (result.OverNotifyMaxCount)
        {
            return "汪(超过最大通知次数)";
        }

        if (result.InNotifyInterval)
        {
            return "汪";
        }

        return "OK";
    }
}

public class CheckShouldWangResultToTextValueConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is CheckShouldWangResult result)
        {
            return CheckShouldWangResultToTextConverter.ToText(result);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value;
    }
}
