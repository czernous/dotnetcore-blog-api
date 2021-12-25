using System.Threading.Tasks;
using api.Interfaces;
using api.Models;
using MongoDB.Driver;

#pragma warning disable 1591

namespace api.Services
{
    public class ImageService : DbCrudService<CldImage>
    {
        private readonly IMongoCollection<CldImage> _images;
        private static string collectionName = "Images";
        public ImageService(IMongoService mongoService) : base(collectionName)
        {
            var db = mongoService.GetMongoDb();
            _collection = db.GetCollection<CldImage>(collectionName);
            _images = _collection;
        }

        public CldImage GetByUrl(string url) =>
            _images.Find(cldImage => cldImage.Url == url).FirstOrDefault();

        public async Task<CldImage> GetByUrlAsync(string url) =>
            await _images.Find(cldImage => cldImage.Url == url).FirstOrDefaultAsync();

        public CldImage GetByName(string name) =>
    _images.Find(cldImage => cldImage.Name == name).FirstOrDefault();

        public async Task<CldImage> GetByNameAsync(string name) =>
            await _images.Find(cldImage => cldImage.Name == name).FirstOrDefaultAsync();
    }

}
