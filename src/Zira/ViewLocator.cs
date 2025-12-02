using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using ServiceScan.SourceGenerator;
using Volo.Abp.DependencyInjection;
using Zira.ViewModels;
using Zira.Views;

namespace Zira;

public sealed partial class ViewLocator : IDataTemplate, ISingletonDependency
{
    private static readonly Dictionary<Type, Type> ViewTypeCache = new();

    private readonly IServiceProvider _serviceProvider;

    public ViewLocator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        AddViews();
    }

    public TView CreateView<TView>(ViewModel viewModel)
        where TView : Control => (TView)CreateView(viewModel);

    public Control CreateView(ViewModel viewModel)
    {
        var viewModelType = viewModel.GetType();

        if (!ViewTypeCache.TryGetValue(viewModelType, out var viewType))
        {
            return CreateText($"Could not find view for {viewModelType.FullName}");
        }

        var view = (Control)ActivatorUtilities.CreateInstance(_serviceProvider, viewType);
        view.DataContext = viewModel;
        return view;
    }

    Control ITemplate<object?, Control?>.Build(object? data)
    {
        if (data is ViewModel viewModel)
        {
            return CreateView(viewModel);
        }

        return CreateText($"Could not find view for {data?.GetType().FullName}");
    }

    bool IDataTemplate.Match(object? data) => data is ViewModel;

    private static TextBlock CreateText(string text) => new() { Text = text };

    private static void TryAdd(Type viewModelType, Type viewType) =>
        ViewTypeCache.TryAdd(viewModelType, viewType);

    [GenerateServiceRegistrations(
        AssignableTo = typeof(SukiWindow<>),
        CustomHandler = nameof(AddViewsHandler)
    )]
    [GenerateServiceRegistrations(
        AssignableTo = typeof(UserControl<>),
        CustomHandler = nameof(AddViewsHandler)
    )]
    private static partial void AddViews();

    private static void AddViewsHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
        TViewModel
    >()
        where TView : Control
        where TViewModel : ViewModel => TryAdd(typeof(TViewModel), typeof(TView));
}
