using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591 

namespace api.Models
{
    public class OpenGraph
    {
        [BsonRequired]
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string Title { get; set; }

        [BsonRequired]
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string Description { get; set; }

        [BsonRequired]
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
    }
}