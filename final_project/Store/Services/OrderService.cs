using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrderService
{
    private readonly IMongoCollection<Order> _orders;

    public OrderService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var database = client.GetDatabase(config["MongoDB:Database"]);
        _orders = database.GetCollection<Order>("Orders");
    }

    public async Task<Order> GetOrderByIdAsync(int id)
    {
        return await _orders.Find(order => order.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateOrderAsync(Order order)
    {
        await _orders.InsertOneAsync(order);
    }

    public async Task<(List<Order>, int)> GetOrdersAsync(int page, int pageSize)
    {
        int skip = (page - 1) * pageSize;

        var orders = await _orders
            .Find(_ => true)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();

        var totalOrders = (int)await _orders.CountDocumentsAsync(_ => true);

        return (orders, totalOrders);
    }
    public async Task<int> GetTotalOrderCountAsync()
{
    return (int)await _orders.CountDocumentsAsync(_ => true);
}

    public async Task<int> GetNextOrderIdAsync()
    {
        var lastOrder = await _orders
            .Find(_ => true)
            .SortByDescending(o => o.Id)
            .Limit(1)
            .FirstOrDefaultAsync();

        return lastOrder?.Id + 1 ?? 1; // If no orders exist, start with 1
    }
}
