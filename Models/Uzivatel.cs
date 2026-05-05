using System;
using System.Collections.Generic;
using LiteDB;

namespace FitnessApp.Models
{
    public class ZaznamVahy
    {
        public DateTime Datum { get; set; }
        public double Vaha { get; set; }
    }

    public class Uzivatel
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Jmeno { get; set; } = "";
        public string Pohlavi { get; set; } = "Muž";
        public DateTime DatumNarozeni { get; set; } = new DateTime(2000, 1, 1);
        
        public double VyskaCm { get; set; } = 0;
        public double VahaKg { get; set; } = 0;
        
        public List<ZaznamVahy> HistorieVahy { get; set; } = new List<ZaznamVahy>();
        
        public bool SledovatVahu { get; set; } = false;
        public double CilovaVahaKg { get; set; } = 0;

        [BsonIgnore]
        public double BMI 
        { 
            get 
            {
                if (VyskaCm <= 0 || VahaKg <= 0) return 0;
                double vyskaMetry = VyskaCm / 100.0;
                return Math.Round(VahaKg / (vyskaMetry * vyskaMetry), 1);
            }
        }

        [BsonIgnore]
        public int Vek
        {
            get
            {
                var dnes = DateTime.Today;
                var vek = dnes.Year - DatumNarozeni.Year;
                if (DatumNarozeni.Date > dnes.AddYears(-vek)) vek--;
                return Math.Max(0, vek);
            }
        }
    }
}