using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using FitnessApp.Models;

namespace FitnessApp.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath = "FitnessApp.db";
        
        // Kolekce
        private ILiteCollection<Cvik> Cviky => GetDb().GetCollection<Cvik>("cviky");
        private ILiteCollection<Plan> Plany => GetDb().GetCollection<Plan>("plany");
        private ILiteCollection<ZaznamTreninku> Zaznamy => GetDb().GetCollection<ZaznamTreninku>("zaznamy");
        private ILiteCollection<Uzivatel> Uzivatele => GetDb().GetCollection<Uzivatel>("uzivatele");
        
        private LiteDatabase GetDb()
        {
            return new LiteDatabase(_dbPath);
        }
        
        // Cviky
        public List<Cvik> GetAllCviky()
        {
            return Cviky.FindAll().ToList();
        }

        public void SaveCvik(Cvik cvik)
        {
            Cviky.Upsert(cvik);
        }

        public void DeleteCvik(Guid id)
        {
            Cviky.Delete(id);
        }
        
        // PLány
        public List<Plan> GetAllPlany()
        {
            return Plany.FindAll().ToList();
        }

        public void SavePlan(Plan plan)
        {
            Plany.Upsert(plan);
        }

        public void DeletePlan(Guid id)
        {
            Plany.Delete(id);
        }
        
        // Záznamy
        public List<ZaznamTreninku> GetAllZaznamy()
        {
            return Zaznamy.FindAll().OrderByDescending(z => z.Datum).ToList();
        }

        public void SaveZaznam(ZaznamTreninku zaznam)
        {
            Zaznamy.Upsert(zaznam);
        }

        public void DeleteZaznam(Guid id)
        {
            Zaznamy.Delete(id);
        }
        
        // Uživatelé
        public Uzivatel GetUzivatel()
        {
            return Uzivatele.FindAll().FirstOrDefault() ?? new Uzivatel();
        }

        public void SaveUzivatel(Uzivatel uzivatel)
        {
            Uzivatele.Upsert(uzivatel);
        }

        // Smazání všeho
        public void VymazatCelouDatabazi()
        {
            var db = GetDb();
            db.DropCollection("cviky");
            db.DropCollection("plany");
            db.DropCollection("zaznamy");
            db.DropCollection("uzivatele");
        }
    }
}