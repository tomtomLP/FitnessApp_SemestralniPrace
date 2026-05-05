using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using FitnessApp.Models;

namespace FitnessApp.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString = "Filename=FitnessApp.db;Connection=shared";

        public List<Cvik> GetAllCviky()
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var col = db.GetCollection<Cvik>("cviky");
                return col.FindAll().ToList();
            }
        }

        public void SaveCvik(Cvik cvik)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var col = db.GetCollection<Cvik>("cviky");
                col.Upsert(cvik);
            }
        }

        public void DeleteCvik(Guid id)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var col = db.GetCollection<Cvik>("cviky");
                col.Delete(id);
            }
        }

        public List<Plan> GetAllPlany()
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var col = db.GetCollection<Plan>("plany");
                return col.FindAll().ToList();
            }
        }

        public void SavePlan(Plan plan)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var col = db.GetCollection<Plan>("plany");
                col.Upsert(plan);
            }
        }

        public void DeletePlan(Guid id)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var col = db.GetCollection<Plan>("plany");
                col.Delete(id);
            }
        }

        public List<ZaznamTreninku> GetAllZaznamy()
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var col = db.GetCollection<ZaznamTreninku>("zaznamy");
                return col.FindAll().OrderByDescending(z => z.Datum).ToList();
            }
        }

        public void SaveZaznam(ZaznamTreninku zaznam)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var col = db.GetCollection<ZaznamTreninku>("zaznamy");
                col.Upsert(zaznam);
            }
        }

        public void DeleteZaznam(Guid id)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var col = db.GetCollection<ZaznamTreninku>("zaznamy");
                col.Delete(id);
            }
        }

        public Uzivatel GetUzivatel()
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var col = db.GetCollection<Uzivatel>("uzivatele");
                var uzivatel = col.FindAll().FirstOrDefault();
                
                if (uzivatel == null)
                {
                    uzivatel = new Uzivatel();
                    col.Insert(uzivatel);
                }
                
                return uzivatel;
            }
        }

        public void SaveUzivatel(Uzivatel uzivatel)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                var col = db.GetCollection<Uzivatel>("uzivatele");
                col.Upsert(uzivatel);
            }
        }

        public void VymazatCelouDatabazi()
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                db.DropCollection("cviky");
                db.DropCollection("plany");
                db.DropCollection("zaznamy");
                db.DropCollection("uzivatele");
            }
        }
    }
}