using api.Interfaces;
using api.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
#pragma warning disable 1591

    [BsonCollection("Posts")]
    public class Post : Document
    {

        [BsonElement("Title")]
        [BsonRequired]
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string Title { get; set; }

        public IEnumerable<Category> Categories { get; set; }

        [BsonRequired]
        [Required]
        public string ImageUrl { get; set; }

        public IEnumerable<string> ResponsiveImgs { get; set; }

        [BsonRequired]
        [Required]
        public SeoData Meta { get; set; }

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