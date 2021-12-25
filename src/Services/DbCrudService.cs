using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using MongoDB.Driver;

#pragma warning disable 1591 

namespace api.Services
{
    public class DbCrudService<T> where T : IEntityBase
    {
        public IMongoCollection<T> _collection;
        public IMongoService mongoService;

        public DbCrudService(string collectionName = "")
        {

        }

        public List<T> GetAll() =>
            _collection.Find(item => true).ToList();

        public async Task<List<T>> GetAllAsync() =>
            await _collection.Find(item => true).ToListAsync();


        public T GetById(string id) =>
            _collection.Find(item => item.Id == id).FirstOrDefault();

        public async Task<T> GetByIdAsync(string id) =>
            await _collection.Find(item => item.Id == id).FirstOrDefaultAsync();

        public async Task<T> CreateAsync(T item)
        {
            item.Created = DateTime.UtcNow;
            await _collection.InsertOneAsync(item);
            return item;
        }

        public async Task UpdateAsync(string id, T itemIn) =>
            await _collection.ReplaceOneAsync(item => item.Id == id, itemIn);

        public void Update(string id, T itemIn) =>
            _collection.ReplaceOne(item => item.Id == id, itemIn);

        public void Remove(string id, T itemIn) =>
           _collection.DeleteOne(item => item.Id == itemIn.Id);

        public async Task RemoveAsync(string id) =>
           await _collection.DeleteOneAsync(item => item.Id == id);

    }
}
