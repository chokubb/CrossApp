using Core.Abstractions;
using Core.Services;

IOrderStore store = StoreFactory.Create(args);

OrderService service = new OrderService(store);

return OrderServiceDemo.Run(service, store);