using Core.Abstractions;
using Core.Services;

IOrderStore store = StoreFactory.Create(args);

OrderService service = new OrderService(store);

if (args.Contains("--lab05"))
{
    return OrderServiceDemo.Run(service, store);
}

return OrderEventsDemo.Run(service);