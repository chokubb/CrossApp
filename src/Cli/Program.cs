using Core.Abstractions;
using Core.Services;

string command = args.FirstOrDefault(
    arg => !arg.StartsWith("--")) ?? "import";

IOrderStore store = StoreFactory.Create(args);

OrderService service = new OrderService(store);

// Зберігаємо всі підкоманди лабораторної №7.
if (command.StartsWith("report-", StringComparison.Ordinal))
{
    return OrderReportsDemo.Run(service, args);
}

if (command is not (
    "import" or
    "import-stream" or
    "import-bad" or
    "import-closed" or
    "import-timeout" or
    "import-wait"))
{
    Console.WriteLine("Невідома команда: " + command);
    return 1;
}

return await OrderImportDemo.RunAsync(service, command);