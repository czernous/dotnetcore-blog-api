using System;
using System.ComponentModel.DataAnnotations;
using api.Attributes;

namespace api.Models
{
#pragma warning disable 1591
    [BsonCollection("Categories")]
    public class Category : Document
    {
        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string Name { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}