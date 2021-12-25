using System;
using System.ComponentModel.DataAnnotations;
using api.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

#pragma warning disable 1591 

namespace api.Models
{
    public class Subscriber : IEntityBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"(?s)^((?!<)(?!>).)*$", ErrorMessage = "This field cannot contain HTML tags")]
        public string Email { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}