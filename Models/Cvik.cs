using System;
using LiteDB;

namespace FitnessApp.Models
{
    public class Cvik
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nazev { get; set; } = string.Empty;
        
        public string Kategorie { get; set; } = string.Empty;
        
        public bool JeOblibeny { get; set; } = false;
    }
}