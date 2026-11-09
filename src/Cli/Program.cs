using Core.Abstractions;
using Core.Services;

IOrderStore store = StoreFactory.Create(args);

OrderService service = new OrderService(store);

return OrderEventsDemo.Run(service);