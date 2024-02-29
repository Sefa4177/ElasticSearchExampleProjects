namespace Elasticsearch.API.DTOs
{
    public record ProductUpdateDto(string id,string Name, decimal Price, int Stock, ProductFeatureDto Feature)
    {

    }
}
