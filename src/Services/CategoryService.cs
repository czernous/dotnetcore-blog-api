using api.Interfaces;
using api.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable 1591 

namespace api.Services
{
    public class CategoryService : DbCrudService<Category>
    {
        private readonly IMongoCollection<Category> _categories;
        private static string collectionName = "Categories";
        public CategoryService(IMongoService mongoService) : base(collectionName)
        {
            var db = mongoService.GetMongoDb();
            _collection = db.GetCollection<Category>(collectionName);
            _categories = _collection;
        }

        public Category GetByName(string name) =>
            _categories.Find(category => category.Name.ToLower() == name.ToLower()).FirstOrDefault();

        public async Task<Category> GetByNameAsync(string name) =>
            await _categories.Find(category => category.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();
    }
}