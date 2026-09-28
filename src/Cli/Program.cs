using Core;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Contains("--json"))
{
    var options = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    Console.WriteLine(JsonSerializer.Serialize(report, options));
    return;
}

Console.WriteLine("CrossApp – інформація про середовище");
Console.WriteLine($"Студент: Муц Анастасія, група ФЕІ-31");
Console.WriteLine(new string('-', 52));

Console.WriteLine($"ОС                : {report.OsDescription}");
Console.WriteLine($"Runtime           : {report.FrameworkDescription}");
Console.WriteLine($"Архітектура       : {report.ProcessArchitecture}");
Console.WriteLine($"RID (визначено)   : {report.DetectedRid}");
Console.WriteLine($"RID (від .NET)    : {report.ReportedRid}");
Console.WriteLine($"Каталог           : {report.BaseDirectory}");
Console.WriteLine($"TFM               : {report.BuildNote}");