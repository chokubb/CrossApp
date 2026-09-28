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
| win-x64 | self-contained | ___ MB | Ні |
| win-x64 | framework-dependent | ___ MB | Так, .NET 10 |
| win-x64 | single-file self-contained | ___ MB | Ні |
| win-x64 | trimmed self-contained | ___ MB | Ні |