using MongoDB.Driver;

#pragma warning disable 1591 

namespace api.Interfaces
{
    public interface IMongoService
    {
        MongoClient GetMongoClient();
        IMongoDatabase GetMongoDb();
    }
}