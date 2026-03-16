using System.IO;
using System.Reflection;

namespace IgnaviorLauncher.Services;

public static class ResourceService
{
    private static readonly string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static readonly string BaseDirectory = Path.Combine(local, "IgnaviorLauncher", "bin");
    private const string REL_PATH = "IgnaviorLauncher.Resources.xdelta3.exe";

    // TODO: Move to separate service
    public static readonly string LocalAppDirectory = Path.Combine(local, "IgnaviorLauncher");

    /// <summary>
    /// Finds the xdelta3 patcher executable from embedded resources.
    /// </summary>
    public static string GetPatcher()
    {
        string exe = Path.Combine(BaseDirectory, "xdelta3.exe");
        if (File.Exists(exe))
        {
            return exe;
        }

        Directory.CreateDirectory(BaseDirectory);
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(REL_PATH))
        {
            if (stream == null)
            {
                throw new Exception("Patcher executable (xdelta3) not found in embedded resources.");
            }

            using FileStream fileStream = new(
                path: exe,
                mode: FileMode.Create,
                access: FileAccess.Write);
            stream.CopyTo(fileStream);
        }
        return exe;
    }
}