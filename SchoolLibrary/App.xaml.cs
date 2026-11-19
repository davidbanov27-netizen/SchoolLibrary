using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using SchoolLibrary.Services;
using SchoolLibrary.ViewModels;

namespace SchoolLibrary;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Без това WPF форматира датите и числата в binding-ите по en-US, а не по езика на системата.
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        base.OnStartup(e);

        var service = new LibraryService(new DataStore());
        var window = new MainWindow
        {
            DataContext = new MainViewModel(service, new LocalAiSearchService())
        };
        window.Show();
    }
}
