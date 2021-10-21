﻿#if WINDOWS_UWP || UNO_UWP_COMPATIBILITY
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace Uno.Extensions.Navigation;

public static class FrameworkElementExtensions
{
    public static async Task EnsureLoaded(this FrameworkElement element)
    {
        if (element == null)
        {
            return;
        }

        var completion = new TaskCompletionSource<object>();

        // Note: We're attaching to three different events to
        // a) always detect when element is loaded (sometimes Loaded is never fired)
        // b) detect as soon as IsLoaded is true (Loading and Loaded not always in right order)

        RoutedEventHandler loaded = null;
        EventHandler<object> layoutChanged = null;
        TypedEventHandler<FrameworkElement, object> loading = null;

        Action loadedAction = () =>
        {
            if (element.IsLoaded)
            {
                completion.SetResult(null);
                element.Loaded -= loaded;
                element.Loading -= loading;
                element.LayoutUpdated -= layoutChanged;
            }
        };

        loaded = (s, e) => loadedAction();
        loading = (s, e) => loadedAction();
        layoutChanged = (s, e) => loadedAction();

        element.Loaded += loaded;
        element.Loading += loading;
        element.LayoutUpdated += layoutChanged;

        if (element.IsLoaded)
        {
            loadedAction();
        }

        await completion.Task;
    }

    public static void InjectServicesAndSetDataContext(this FrameworkElement view, IServiceProvider services, INavigator navigation, object viewModel)
    {
        if (view is not null)
        {
            if (viewModel is not null &&
                view.DataContext != viewModel)
            {
                view.DataContext = viewModel;
            }
        }

        if (view is IInjectable<INavigator> navAware)
        {
            navAware.Inject(navigation);
        }

        if (view is IInjectable<IServiceProvider> spAware)
        {
            spAware.Inject(services);
        }

        if (view is IInjectable<IRouteMappings> mappings)
        {
            mappings.Inject(services.GetService<IRouteMappings>());
        }
    }
}
