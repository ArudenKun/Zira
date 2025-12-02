using Avalonia.Metadata;
using R3.ObservableEvents;

[assembly: GenerateStaticEventObservables(typeof(TaskScheduler))]

[assembly: XmlnsDefinition("https://github.com/arudenkun/Zira", "Zira.ViewModels")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/Zira", "Zira.ViewModels.Pages")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/Zira", "Zira.ViewModels.Dialogs")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/Zira", "Zira.Views")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/Zira", "Zira.Views.Pages")]
[assembly: XmlnsDefinition("https://github.com/arudenkun/Zira", "Zira.Views.Dialogs")]
[assembly: XmlnsPrefix("https://github.com/arudenkun/Zira", "zira")]
