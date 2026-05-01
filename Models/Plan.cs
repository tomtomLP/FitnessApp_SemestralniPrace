using System;
using System.Collections.Generic;
using LiteDB;

namespace FitnessApp.Models
{
    public class Plan
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Nazev { get; set; } = string.Empty;

        public List<Guid> CvikyIds { get; set; } = new List<Guid>();
    }
}