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

## Лабораторна робота 4 — Доменна модель

Предметна область: Замовлення.

У Core/Domain реалізовано:
- Order — замовлення;
- OrderLine — рядок замовлення;
- Customer — клієнт;
- CustomerOrderPolicy — правило для відкритих замовлень;
- OrderStatus — можливі стани замовлення.

### Основні інваріанти

1. Ідентифікатори замовлення, клієнта та товару не можуть бути порожніми.
2. Назва товару не може бути порожньою.
3. Ціна товару не може бути від'ємною.
4. Кількість товару має бути більшою за нуль.
5. Порожнє замовлення не можна підтвердити.
6. Після підтвердження або скасування не можна додавати рядки.
7. Заборонене повторне підтвердження та недопустимі переходи станів.

Некоректні аргументи викликають ArgumentException
або ArgumentOutOfRangeException.

Операції, заборонені поточним станом об'єкта,
викликають InvalidOperationException.

### Інкапсуляція

Сутності мають приватні конструктори.
Створення відбувається через фабричні методи Create.

Список Order.Lines доступний лише для читання.
Змінювати його можна тільки через доменні методи.

### Мапінг DTO

Методи ToDto і FromDto перетворюють доменні сутності
на DTO та відновлюють їх із DTO.

FromDto використовує ті самі перевірки, що і Create.

### Додаткові завдання

1. DomainImportMapper перетворює імпортовані ProductDto
   на доменні OrderLine і збирає помилки перевірки.
2. CustomerOrderPolicy забороняє створення шостого
   відкритого замовлення одного клієнта.
3. OrderStatus містить Draft, Confirmed, Cancelled.
   Допустимі переходи перевіряються через switch expression.

### Запуск демонстрації

dotnet run --project src/Cli -f net10.0

## Лабораторна робота 5 — Сервісний шар

Предметна область: Замовлення.

### Архітектура

У Core/Abstractions створено інтерфейс IOrderStore.

Методи інтерфейсу:
- List() — отримати всі замовлення;
- GetById() — знайти замовлення за ID;
- Add() — додати замовлення;
- Update() — оновити замовлення;
- Remove() — видалити замовлення.

Реалізовано два сховища:
- InMemoryOrderStore — зберігання в оперативній пам'яті;
- FileOrderStore — зберігання у JSON-файлі.

OrderService залежить тільки від IOrderStore.
Залежність передається через конструктор без DI-контейнера.

### Файлове сховище

Файл: data/orders-lab05.json

Для збереження використовується OrderDto.
Перетворення здійснюється через ToDto() і FromDto().

### Додаткові завдання

1. CachingOrderStore — декоратор із кешуванням.
2. OrderService.Search(Func<Order, bool>) —
   пошук за довільною умовою.
3. StoreFactory — фабрика для вибору сховища.

### Команди запуску

dotnet run --project src/Cli -f net10.0

dotnet run --project src/Cli -f net10.0 -- --file

dotnet run --project src/Cli -f net10.0 -- --cache

dotnet run --project src/Cli -f net10.0 -- --file --cache

### Git

Коміти лабораторної №5:
- lab05: add order service and storage implementations
- lab05: add manual DI demo and optional tasks