using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;

var info = new
{
    Title = "CrossApp – практикум з крос-платформного програмування",
    Student = "Муц Анастасія, група ФЕІ-31",
    OSDescription = RuntimeInformation.OSDescription,
    OSVersion = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    BaseDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Замовлення",
    Entities = new[]
    {
        "Customer",
        "Product",
        "Order",
        "OrderLine"
    }
};

if (args.Contains("--json"))
{
    Console.WriteLine(JsonSerializer.Serialize(info));
}
else
{
    Console.WriteLine(info.Title);
    Console.WriteLine($"Студент: {info.Student}");
    Console.WriteLine(new string('-', 52));

    Console.WriteLine($"ОС (OSDescription)      : {info.OSDescription}");
    Console.WriteLine($"ОС (Environment)        : {info.OSVersion}");
    Console.WriteLine($"Архітектура процесу     : {info.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)       : {info.DotNetVersion}");
    Console.WriteLine($"Runtime                  : {info.Runtime}");
    Console.WriteLine($"Каталог застосунку       : {info.BaseDirectory}");
    Console.WriteLine($"Поточний каталог         : {info.CurrentDirectory}");

    Console.WriteLine(new string('-', 52));

    Console.WriteLine(
        "Предметна область: Замовлення " +
        "(клієнти, товари, замовлення, рядки замовлення)"
    );
}