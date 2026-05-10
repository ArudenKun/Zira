using System.Threading.Tasks;
using Avalonia.Metadata;
using R3.ObservableEvents;

[assembly: GenerateStaticEventObservables(typeof(TaskScheduler))]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.Converters")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.Navigation")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.Utilities")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.ViewModels")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.ViewModels.Components")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.ViewModels.Dialogs")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.ViewModels.Pages")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.Views")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.Views.Components")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.Views.Dialogs")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.Views.Pages")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/zira", "Zira.Controls")]
[assembly: XmlnsPrefix("https://github.com/arudenkun/zira", "zira")]
