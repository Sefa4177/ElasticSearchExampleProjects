﻿using Elasticsearch.API.DTOs;
using Elasticsearch.API.Models;
using Nest;
using System.Collections;
using System.Collections.Immutable;

namespace Elasticsearch.API.Repository
{
    public class ProductRepository
    {
        private readonly ElasticClient _client;
        private const string _indexName = "products";
        public ProductRepository(ElasticClient client)
        {
            _client = client;
        }

        public async Task<Product?> SaveAsync(Product newProduct)
        {
            newProduct.Created = DateTime.Now;

            var response = await _client.IndexAsync(newProduct, x => x.Index(_indexName).Id(Guid.NewGuid().ToString()));

            if(!response.IsValid) return null;

            newProduct.Id = response.Id;

            return newProduct;
        }

        public async Task<ImmutableList<Product>> GetAllAsync()
        {
            var result = await _client.SearchAsync<Product>(s => s.Index(_indexName).Query(q => q.MatchAll()));

            foreach (var hit in result.Hits) hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            var response = await _client.GetAsync<Product>(id, x=> x.Index(_indexName));

            if(!response.IsValid) return null;

            response.Source.Id = response.Id;  
            return response.Source;
        }

        public async Task<bool> UpdateAsync(ProductUpdateDto updateProduct)
        {
            var response = await _client.UpdateAsync<Product, ProductUpdateDto>(updateProduct.id, x => x.Index(_indexName).Doc(updateProduct));
            return response.IsValid;
        }

        public async Task<DeleteResponse> DeleteAsync(string id)
        {
            var response = await _client.DeleteAsync<Product>(id,x => x.Index(_indexName));

            return response;
        }

    }
}
