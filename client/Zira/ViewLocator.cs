using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Volo.Abp.DependencyInjection;
using Zira.ViewModels;
using Zira.Views;

namespace Zira;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator"
)]
public class ViewLocator : IDataTemplate, ISingletonDependency
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        if (param is MainWindowViewModel mainWindowViewModel)
        {
            return new MainWindow { DataContext = mainWindowViewModel };
        }

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModel;
    }
}
