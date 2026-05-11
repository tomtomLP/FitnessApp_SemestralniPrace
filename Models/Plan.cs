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
        
        public string Uroven { get; set; } = "Začátečník";
        
        public List<Guid> CvikyIds { get; set; } = new List<Guid>();
        
        public bool JeVlastni { get; set; } = true;
    }
}