using System;
using System.Collections.Generic;
using LiteDB;

namespace FitnessApp.Models
{
    public class ZaznamTreninku
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Nazev { get; set; } = "Freestyle Workout";
        
        public DateTime Datum { get; set; } = DateTime.Now;
        
        public Guid? PlanId { get; set; }

        public int CelkovyCasSekundy { get; set; } = 0;

        public List<OdcvicenyCvik> OdcviceneCviky { get; set; } = new List<OdcvicenyCvik>();
    }

    public class OdcvicenyCvik
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 
        
        public Guid CvikId { get; set; }
        
        public string CvikNazev { get; set; } = string.Empty;

        public List<Serie> Serie { get; set; } = new List<Serie>();
    }

    public class Serie
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Poradi { get; set; } = 1;
        
        public double Vaha { get; set; } = 0;
        
        public int Opakovani { get; set; } = 0;
        
        public bool JeWarmup { get; set; } = false; 
        
        public bool JeHotovo { get; set; } = false;
    }
}