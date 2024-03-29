using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using api.Interfaces;
using api.Attributes;
using MongoDB.Driver;
using MongoDB.Bson;
using api.Models;


#pragma warning disable 1591

namespace api.Db
{

    public class MongoRepository<TDocument> : IMongoRepository<TDocument>
        where TDocument : IDocument
    {
        private readonly IMongoCollection<TDocument> _collection;
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDb;

        public MongoRepository(IBlogDatabaseSettings settings)
        {
            _mongoClient = new MongoClient(settings.ConnectionString);
            _mongoDb = _mongoClient.GetDatabase(settings.DatabaseName);

            _collection = _mongoDb.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        }

        private protected string GetCollectionName(Type documentType)
        {
            return ((BsonCollectionAttribute)documentType.GetCustomAttributes(
                    typeof(BsonCollectionAttribute),
                    true)
                .FirstOrDefault())?.CollectionName;
        }

        public virtual IQueryable<TDocument> AsQueryable()
        {
            return _collection.AsQueryable();
        }

        public virtual IEnumerable<TDocument> FilterBy(
            Expression<Func<TDocument, bool>> filterExpression)
        {
            return _collection.Find(filterExpression).ToEnumerable();
        }

        public virtual async Task<IEnumerable<TDocument>> FilterByAsync(
            Expression<Func<TDocument, bool>> filterExpression)
        {
            return await _collection.Find(filterExpression).ToListAsync();
        }

        public virtual IEnumerable<TProjected> FilterBy<TProjected>(
            Expression<Func<TDocument, bool>> filterExpression,
            Expression<Func<TDocument, TProjected>> projectionExpression)
        {
            return _collection.Find(filterExpression).Project(projectionExpression).ToEnumerable();
        }

        public virtual async Task<IEnumerable<TProjected>> FilterByAsync<TProjected>(
            Expression<Func<TDocument, bool>> filterExpression,
            Expression<Func<TDocument, TProjected>> projectionExpression)
        {
            return await _collection.Find(filterExpression).Project(projectionExpression).ToListAsync();
        }
        public virtual async Task<PagedData<TDocument>> FilterByAndPaginateAsync(Expression<Func<TDocument, bool>> filterExpression, SortDefinition<TDocument> sortDefinition, int? page, int? pageSize)
        {


            bool hasPagination = page != null && pageSize != null;

            var totalDocuments = await _collection.CountDocumentsAsync(filterExpression);

            var totalPages = hasPagination ? (int)Math.Ceiling((double)totalDocuments / (int)pageSize) : 0;


            // Skip a certain number of documents based on the page number and page size
            var documents = hasPagination
                ? await _collection
                    .Find(filterExpression)
                    .Sort(sortDefinition)
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync()
                : _collection
                    .Find(filterExpression)
                    .Sort(sortDefinition)
                    .ToEnumerable();

            // Return the paginated results and additional information in the response

            var result = new PagedData<TDocument>
            {
                Data = documents,
                HasPagination = hasPagination,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalDocuments = totalDocuments
            };

            return result;
        }


        public virtual TDocument FindOne(FilterDefinition<TDocument> filterExpression)
        {
            return _collection.Find(filterExpression).FirstOrDefault();
        }

        public virtual async Task<TDocument> FindOneAsync(FilterDefinition<TDocument> filterExpression)
        {
            return await _collection.Find(filterExpression).FirstOrDefaultAsync();
        }

        public virtual TDocument FindById(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
            return _collection.Find(filter).SingleOrDefault();
        }

        public virtual async Task<TDocument> FindByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
            return await _collection.Find(filter).SingleOrDefaultAsync();
        }


        public virtual void InsertOne(TDocument document)
        {
            _collection.InsertOne(document);
        }

        public virtual async Task InsertOneAsync(TDocument document)
        {
            await _collection.InsertOneAsync(document);
        }

        public void InsertMany(ICollection<TDocument> documents)
        {
            _collection.InsertMany(documents);
        }


        public virtual async Task InsertManyAsync(ICollection<TDocument> documents)
        {
            await _collection.InsertManyAsync(documents);
        }

        public void ReplaceOne(TDocument document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            _collection.FindOneAndReplace(filter, document);
        }

        public virtual async Task ReplaceOneAsync(TDocument document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            await _collection.FindOneAndReplaceAsync(filter, document);
        }

        public async Task ReplaceManyAsync(FilterDefinition<TDocument> filterExpression, UpdateDefinition<TDocument> updateExpression) =>
            await _collection.UpdateManyAsync(filterExpression, updateExpression);


        public void DeleteOne(Expression<Func<TDocument, bool>> filterExpression)
        {
            _collection.FindOneAndDelete(filterExpression);
        }

        public async Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            await _collection.FindOneAndDeleteAsync(filterExpression);
        }

        public void DeleteById(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
            _collection.FindOneAndDelete(filter);
        }

        public async Task DeleteByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
            await _collection.FindOneAndDeleteAsync(filter);
        }

        public void DeleteMany(Expression<Func<TDocument, bool>> filterExpression)
        {
            _collection.DeleteMany(filterExpression);
        }

        public Task DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            return Task.Run(() => _collection.DeleteManyAsync(filterExpression));
        }
    }
}