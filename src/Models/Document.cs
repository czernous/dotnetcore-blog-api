using System;
using api.Interfaces;
using MongoDB.Bson;

#pragma warning disable 1591

namespace api.Models
{
    public class Document : IDocument
    {
        public ObjectId Id { get; set; }

        public DateTime CreatedAt => Id.CreationTime;

        public string _id => Id.ToString();
    }
}