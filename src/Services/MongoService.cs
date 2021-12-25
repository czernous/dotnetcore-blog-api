using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using api.Models;
using api.Interfaces;

#pragma warning disable 1591 

namespace api.Services
{
    public class MongoService : IMongoService
    {

        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDb;
        public MongoService(IBlogDatabaseSettings settings)
        {
            _mongoClient = new MongoClient(settings.ConnectionString);
            _mongoDb = _mongoClient.GetDatabase(settings.DatabaseName);
        }

        public MongoClient GetMongoClient() => _mongoClient;
        public IMongoDatabase GetMongoDb() => _mongoDb;
    }
}