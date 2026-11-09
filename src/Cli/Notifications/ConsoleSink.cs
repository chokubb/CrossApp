using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(OrderChangedEventArgs e)
    {
        Console.WriteLine(
            $"[ConsoleSink] {e.Kind} | {e.OrderId} | " +
            $"позицій: {e.LinesCount} | сума: {e.Total:F2}");
    }
}