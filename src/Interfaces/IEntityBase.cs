using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

#pragma warning disable 1591 

namespace api.Interfaces
{
    public interface IEntityBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        string Id { get; set; }
        DateTime Created { get; set; }
    }
}