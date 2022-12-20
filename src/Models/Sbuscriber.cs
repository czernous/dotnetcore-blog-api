using System;
using System.ComponentModel.DataAnnotations;
using api.Attributes;

#pragma warning disable 1591 

namespace api.Models
{
    [BsonCollection("Subscribers")]
    public class Subscriber : Document
    {

        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string Email { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}