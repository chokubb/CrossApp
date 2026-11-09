using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class FileSink : INotificationSink
{
    private readonly string _path;

    public FileSink(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Шлях до лог-файла не може бути порожнім.",
                nameof(path));
        }

        _path = Path.GetFullPath(path);
    }

    public void Notify(OrderChangedEventArgs e)
    {
        string? directory = Path.GetDirectoryName(_path);

        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        string line =
            $"{e.At:HH:mm:ss} {e.Kind} | {e.OrderId} | " +
            $"позицій: {e.LinesCount} | сума: {e.Total:F2}";

        File.AppendAllText(
            _path,
            line + Environment.NewLine);
    }
}