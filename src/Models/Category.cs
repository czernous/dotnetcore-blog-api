using System;
using System.ComponentModel.DataAnnotations;
using api.Interfaces;
using api.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
#pragma warning disable 1591
    [BsonCollection("Categories")]
    public class Category : Document
    {
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string Name { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}