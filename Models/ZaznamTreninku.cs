using System;
using System.Collections.Generic;
using LiteDB;

namespace FitnessApp.Models
{
    public class ZaznamTreninku
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public DateTime Datum { get; set; } = DateTime.Now;
        
        public Guid PlanId { get; set; }

        public List<OdcvicenyCvik> OdcviceneCviky { get; set; } = new List<OdcvicenyCvik>();
    }

    public class OdcvicenyCvik
    {
        public Guid CvikId { get; set; }

        public List<Serie> Serie { get; set; } = new List<Serie>();
    }

    public class Serie
    {
        public int Opakovani { get; set; }
        public double Vaha  { get; set; }
    }
}