# CrossApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Замовлення.

Сутності: Customer, Product, Order, OrderLine.

Призначення: оформлення замовлень клієнтів, збереження позицій замовлення та підрахунок загальної суми.

## Запуск

dotnet build
dotnet run --project src/Cli

## Середовище

.NET SDK 10.0, Windows x64

## Self-contained publish

win-x64: 76.84 MB  
linux-x64: 78.8 MB

## Структура solution

CrossApp
├── src
│   ├── Core
│   │   ├── Core.csproj
│   │   └── EnvironmentInfo.cs
│   └── Cli
│       ├── Cli.csproj
│       └── Program.cs
├── README.md
└── .gitignore

Cli має ProjectReference на Core.
Core містить спільну логіку, а Cli відповідає за взаємодію з користувачем і форматування виводу.

Для подальшого розвитку предметної області передбачено таку структуру:
- Core/Dto — типи для передачі даних;
- Core/Domain — доменні сутності;
- Core/Storage — реалізації сховищ.

## Build і запуск

dotnet build
dotnet run --project src/Cli -f net10.0

## Публікація

| RID | Режим | Розмір | Потрібен встановлений runtime |
|---|---|---:|---|
| win-x64 | self-contained | 76,85 MB | Ні |
| win-x64 | framework-dependent | 0,19 MB | Так, .NET 10 |
| win-x64 | single-file self-contained | 70,15 MB | Ні |
| win-x64 | trimmed self-contained | 19,54 MB | Ні |

## Лабораторна робота 3 — імпорт даних

Для представлення даних предметної області використовуються record-типи
ProductDto, CustomerDto та OrderDto.

### CSV

Формат файлу:
id;name;price;description

Роздільник: `;`  
Кодування: UTF-8  
Числові значення обробляються з `CultureInfo.InvariantCulture`.

Запуск:

dotnet run --project src/Cli -f net10.0

Або з указаним файлом:

dotnet run --project src/Cli -f net10.0 -- data/correct.csv

### JSON

dotnet run --project src/Cli -f net10.0 -- data/sample.json

### Різнорідні записи

Префікс `P` позначає товар, `C` — клієнта.

dotnet run --project src/Cli -f net10.0 -- --mixed data/mixed.csv