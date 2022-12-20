using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591 

namespace api.Models
{
    public class SeoData
    {
        [BsonRequired]
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        [StringLength(160, ErrorMessage = "Wrong string length. Meta description must be 160 characters or less")]
        public string MetaDescription { get; set; }

        [BsonRequired]
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string MetaKeywords { get; set; }

        [BsonRequired]
        [Required]
        public OpenGraph OpenGraph { get; set; }
    }
}