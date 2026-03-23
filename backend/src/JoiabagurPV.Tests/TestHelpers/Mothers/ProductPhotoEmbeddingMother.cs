using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Infrastructure.Data;

namespace JoiabagurPV.Tests.TestHelpers.Mothers;

/// <summary>
/// Mother Object for creating ProductPhotoEmbedding entities in integration tests.
/// Uses Bogus via TestDataGenerator for realistic default values.
/// </summary>
public class ProductPhotoEmbeddingMother
{
    private readonly ApplicationDbContext _context;
    private readonly ProductPhotoEmbedding _embedding;

    public ProductPhotoEmbeddingMother(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _embedding = TestDataGenerator.CreateProductPhotoEmbedding();
    }

    /// <summary>Sets the product photo ID this embedding belongs to.</summary>
    public ProductPhotoEmbeddingMother WithPhotoId(Guid photoId)
    {
        _embedding.ProductPhotoId = photoId;
        return this;
    }

    /// <summary>Sets the product ID.</summary>
    public ProductPhotoEmbeddingMother WithProductId(Guid productId)
    {
        _embedding.ProductId = productId;
        return this;
    }

    /// <summary>Sets the product SKU.</summary>
    public ProductPhotoEmbeddingMother WithSku(string sku)
    {
        _embedding.ProductSku = sku;
        return this;
    }

    /// <summary>Sets the embedding vector (serialized as JSON text).</summary>
    public ProductPhotoEmbeddingMother WithVector(float[] vector)
    {
        _embedding.EmbeddingVector = System.Text.Json.JsonSerializer.Serialize(vector);
        return this;
    }

    /// <summary>Persists the embedding to the database and returns the entity.</summary>
    public async Task<ProductPhotoEmbedding> CreateAsync()
    {
        _context.ProductPhotoEmbeddings.Add(_embedding);
        await _context.SaveChangesAsync();
        return _embedding;
    }
}
