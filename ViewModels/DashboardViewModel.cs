using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitnessApp.Models;
using FitnessApp.Services;

namespace FitnessApp.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly DatabaseService _db = new DatabaseService();

        // Akce pro navigaci
        public Action ProfilZadan { get; set; }
        public Action HistorieZadana { get; set; }
        public Action VolnyTreninkZadan { get; set; }
        public Action<Plan> PlanZadan { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FullGreeting))]
        private string _greeting = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FullGreeting))]
        private string _userName = "Uživateli";

        [ObservableProperty]
        private string _currentDateText = "";

        public string FullGreeting => $"{Greeting}, {UserName}!";

        // Statistiky - měsíc
        [ObservableProperty] private int _pocetTreninku;
        [ObservableProperty] private double _celkoveVolume;
        [ObservableProperty] private int _celkovyCasMinuty;

        // Sledování váhy
        [ObservableProperty] private bool _zobrazitSledovaniVahy;
        [ObservableProperty] private double _sledovaniVahyStart;
        [ObservableProperty] private double _sledovaniVahyAktualni;
        [ObservableProperty] private double _sledovaniVahyCil;
        [ObservableProperty] private double _progressPercentage;

        // Quick start + aktvita
        [ObservableProperty]
        private ObservableCollection<Plan> _dostupnePlany = new ObservableCollection<Plan>();

        [ObservableProperty]
        private ObservableCollection<ZaznamTreninku> _nedavnaAktivita = new ObservableCollection<ZaznamTreninku>();

        [ObservableProperty]
        private bool _maNedavnouAktivitu;

        public DashboardViewModel()
        {
            NastavDatumACas();
            NactiData();
        }

        public void NactiData()
        {
            var uzivatel = _db.GetUzivatel();
            UserName = string.IsNullOrWhiteSpace(uzivatel.Jmeno) ? "Uživateli" : uzivatel.Jmeno;

            // Výpočet sledování váhy
            ZobrazitSledovaniVahy = uzivatel.SledovatVahu;
            if (ZobrazitSledovaniVahy)
            {
                SledovaniVahyAktualni = uzivatel.VahaKg;
                SledovaniVahyCil = uzivatel.CilovaVahaKg;
                
                var prvniZaznam = uzivatel.HistorieVahy?.OrderBy(v => v.Datum).FirstOrDefault();
                SledovaniVahyStart = prvniZaznam != null ? prvniZaznam.Vaha : uzivatel.VahaKg;

                // Matematika pro progress (v %)
                double celkovyRozdil = Math.Abs(SledovaniVahyStart - SledovaniVahyCil);
                if (celkovyRozdil == 0) 
                {
                    ProgressPercentage = 100;
                }
                else
                {
                    double uslyRozdil = Math.Abs(SledovaniVahyStart - SledovaniVahyAktualni);
                    ProgressPercentage = (uslyRozdil / celkovyRozdil) * 100.0;
                    
                    if (ProgressPercentage < 0) ProgressPercentage = 0;
                    if (ProgressPercentage > 100) ProgressPercentage = 100;
                }
            }

            // Quick start
            DostupnePlany.Clear();
            foreach (var plan in _db.GetAllPlany())
            {
                DostupnePlany.Add(plan);
            }

            // Statistiky - měsíc
            var vsechnyZaznamy = _db.GetAllZaznamy();
            var letosniMesic = DateTime.Now.Month;
            var letosniRok = DateTime.Now.Year;

            var zaznamyTentoMesic = vsechnyZaznamy
                .Where(z => z.Datum.Month == letosniMesic && z.Datum.Year == letosniRok)
                .ToList();

            PocetTreninku = zaznamyTentoMesic.Count;
            CelkovyCasMinuty = zaznamyTentoMesic.Sum(z => z.CelkovyCasSekundy) / 60;

            double volume = 0;
            foreach (var zaznam in zaznamyTentoMesic)
            {
                foreach (var cvik in zaznam.OdcviceneCviky)
                {
                    foreach (var serie in cvik.Serie.Where(s => s.JeHotovo))
                    {
                        volume += serie.Vaha * serie.Opakovani;
                    }
                }
            }
            CelkoveVolume = volume;
            
            // Nedávná aktivita
            NedavnaAktivita.Clear();
            var posledni3 = vsechnyZaznamy.OrderByDescending(z => z.Datum).Take(3).ToList();
            foreach (var z in posledni3) NedavnaAktivita.Add(z);
            
            MaNedavnouAktivitu = NedavnaAktivita.Any();
        }

        private void NastavDatumACas()
        {
            var culture = new CultureInfo("cs-CZ");
            string datum = DateTime.Now.ToString("dddd, d. MMMM", culture);

            CurrentDateText = char.ToUpper(datum[0]) + datum.Substring(1);
            
            int hour = DateTime.Now.Hour;
            if (hour >= 5 && hour < 12) Greeting = "Dobré ráno";
            else if (hour == 12) Greeting = "Dobré poledne";
            else if (hour >= 13 && hour < 18) Greeting = "Dobré odpoledne";
            else Greeting = "Dobrý večer";
        }
        
        [RelayCommand] private void JdiNaProfil() => ProfilZadan?.Invoke();
        [RelayCommand] private void JdiNaHistorii() => HistorieZadana?.Invoke();
        [RelayCommand] private void SpustitVolnyTrenink() => VolnyTreninkZadan?.Invoke();
        [RelayCommand] private void SpustitPlan(Plan p) => PlanZadan?.Invoke(p);
    }
}