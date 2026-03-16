using System.Configuration;
using System.Data;
using System.Windows;

namespace IgnaviorLauncher;
using Services;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            string path = ResourceService.GetPatcher();
            AppDomain.CurrentDomain.SetData("PatcherPath", path);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to extract xdelta3 patcher: {ex.Message}",
                caption: "Error",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Error
                );
            Shutdown();
        }
    }
}
