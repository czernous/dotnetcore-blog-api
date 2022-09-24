using System;
using System.Collections.Generic;
using api.Interfaces;
using api.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
#pragma warning disable 1591

    [BsonCollection("Images")]
    public class CldImage : Document
    {
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
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}