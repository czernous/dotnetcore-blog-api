using System;
using System.ComponentModel.DataAnnotations;
using api.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
#pragma warning disable 1591
    public class Category : IEntityBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string Name { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}