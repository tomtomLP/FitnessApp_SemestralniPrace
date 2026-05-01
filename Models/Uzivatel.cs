using System;
using LiteDB;

namespace FitnessApp.Models
{
    public class Uzivatel
    {
        [BsonId]
        public Guid Id { get; set; } =  Guid.NewGuid();
        
        public double VahaKg  { get; set; }
        public double VyskaCm { get; set; }

        [BsonIgnore]
        public double BMI
        {
            get
            {
                if (VyskaCm <= 0) return 0;
                double vyskaMetry = VyskaCm / 100.0;
                return Math.Round(VahaKg / (vyskaMetry * vyskaMetry), 2);
            }
        }
    }
}