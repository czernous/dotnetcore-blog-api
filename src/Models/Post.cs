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

        public string ImageUrl { get; set; }
        public IEnumerable<ResponsiveUrl> ResponsiveImgs { get; set; }

        public string ImageAltText { get; set; }

        public string BlurredImageUrl { get; set; }

        [Required]
        public string Slug { get; set; }

        [Required]
        [MaxLength(120)]
        public string ShortDescription { get; set; }

        [Required]
        public SeoData Meta { get; set; }
        public bool isPublished { get; set; } = false;

        [Required]
        public string Body { get; set; }

        [BsonDateTimeOptions]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}