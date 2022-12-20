using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

#pragma warning disable 1591

namespace api.Interfaces
{
    public interface IDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        ObjectId Id { get; set; }

        DateTime CreatedAt { get; }

        string _id { get; }
    }
}