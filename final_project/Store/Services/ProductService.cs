using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class Counter
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }

    public int Value { get; set; }
}
    public class ProductService
    {
         private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<Counter> _counters;
   

    public ProductService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var database = client.GetDatabase(config["MongoDB:Database"]);
        _products = database.GetCollection<Product>("Products");
        _counters = database.GetCollection<Counter>("Counters");
    }

    public List<string> GetCategories()
{
    return new List<string>
    {
        "all",
        "Tables",
        "Chairs",
        "Kids",
        "Sofas",
        "Beds"
    };
}

public List<string> GetCompanies()
{
    return new List<string>
    {
        "all",
        "Modenza",
        "Luxora",
        "Artifex",
        "Comfora",
        "Homestead"
    };
}



public async Task<int> GetTotalProductCountAsync()
{
    return (int)await _products.CountDocumentsAsync(_ => true);
}

public async Task<List<string>> GetCategoriesAsync()
{
    return await _products.Distinct<string>("Category", Builders<Product>.Filter.Empty).ToListAsync();
}

public async Task<List<string>> GetCompaniesAsync()
{
    return await _products.Distinct<string>("Company", Builders<Product>.Filter.Empty).ToListAsync();
}
    private async Task<int> GetNextIdAsync()
    {
        var filter = Builders<Counter>.Filter.Eq(c => c.Id, "product_id");
        var update = Builders<Counter>.Update.Inc(c => c.Value, 1);
        var options = new FindOneAndUpdateOptions<Counter>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var counter = await _counters.FindOneAndUpdateAsync(filter, update, options);
        return counter.Value;
    }public async Task SeedProductsAsync(List<Product> products)
{
    // Drop the collection to avoid duplicate IDs
    var filter = Builders<Product>.Filter.Empty;
    await _products.DeleteManyAsync(filter);

    // Ensure unique IDs
    int idCounter = 1;
    foreach (var product in products)
    {
        product.Id = idCounter++;
    }

    await _products.InsertManyAsync(products);
}


public async Task<List<Product>> GetProductsAsync(int page, int pageSize)
{
    return await _products
        .Find(_ => true)
        .Skip((page - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();
}
public async Task<(List<Product>, int)> GetFilteredProductsAsync(ProductFilters filters, int page, int pageSize)
{
    // Start with an empty filter
    var filterDefinition = Builders<Product>.Filter.Empty;

    // Apply search filter
    if (!string.IsNullOrEmpty(filters.Search))
    {
        filterDefinition &= Builders<Product>.Filter.Regex(p => p.Title, new MongoDB.Bson.BsonRegularExpression(filters.Search, "i"));
    }

    // Apply category filter (ignore "all")
    if (!string.IsNullOrEmpty(filters.Category) && filters.Category.ToLower() != "all")
    {
        filterDefinition &= Builders<Product>.Filter.Eq(p => p.Category, filters.Category);
    }

    // Apply company filter (ignore "all")
    if (!string.IsNullOrEmpty(filters.Company) && filters.Company.ToLower() != "all")
    {
        filterDefinition &= Builders<Product>.Filter.Eq(p => p.Company, filters.Company);
    }

    // Apply price filter
    if (filters.Price.HasValue)
    {
        filterDefinition &= Builders<Product>.Filter.Lte(p => p.Price, filters.Price.Value);
    }

    // shipping filter
if (filters.Shipping.HasValue)
{
    Console.WriteLine(filters.Shipping.Value);
    if (filters.Shipping.Value) // Include only products with shipping
    {
        filterDefinition &= Builders<Product>.Filter.Eq(p => p.Shipping, true);
    }
   
}

    // Set sorting based on the order field
    var sortDefinition = Builders<Product>.Sort.Ascending(p => p.Title);
    if (filters.Order.ToLower() == "z-a")
    {
        sortDefinition = Builders<Product>.Sort.Descending(p => p.Title);
    }
    else if (filters.Order.ToLower() == "high")
    {
        sortDefinition = Builders<Product>.Sort.Descending(p => p.Price);
    }
    else if (filters.Order.ToLower() == "low")
    {
        sortDefinition = Builders<Product>.Sort.Ascending(p => p.Price);
    }

    // Calculate the total count of filtered products
    var total = await _products.CountDocumentsAsync(filterDefinition);
    Console.WriteLine(total);
    // Fetch filtered and paginated products
    var products = await _products
        .Find(filterDefinition)
        .Sort(sortDefinition)
        .Skip((page - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();

    return (products, (int)total);
}

public async Task<Product> GetProductByIdAsync(int id)
{
    return await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
}

         public async Task CreateProductAsync(Product product)
    {
        product.Id = await GetNextIdAsync();
        await _products.InsertOneAsync(product);
    }
        public static List<Product> GetSeedProducts()
{
    return new List<Product>
    {
        new Product
    {
        Title = "avant-garde lamp",
        Company = "Modenza",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = true,
        Category = "Kids",
        Image = "https://images.pexels.com/photos/943150/pexels-photo-943150.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 17999,
        Shipping = false,
        Colors = new List<string> { "#33FF57", "#3366FF" },
        CreatedAt = DateTime.Parse("2023-08-10T10:07:41.876Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T10:16:43.298Z")
    },
    new Product
    {
        Title = "chic chair",
        Company = "Luxora",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Chairs",
        Image = "https://images.pexels.com/photos/5705090/pexels-photo-5705090.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 33999,
        Shipping = true,
        Colors = new List<string> { "#FF5733", "#33FF57", "#3366FF" },
        CreatedAt = DateTime.Parse("2023-08-10T09:32:58.392Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T09:33:03.728Z")
    },
    new Product
    {
        Title = "coffee table",
        Company = "Modenza",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = true,
        Category = "Tables",
        Image = "https://images.pexels.com/photos/3679601/pexels-photo-3679601.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=2",
        Price = 17999,
        Shipping = false,
        Colors = new List<string> { "#FF5733", "#FFFF00" },
        CreatedAt = DateTime.Parse("2023-08-02T14:32:02.775Z"),
        UpdatedAt = DateTime.Parse("2023-08-04T07:35:16.880Z")
    },
    new Product
    {
        Title = "comfy bed",
        Company = "Homestead",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = true,
        Category = "Beds",
        Image = "https://images.pexels.com/photos/1034584/pexels-photo-1034584.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 12999,
        Shipping = false,
        Colors = new List<string> { "#FF5733" },
        CreatedAt = DateTime.Parse("2023-08-02T14:34:10.146Z"),
        UpdatedAt = DateTime.Parse("2023-08-08T14:06:28.575Z")
    },
    new Product
    {
        Title = "contemporary sofa",
        Company = "Comfora",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Sofas",
        Image = "https://images.pexels.com/photos/1571459/pexels-photo-1571459.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 15999,
        Shipping = false,
        Colors = new List<string> { "#FFFF00" },
        CreatedAt = DateTime.Parse("2023-08-10T09:34:24.429Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T09:34:27.095Z")
    },
    new Product
    {
        Title = "cutting-edge bed",
        Company = "Homestead",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Beds",
        Image = "https://images.pexels.com/photos/2029694/pexels-photo-2029694.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 8499,
        Shipping = true,
        Colors = new List<string> { "#FFFF00", "#3366FF" },
        CreatedAt = DateTime.Parse("2023-08-10T10:08:58.922Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T10:09:01.251Z")
    },
    new Product
    {
        Title = "futuristic shelves",
        Company = "Luxora",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Kids",
        Image = "https://images.pexels.com/photos/2177482/pexels-photo-2177482.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 9499,
        Shipping = true,
        Colors = new List<string> { "#33FF57", "#FFFF00" },
        CreatedAt = DateTime.Parse("2023-08-10T10:02:51.583Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T10:02:53.797Z")
    },
new Product
{
    Title = "glass table",
    Company = "Modenza",
    Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
    Featured = false,
    Category = "Tables",
    Image = "https://images.pexels.com/photos/1571452/pexels-photo-1571452.jpeg?auto=compress&cs=tinysrgb&w=1600",
    Price = 15999,
    Shipping = false,
    Colors = new List<string> { "#FF5733", "#3366FF" },
    CreatedAt = DateTime.Parse("2023-08-10T10:10:46.803Z"),
    UpdatedAt = DateTime.Parse("2023-08-10T10:10:48.971Z")
},
new Product
{
    Title = "King Bed",
    Company = "Homestead",
    Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
    Featured = false,
    Category = "Beds",
    Image = "https://images.pexels.com/photos/6489083/pexels-photo-6489083.jpeg?auto=compress&cs=tinysrgb&w=1600",
    Price = 18999,
    Shipping = true,
    Colors = new List<string> { "#FF5733" },
    CreatedAt = DateTime.Parse("2023-08-10T10:11:38.696Z"),
    UpdatedAt = DateTime.Parse("2023-08-10T10:11:40.915Z")
},
new Product
{
    Title = "Lounge Chair",
    Company = "Luxora",
    Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
    Featured = false,
    Category = "Chairs",
    Image = "https://images.pexels.com/photos/2082090/pexels-photo-2082090.jpeg?auto=compress&cs=tinysrgb&w=1600",
    Price = 25999,
    Shipping = false,
    Colors = new List<string> { "#FF5733", "#33FF57", "#3366FF" },
    CreatedAt = DateTime.Parse("2023-08-10T10:13:29.629Z"),
    UpdatedAt = DateTime.Parse("2023-08-10T10:15:29.354Z")
},new Product
    {
        Title = "Minimalist Shelves",
        Company = "Artifex",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Kids",
        Image = "https://images.pexels.com/photos/439227/pexels-photo-439227.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 43999,
        Shipping = false,
        Colors = new List<string> { "#FF5733", "#FFFF00" },
        CreatedAt = DateTime.Parse("2023-08-10T09:31:43.653Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T09:31:46.013Z")
    },
    new Product
    {
        Title = "modern sofa",
        Company = "Comfora",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Sofas",
        Image = "https://images.pexels.com/photos/6480707/pexels-photo-6480707.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=2",
        Price = 29999,
        Shipping = false,
        Colors = new List<string> { "#FF5733", "#33FF57", "#FFFF00" },
        CreatedAt = DateTime.Parse("2023-08-02T14:25:10.447Z"),
        UpdatedAt = DateTime.Parse("2023-08-08T14:04:21.619Z")
    },
    new Product
    {
        Title = "modern table",
        Company = "Modenza",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Tables",
        Image = "https://images.pexels.com/photos/447592/pexels-photo-447592.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 38999,
        Shipping = true,
        Colors = new List<string> { "#33FF57", "#3366FF" },
        CreatedAt = DateTime.Parse("2023-08-08T14:02:24.368Z"),
        UpdatedAt = DateTime.Parse("2023-08-08T14:53:24.452Z")
    },
    new Product
    {
        Title = "reclining sofa",
        Company = "Comfora",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Sofas",
        Image = "https://images.pexels.com/photos/4316737/pexels-photo-4316737.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 32999,
        Shipping = false,
        Colors = new List<string> { "#FF5733", "#33FF57", "#3366FF", "#FFFF00" },
        CreatedAt = DateTime.Parse("2023-08-10T10:05:57.858Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T10:06:01.554Z")
    },
    new Product
    {
        Title = "Sectional Sofa",
        Company = "Comfora",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Sofas",
        Image = "https://images.pexels.com/photos/4857775/pexels-photo-4857775.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 35999,
        Shipping = true,
        Colors = new List<string> { "#FF5733", "#33FF57", "#3366FF", "#FFFF00" },
        CreatedAt = DateTime.Parse("2023-08-10T10:14:14.760Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T10:14:17.029Z")
    },
    new Product
    {
        Title = "sleek bed",
        Company = "Homestead",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Beds",
        Image = "https://images.pexels.com/photos/16869701/pexels-photo-16869701/free-photo-of-modern-luxury-real-estate-property-interior-bedroom.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 53999,
        Shipping = true,
        Colors = new List<string> { "#FFFF00", "#3366FF" },
        CreatedAt = DateTime.Parse("2023-08-10T09:30:26.259Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T09:30:28.876Z")
    },
    new Product
    {
        Title = "sleek chair",
        Company = "Luxora",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Chairs",
        Image = "https://images.pexels.com/photos/116910/pexels-photo-116910.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 19999,
        Shipping = false,
        Colors = new List<string> { "#FF5733", "#33FF57", "#3366FF", "#FFFF00" },
        CreatedAt = DateTime.Parse("2023-08-01T11:14:57.336Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T10:17:13.540Z")
    },
new Product
    {
        Title = "streamlined table",
        Company = "Modenza",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Tables",
        Image = "https://images.pexels.com/photos/890669/pexels-photo-890669.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 20999,
        Shipping = true,
        Colors = new List<string> { "#FF5733", "#3366FF" },
        CreatedAt = DateTime.Parse("2023-08-10T09:36:07.565Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T09:36:09.798Z")
    },
    new Product
    {
        Title = "stylish bed",
        Company = "Homestead",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Beds",
        Image = "https://images.pexels.com/photos/6758398/pexels-photo-6758398.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 16999,
        Shipping = true,
        Colors = new List<string> { "#FF5733" },
        CreatedAt = DateTime.Parse("2023-08-10T10:01:20.801Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T10:01:24.100Z")
    },
    new Product
    {
        Title = "Toy Shelf",
        Company = "Luxora",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Kids",
        Image = "https://images.pexels.com/photos/3932929/pexels-photo-3932929.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 7999,
        Shipping = false,
        Colors = new List<string> { "#33FF57", "#FFFF00" },
        CreatedAt = DateTime.Parse("2023-08-10T10:12:28.626Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T10:12:30.715Z")
    },


    new Product
    {
        Title = "velvet sofa",
        Company = "Luxora",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Chairs",
        Image = "https://images.pexels.com/photos/4916510/pexels-photo-4916510.jpeg?auto=compress&cs=tinysrgb&w=1600",
        Price = 28999,
        Shipping = true,
        Colors = new List<string> { "#FF5733", "#33FF57", "#FFFF00" },
        CreatedAt = DateTime.Parse("2023-08-10T10:04:26.080Z"),
        UpdatedAt = DateTime.Parse("2023-08-10T10:04:29.084Z")
    },
    new Product
    {
        Title = "wooden shelves",
        Company = "Artifex",
        Description = "Cloud bread VHS hell of banjo bicycle rights jianbing umami mumblecore etsy 8-bit pok pok +1 wolf. Vexillologist yr dreamcatcher waistcoat, authentic chillwave trust fund. Viral typewriter fingerstache pinterest pork belly narwhal. Schlitz venmo everyday carry kitsch pitchfork chillwave iPhone taiyaki trust fund hashtag kinfolk microdosing gochujang live-edge",
        Featured = false,
        Category = "Kids",
        Image = "https://images.pexels.com/photos/3932930/pexels-photo-3932930.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=2",
        Price = 11999,
        Shipping = true,
        Colors = new List<string> { "#33FF57", "#3366FF" },
        CreatedAt = DateTime.Parse("2023-08-02T14:36:43.227Z"),
        UpdatedAt = DateTime.Parse("2023-08-04T07:35:59.208Z")
    }




    };
}

}
