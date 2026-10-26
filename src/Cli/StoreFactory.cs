using Core;
using Core.Abstractions;
using Core.Storage;

public static class StoreFactory
{
    public static IOrderStore Create(string[] args)
    {
        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cache");

        IOrderStore store;

        if (useFile)
        {
            string path = Path.Combine(
                "data",
                "orders-lab05.json");

            store = new FileOrderStore(path);
        }
        else
        {
            store = new InMemoryOrderStore(
                SampleData.Orders());
        }

        if (useCache)
        {
            store = new CachingOrderStore(store);
        }

        return store;
    }
}