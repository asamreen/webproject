using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
{
    // Fetch paginated orders and total count
    var (orders, totalOrders) = await _orderService.GetOrdersAsync(page, pageSize);

    // Format the response
    var formattedOrders = orders.Select(order => new
    {
        id = order.Id,
        attributes = new
        {
            address = order.Address,
            createdAt = order.CreatedAt,
            updatedAt = order.UpdatedAt,
            name = order.Name,
            orderTotal = order.OrderTotal,
            cartItems = order.CartItems.Select(cartItem => new
            {
                cartID = cartItem.CartID,
                productID = cartItem.ProductID,
                company = cartItem.Company,
                image = cartItem.Image,
                title = cartItem.Title,
                productColor = cartItem.ProductColor,
                price = cartItem.Price,
                amount = cartItem.Amount
            }).ToList(),
            numItemsInCart = order.NumItemsInCart
        }
    }).ToList();

    // Add pagination meta
    var meta = new
    {
        pagination = new
        {
            page,
            pageSize,
            pageCount = (int)Math.Ceiling((double)totalOrders / pageSize),
            total = totalOrders
        }
    };

    return Ok(new { data = formattedOrders, meta });
}

[HttpPost]
public async Task<IActionResult> CreateOrder([FromBody] JsonElement payload)
{
    // Extract "data" from the payload
    if (!payload.TryGetProperty("data", out JsonElement data))
    {
        return BadRequest(new { message = "Payload must include a 'data' property." });
    }

    // Deserialize the data into an Order object
    var order = System.Text.Json.JsonSerializer.Deserialize<Order>(data.ToString(), new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (order == null)
    {
        return BadRequest(new { message = "Invalid order data." });
    }

    // Set additional properties
    order.CreatedAt = DateTime.UtcNow;
    order.UpdatedAt = DateTime.UtcNow;
    order.Id = await _orderService.GetNextOrderIdAsync();

    // Save the order
    await _orderService.CreateOrderAsync(order);

    // Format the response
    return Ok(new
    {
        id = order.Id,
        data = new
        {
            name = order.Name,
            address = order.Address,
            chargeTotal = order.ChargeTotal,
            orderTotal = order.OrderTotal,
            cartItems = order.CartItems.Select(ci => new
            {
                cartID = ci.CartID,
                productID = ci.ProductID,
                image = ci.Image,
                title = ci.Title,
                price = ci.Price,
                company = ci.Company,
                productColor = ci.ProductColor,
                amount = ci.Amount
            }),
            numItemsInCart = order.NumItemsInCart
        }
    });
}

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);

        if (order == null)
        {
            return NotFound(new { message = "Order not found." });
        }

        return Ok(new
        {
            id = order.Id,
            address = order.Address,
            createdAt = order.CreatedAt,
            updatedAt = order.UpdatedAt,
            name = order.Name,
            orderTotal = order.OrderTotal,
            cartItems = order.CartItems,
            numItemsInCart = order.NumItemsInCart
        });
    }
}
