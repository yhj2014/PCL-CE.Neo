using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using PCL_CE.Neo.UI.Pages;
using System.Collections.Generic;

namespace PCL_CE.Neo.UI.Navigation;

public interface INavigationService
{
    void Initialize(Frame mainFrame);
    bool NavigateTo(string route);
    bool NavigateTo<T>() where T : Page;
    void GoBack();
    void GoForward();
    bool CanGoBack { get; }
    bool CanGoForward { get; }
    string CurrentRoute { get; }
}

public class NavigationService : INavigationService
{
    private readonly ILogger<NavigationService> _logger;
    private Frame? _mainFrame;
    private readonly Dictionary<string, System.Type> _routeMap = new();

    public NavigationService(ILogger<NavigationService> logger)
    {
        _logger = logger;
        InitializeRouteMap();
    }

    private void InitializeRouteMap()
    {
        _routeMap["Home"] = typeof(HomePage);
        _routeMap["Download"] = typeof(DownloadPage);
        _routeMap["Launch"] = typeof(LaunchPage);
        _routeMap["Instance"] = typeof(InstancePage);
        _routeMap["Login"] = typeof(LoginPage);
        _routeMap["Tools"] = typeof(ToolsPage);
        _routeMap["Settings"] = typeof(SettingsPage);
    }

    public void Initialize(Frame mainFrame)
    {
        _mainFrame = mainFrame;
        _logger.LogDebug("导航服务已初始化");
    }

    public bool NavigateTo(string route)
    {
        try
        {
            if (_mainFrame == null)
            {
                _logger.LogWarning("导航框架未初始化");
                return false;
            }

            if (_routeMap.TryGetValue(route, out var pageType))
            {
                _logger.LogInformation("导航到页面: {Route}", route);
                return _mainFrame.Navigate(pageType);
            }

            _logger.LogWarning("未找到路由: {Route}", route);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导航到 {Route} 失败", route);
            return false;
        }
    }

    public bool NavigateTo<T>() where T : Page
    {
        try
        {
            if (_mainFrame == null)
            {
                _logger.LogWarning("导航框架未初始化");
                return false;
            }

            var pageType = typeof(T);
            _logger.LogInformation("导航到页面类型: {Type}", pageType.Name);
            return _mainFrame.Navigate(pageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导航到 {Type} 失败", typeof(T).Name);
            return false;
        }
    }

    public void GoBack()
    {
        try
        {
            if (_mainFrame != null && _mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
                _logger.LogDebug("导航返回");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导航返回失败");
        }
    }

    public void GoForward()
    {
        try
        {
            if (_mainFrame != null && _mainFrame.CanGoForward)
            {
                _mainFrame.GoForward();
                _logger.LogDebug("导航前进");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导航前进失败");
        }
    }

    public bool CanGoBack => _mainFrame?.CanGoBack ?? false;

    public bool CanGoForward => _mainFrame?.CanGoForward ?? false;

    public string CurrentRoute
    {
        get
        {
            if (_mainFrame?.Content is Page currentPage)
            {
                foreach (var (route, pageType) in _routeMap)
                {
                    if (pageType == currentPage.GetType())
                    {
                        return route;
                    }
                }
            }
            return string.Empty;
        }
    }
}