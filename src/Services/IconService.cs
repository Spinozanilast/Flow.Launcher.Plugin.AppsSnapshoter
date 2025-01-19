using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Flow.Launcher.Plugin.AppsSnapshoter.Models;

namespace Flow.Launcher.Plugin.AppsSnapshoter.Services;

public class IconService
{
    private const string DefaultIconsDirectoryName = "Icons";
    private const string DefaultIconsImagesExtension = ".png";


    private readonly string _iconsDirectory;

    public IconService(string pluginDirectory)
    {
        _iconsDirectory = Path.Combine(pluginDirectory, DefaultIconsDirectoryName);
        if (!Directory.Exists(_iconsDirectory))
        {
            Directory.CreateDirectory(_iconsDirectory);
        }
    }

    public void SetIconForApp(AppModel model, string executablePath = null)
    {
        var iconFilePath = Path.Combine(_iconsDirectory, model.AppModuleName + DefaultIconsImagesExtension);

        if (File.Exists(iconFilePath))
        {
            model.IconPath = iconFilePath;
            return;
        }

        var icon = Icon.ExtractAssociatedIcon(executablePath ?? model.ExecutionFilePath);
        var bitmapIcon = icon?.ToBitmap();
        bitmapIcon?.Save(iconFilePath, ImageFormat.Png);
        model.IconPath = iconFilePath;
    }
}