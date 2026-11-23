
# CrossApp

Наскрізний навчальний проєкт з курсу «Крос-платформне програмування».

**Предметна область:** замовлення інтернет-магазину.

Проєкт розроблено на C#/.NET. Підтримуються .NET 8 і .NET 10.

## Структура проєкту

- `src/Core/Domain` — доменні сутності та правила.
- `src/Core/Dto` — типи для передачі та збереження даних.
- `src/Core/Abstractions` — інтерфейси.
- `src/Core/Storage` — сховища замовлень.
- `src/Core/Services` — сервісний шар.
- `src/Core/Events` — типи доменних подій.
- `src/Core/Import` — імпорт даних.
- `src/Cli` — консольний застосунок і демонстрація.
- `src/Cli/Notifications` — обробники повідомлень.
- `data` — тестові дані.

Проєкт Cli використовує Core через ProjectReference.

## Збірка

Запускати команди з кореневої папки CrossApp.

```powershell
dotnet build
```

## Запуск лабораторної №6

Для запуску цієї демонстрації потрібно перейти
до відповідного коміту з історії Git.

## Доменна модель

Основні сутності:
- `Order` — замовлення;
- `OrderLine` — позиція замовлення;
- `Customer` — клієнт.

Доменні методи перевіряють інваріанти: коректну
ціну, додатну кількість, допустимі зміни стану тощо.

## Сервісний шар

`OrderService` організовує операції із замовленнями.

Сервіс залежить від інтерфейсу `IOrderStore`,
а не від конкретного сховища.

Реалізації:
- `InMemoryOrderStore` — оперативна пам'ять;
- `FileOrderStore` — JSON-файл;
- `CachingOrderStore` — декоратор кешування.

`StoreFactory` обирає реалізацію за аргументами запуску.

## Лабораторна №6 — доменні події

У `OrderService` оголошено подію `Changed`.

Види змін:
- `OrderCreated`;
- `LineAdded`;
- `OrderConfirmed`;
- `OrderCancelled`;
- `OrderRemoved`.

Події виникають після успішних операцій.

У `OrderEventsDemo` реалізовано:
- два підписники;
- підписку через `+=`;
- відписку через `-=`;
- лічильник подій із лямбдою;
- перевірку відсутності подій після помилок;
- пошук через `Func<Order, bool>`.

Виведення повідомлень і запис логів виконуються
в Cli, а не безпосередньо в доменному коді Core.

### Додаткові завдання

1. `INotificationSink` з реалізаціями `ConsoleSink`
   та `FileSink`. Обробники передаються сервісу
   через конструктор.

2. `Action<string>` в `ProductCsvImporter.Load()`
   повідомляє про пропущені рядки CSV.

Для демонстрації імпорту використовується
`data/sample.csv`.

### Журнали

Під час запуску створюються:

- `src/Cli/bin/Debug/net10.0/logs/app.log`
- `src/Cli/bin/Debug/net10.0/logs/sinks.log`

Це згенеровані файли, які не додаються до Git.

## Попередні лабораторні

Попередні реалізації збережені в історії Git.

| Лабораторна | Коміт |
|---|---|
| №3 | `b772261` |
| №4 | `71faf4c` |
| №5 | `a9c6d61` |
| №6 до рефакторингу | `097697c` |

Наприклад, перехід до лабораторної №5:

```powershell
git switch --detach a9c6d61
dotnet run --project src/Cli -f net10.0
```

Повернення до актуальної версії:

```powershell
git switch main
```

Перед перемиканням необхідно зберегти або
закомітити локальні зміни.


## Лабораторна робота 7 — LINQ-звіти

Предметна область: замовлення інтернет-магазину.

### Реалізація

У Core/Reports створено ReportService і DTO для звітів.

Реалізовано:
- TopCustomers — топ клієнтів за сумою замовлень;
- HighValueOrders — замовлення дорожчі за задану суму;
- TopOrders — найбільші замовлення;
- ByStatus — групування за статусом;
- ByMonth — групування за місяцем;
- Totals — кількість, сума, середній чек і максимум.

Для запитів використовуються Where, Select, GroupBy,
OrderBy, OrderByDescending, ThenBy, Take, Sum,
Average, Max і Count.

Методи звітів приймають IEnumerable<Order> та
повертають DTO. Загальні підсумки повертаються
іменованим кортежем.

Для Top-N використовується додаткове сортування
за ID, щоб порядок результатів був стабільним.

Порожня колекція обробляється без винятків.

SampleData містить 18 замовлень із різними
статусами, сумами та місяцями створення.

### Команди

```powershell
dotnet run --project src/Cli -f net10.0 -- report-top
dotnet run --project src/Cli -f net10.0 -- report-high
dotnet run --project src/Cli -f net10.0 -- report-groups
dotnet run --project src/Cli -f net10.0 -- report-months
dotnet run --project src/Cli -f net10.0 -- report-sum
dotnet run --project src/Cli -f net10.0 -- report-orders-top
dotnet run --project src/Cli -f net10.0 -- report-deferred
dotnet run --project src/Cli -f net10.0 -- report-empty
```

Без указаної команди запускається report-top.

### Відкладене виконання LINQ

Метод DeferredHighValue створює запит Where/Select,
але не викликає ToList(), тому обробка відкладається.

Перший Count() виконує запит над початковим списком.
Після додавання ще одного відповідного замовлення
другий Count() повертає число на одиницю більше.

Після виклику ToList() створюється окремий знімок
результату. Подальше очищення вихідного списку
вже не змінює цей знімок.

### Додаткові завдання

1. GroupJoin — об'єднання клієнтів та їхніх
   замовлень у CustomerOrderSummary.

2. TopBy — власний метод-розширення для
   вибору N елементів із найбільшими показниками.
   Використаний у TopCustomers і TopOrders.

3. ByStatusQuery — групування через синтаксис
   запитів from/group/into/orderby/select.
   Порівняно з методним синтаксисом у ByStatus.
   Методний синтаксис зручний для коротких
   послідовних операцій, а синтаксис запитів
   може бути наочнішим для групування.

4. BenchmarkTop — порівняння часу LINQ-запитів
   на 100 000 записах за допомогою Stopwatch.
   Порівнюються варіанти з проміжним ToList()
   і без нього. Час вимірюється окремо
   від створення тестових даних.

### Команди додаткових завдань

```powershell
dotnet run --project src/Cli -f net10.0 -- report-join
dotnet run --project src/Cli -f net10.0 -- report-orders-top
dotnet run --project src/Cli -f net10.0 -- report-query
dotnet run --project src/Cli -f net10.0 -- report-benchmark
```

Усі LINQ-запити звітів розташовані в Core.
Cli відповідає за вибір команди та виведення результатів.


## Лабораторна робота 8 — async/await, скасування та unit-тести

Предметна область: замовлення.

### Асинхронний імпорт

Реалізовано інтерфейс IOrderLineImporter та клас
JsonOrderLineImporter для читання товарних позицій із JSON.

LoadAsync() використовує FileStream(useAsync: true)
і JsonSerializer.DeserializeAsync із CancellationToken.

OrderService.ImportAsync() додає позиції до замовлення,
використовуючи наявний метод AddLine() та його інваріанти.

Подія LineAdded виникає після кожного успішного додавання.

Повторний імпорт позицій з однаковими ProductId
не створює дублікатів у межах одного замовлення.

### Скасування

У Cli використано CancellationTokenSource.

Скасування підтримується через:
- Ctrl+C;
- таймаут;
- заздалегідь скасований токен.

Токен передається від Cli до OrderService,
а звідти до JsonOrderLineImporter.

OperationCanceledException обробляється окремо
від доменних і технічних помилок.

Потоковий імпорт може залишити вже додані позиції,
якщо скасування відбудеться посеред обробки.
Транзакційного відкату в цій лабораторній немає.

### Винятки

DomainException використовується для порушень
правил замовлення та некоректних імпортованих даних.

ArgumentException і ArgumentOutOfRangeException
залишаються для неправильних аргументів методів.

FileNotFoundException, IOException і JsonException
обробляються як технічні помилки.

### Unit-тести

Створено окремий проєкт tests/Core.Tests,
який має посилання на Core, але не залежить від Cli.

Тести перевіряють:
- інваріанти замовлення;
- незмінність стану після відмови;
- одноразове виникнення події;
- LINQ-звіти та стабільне сортування;
- асинхронний імпорт;
- правильну суму замовлення;
- потоковий імпорт;
- скасування;
- доменні винятки.

Тестові дані незалежні від SampleData.

Запуск:

```powershell
dotnet test
```

### Команди імпорту

```powershell
dotnet run --project src/Cli -f net10.0 -- import
dotnet run --project src/Cli -f net10.0 -- import-stream
dotnet run --project src/Cli -f net10.0 -- import-bad
dotnet run --project src/Cli -f net10.0 -- import-closed
dotnet run --project src/Cli -f net10.0 -- import-timeout
dotnet run --project src/Cli -f net10.0 -- import-wait
```

### Додаткові завдання

1. Потоковий імпорт через IAsyncEnumerable та await foreach.

2. Зв'язані CancellationTokenSource і тест,
   який перевіряє скасування саме таймаутом.

3. IProgress<int> для відображення відсотків
   виконання звичайного імпорту.

4. Вимірювання покриття Core unit-тестами.

Команда покриття:

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

Результат формується у coverage.cobertura.xml.

### Попередні лабораторні

LINQ-звіти з лабораторної №7 залишаються доступними
через підкоманди report-top, report-groups, report-sum
та інші.

Історичні версії лабораторних також збережені в Git.

## Репозиторій

https://github.com/chokubb/CrossApp
