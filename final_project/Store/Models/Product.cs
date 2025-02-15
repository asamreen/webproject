using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

    public class Product
    {
         [BsonId]
    [BsonRepresentation(BsonType.Int32)] 
    public int Id { get; set; }


        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("company")]
        public string Company { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("featured")]
        public bool Featured { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("image")]
        public string Image { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("shipping")]
        public bool Shipping { get; set; }

        [BsonElement("colors")]
        public List<string> Colors { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
