using api.Interfaces;
using api.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable 1591 

namespace api.Services
{
    public class PostService : DbCrudService<Post>
    {
        private readonly IMongoCollection<Post> _posts;
        private static string collectionName = "Posts";

        public PostService(IMongoService mongoService) : base(collectionName)
        {
            var db = mongoService.GetMongoDb();
            _collection = db.GetCollection<Post>(collectionName);

            _posts = _collection;
        }

        public async Task<List<Post>> GetAllByCategory(Category category)
        {
            var filter = Builders<Post>.Filter.AnyEq("Categories", category);
            return await _posts.Find(filter).ToListAsync();
        }


        public async Task<Post> GetOneByImage(CldImage image)
        {
            var filter = Builders<Post>.Filter.Eq("ImageUrl", image.Url);
            return await _posts.Find(filter).FirstOrDefaultAsync();
        }


        public async Task UpdMatchingCatAsync(Category category, Category categoryIn)
        {
            var filter = Builders<Post>.Filter.AnyEq("Categories", category);
            var update = Builders<Post>.Update.Set("Categories.$[].Name", categoryIn.Name);
            await _posts.UpdateManyAsync(filter, update);
        }

        public async Task DelMatchingCatAsync(Category category)
        {
            var filter = Builders<Post>.Filter.AnyEq("Categories", category);
            var update = Builders<Post>.Update.Pull("Categories", category);
            await _posts.UpdateManyAsync(filter, update);
        }
    }
}