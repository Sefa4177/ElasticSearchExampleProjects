using Elastic.Clients.Elasticsearch;
using Elasticsearch.API.DTOs;
using Elasticsearch.API.Models;
using Elasticsearch.API.Repository;
using System.Net;

namespace Elasticsearch.API.Services
{
    public class ProductServices
    {
        private readonly ProductRepository _Productrepository;
        private readonly ILogger<ProductServices> _logger;

        public ProductServices(ProductRepository productrepository, ILogger<ProductServices> logger)
        {
            _Productrepository = productrepository;
            _logger = logger;
        }

        public async Task<ResponseDto<ProductDto>> SaveAsync(ProductCreateDto request)
        {
            var responseProduct = await _Productrepository.SaveAsync(request.CreateProduct());

            if(responseProduct == null)
            {
                return ResponseDto<ProductDto>.Fail(new List<string> { "Kayıt sırasında bir hata meydana geldi"}, HttpStatusCode.InternalServerError);
            }

            return ResponseDto<ProductDto>.Success(responseProduct.CreateDto(), HttpStatusCode.Created);
        }

        public async Task<ResponseDto<List<ProductDto>>> GetAllAsync()
        {
            var products = await _Productrepository.GetAllAsync();

            var productListDto = new List<ProductDto>();

            foreach (var product in products)
            {
                if (product.Feature == null)
                {
                    productListDto.Add(new ProductDto(product.Id, product.Name, product.Price, product.Stock, null));
                    continue;
                }
                
                productListDto.Add(new ProductDto(product.Id, product.Name, product.Price, product.Stock, new ProductFeatureDto(product.Feature.Width, product.Feature.Height, product.Feature.Color.ToString())));
                
            }

            return ResponseDto<List<ProductDto>>.Success(productListDto, HttpStatusCode.OK);
        }

        public async Task <ResponseDto<ProductDto>> GetByIdAsync(string id)
        {
            var hasProduct = await _Productrepository.GetByIdAsync(id);
            if (hasProduct == null)
            {
                return ResponseDto<ProductDto>.Fail("Ürün bulunamadı", HttpStatusCode.NotFound);
            }

            return ResponseDto<ProductDto>.Success(hasProduct.CreateDto(), HttpStatusCode.OK);
        }

        public async Task<ResponseDto<bool>> UpdateAsync(ProductUpdateDto updateProduct)
        {
            var isSucces = await _Productrepository.UpdateAsync(updateProduct);

            if(!isSucces)
            {
                return ResponseDto<bool>.Fail(new List<string> { "Güncelleme sırasında bir hata meydana geldi" }, HttpStatusCode.InternalServerError);
            }

            return ResponseDto<bool>.Success(true,HttpStatusCode.NoContent);
        }

        public async Task<ResponseDto<bool>> DeleteAsync(string id)
        {
            var deleteResponse = await _Productrepository.DeleteAsync(id);

            if(!deleteResponse.IsValidResponse && deleteResponse.Result == Result.NotFound)
            {
                return ResponseDto<bool>.Fail(new List<string> { "Silmeye çalıştıgınız ürün bulunamadı" }, HttpStatusCode.NotFound);
            }

            if(!deleteResponse.IsValidResponse)
            {
                deleteResponse.TryGetOriginalException(out Exception? exception);
                _logger.LogError(exception,deleteResponse.ElasticsearchServerError?.Error.ToString());

                return ResponseDto<bool>.Fail(new List<string> { "Silme esnasında bir hata meydana geldi" }, HttpStatusCode.InternalServerError);
            }

            return ResponseDto<bool>.Success(true, HttpStatusCode.NoContent);
        }
    }
}
