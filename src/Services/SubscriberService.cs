using System.Threading.Tasks;
using api.Interfaces;
using api.Models;
using MongoDB.Driver;

#pragma warning disable 1591 

namespace api.Services
{
    public class SubscriberService : DbCrudService<Subscriber>
    {
        private readonly IMongoCollection<Subscriber> _subscribers;
        private static string collectionName = "Subscribers";
        public SubscriberService(IMongoService mongoService) : base(collectionName)
        {
            var db = mongoService.GetMongoDb();
            _collection = db.GetCollection<Subscriber>(collectionName);
            _subscribers = _collection;
        }

        public Subscriber GetByEmail(string email) =>
    _subscribers.Find(subscriber => subscriber.Email.ToLower() == email.ToLower()).FirstOrDefault();

        public async Task<Subscriber> GetByEmailAsync(string email) =>
            await _subscribers.Find(subscriber => subscriber.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
    }
}