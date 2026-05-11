using System;
using System.Collections.Generic;
using LiteDB;

namespace FitnessApp.Models
{
    public class Cvik
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nazev { get; set; } = string.Empty;
        
        public List<string> Kategorie { get; set; } = new List<string>();
        
        public bool JeOblibeny { get; set; } = false;

        public string Popis { get; set; } = string.Empty;
    }
}