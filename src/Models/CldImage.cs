using System;
using System.Collections.Generic;
using api.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
#pragma warning disable 1591
    public class CldImage : IEntityBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string PublicId { get; set; }
        public int Version { get; set; }
        public string Signature { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; }
        public string ResourceType { get; set; }
        public int Bytes { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string SecureUrl { get; set; }
        public Post UsedInPost { get; set; }
        public List<string> ResponsiveUrls { get; set; }
        public string Path { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}