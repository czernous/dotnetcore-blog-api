using api.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
#pragma warning disable 1591
    public class Post : IEntityBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Title")]
        [BsonRequired]
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string Title { get; set; }

        public List<Category> Categories { get; set; }

        [BsonRequired]
        [Required]
        public string ImageUrl { get; set; }

        public List<string> ResponsiveImgs { get; set; }

        [BsonRequired]
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string MetaDescription { get; set; }

        [BsonRequired]
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string MetaKeywords { get; set; }

        public bool isPublished { get; set; } = false;

        [BsonRequired]
        [Required]
        public string Body { get; set; }

        [BsonDateTimeOptions]
        public DateTime Created { get; set; }

        [BsonDateTimeOptions]
        public DateTime Updated { get; set; } = DateTime.UtcNow;
    }
}