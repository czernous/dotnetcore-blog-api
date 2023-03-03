using api.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591 

namespace api.Models
{
    [BsonCollection("Pages")]
    public class Page : Document
    {
        [BsonElement("pageFields")]
        [Required]

        public Dictionary<string, string> PageFields { get; set; }

        [BsonRequired]
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string Slug { get; set; }

        [BsonRequired]
        [Required]
        public SeoData Meta { get; set; }

        public string? Image { get; set; }

        [BsonDateTimeOptions]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
