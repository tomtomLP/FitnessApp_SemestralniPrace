using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitnessApp.Models;
using FitnessApp.Services;

namespace FitnessApp.ViewModels
{
    public partial class HistoryViewModel : ViewModelBase
    {
        private readonly DatabaseService _db = new DatabaseService();

        [ObservableProperty]
        private ObservableCollection<ZaznamTreninku> _historieTreninku = new ObservableCollection<ZaznamTreninku>();

        // Mazání tréninku - stavy
        [ObservableProperty]
        private bool _isDeleteDialogVisible;

        [ObservableProperty]
        private ZaznamTreninku _zaznamKeSmazani;

        // Editace tréninku - stavy
        [ObservableProperty] private bool _isEditDialogVisible;
        [ObservableProperty] private ZaznamTreninku _zaznamVEditaci;
        
        [ObservableProperty] private string _editNazev;
        [ObservableProperty] private DateTimeOffset _editDatum;
        [ObservableProperty] private int _editHodiny;
        [ObservableProperty] private int _editMinuty;
        [ObservableProperty] private int _editSekundy;
        
        // Seznam cviků pro editaci
        public ObservableCollection<WorkoutCvikWrapper> EditCviky { get; } = new ObservableCollection<WorkoutCvikWrapper>();

        // Přidání nového cviku v editaci
        [ObservableProperty] private bool _isAddExerciseDialogVisible;
        [ObservableProperty] private string _hledanyText = string.Empty;
        [ObservableProperty] private string _vybranaKategorie = "All";
        [ObservableProperty] private ObservableCollection<Cvik> _zobrazeneCvikyKVyberu = new ObservableCollection<Cvik>();
        
        public ObservableCollection<string> KategorieList { get; } = new ObservableCollection<string>
        {
            "All", "Chest", "Back", "Arms", "Shoulders", "Legs", "Glutes", "Core", "Cardio", "Full Body", "Other"
        };
        private List<Cvik> _vsechnyCvikyDB = new List<Cvik>();


        public HistoryViewModel()
        {
            _vsechnyCvikyDB = _db.GetAllCviky().OrderByDescending(c => c.JeOblibeny).ThenBy(c => c.Nazev).ToList();
            NactiHistorii();
        }

        public void NactiHistorii()
        {
            HistorieTreninku.Clear();
            var zaznamyZDb = _db.GetAllZaznamy().OrderByDescending(z => z.Datum).ToList();
            
            foreach (var zaznam in zaznamyZDb)
            {
                HistorieTreninku.Add(zaznam);
            }
        }

        // Editace jako aktivní trénink
        [RelayCommand]
        private void OtevritEditDialog(ZaznamTreninku zaznam)
        {
            if (zaznam == null) return;
            
            ZaznamVEditaci = zaznam;
            EditNazev = zaznam.Nazev;
            EditDatum = new DateTimeOffset(zaznam.Datum);
            
            int sec = zaznam.CelkovyCasSekundy;
            EditHodiny = sec / 3600;
            EditMinuty = (sec % 3600) / 60;
            EditSekundy = sec % 60;

            // Načtení cviků do upravitelných wrapperů
            EditCviky.Clear();
            foreach (var odcviceny in zaznam.OdcviceneCviky)
            {
                var cw = new WorkoutCvikWrapper { CvikId = odcviceny.CvikId, CvikNazev = odcviceny.CvikNazev };
                
                foreach (var serie in odcviceny.Serie.OrderBy(s => s.Poradi))
                {
                    cw.Serie.Add(new WorkoutSerieWrapper
                    {
                        Poradi = serie.Poradi,
                        Vaha = serie.Vaha,
                        Opakovani = serie.Opakovani,
                        JeWarmup = serie.JeWarmup,
                        JeHotovo = serie.JeHotovo
                    });
                }
                EditCviky.Add(cw);
            }

            IsEditDialogVisible = true;
        }

        [RelayCommand]
        private void OdebratCvikZTreninku(WorkoutCvikWrapper cvik)
        {
            if (cvik != null) EditCviky.Remove(cvik);
        }

        [RelayCommand]
        private void UlozitEditaci()
        {
            if (ZaznamVEditaci == null) return;

            // Aktualizace základních údajů
            ZaznamVEditaci.Nazev = EditNazev;
            ZaznamVEditaci.Datum = EditDatum.DateTime;
            ZaznamVEditaci.CelkovyCasSekundy = (EditHodiny * 3600) + (EditMinuty * 60) + EditSekundy;

            // Přepsání cviků z Edit UI do databázového modelu
            ZaznamVEditaci.OdcviceneCviky.Clear();
            foreach (var cw in EditCviky)
            {
                var oc = new OdcvicenyCvik { CvikId = cw.CvikId, CvikNazev = cw.CvikNazev };
                foreach (var s in cw.Serie)
                {
                    oc.Serie.Add(new Serie
                    {
                        Poradi = s.Poradi,
                        Vaha = s.Vaha ?? 0,
                        Opakovani = s.Opakovani ?? 0,
                        JeWarmup = s.JeWarmup,
                        JeHotovo = s.JeHotovo 
                    });
                }
                ZaznamVEditaci.OdcviceneCviky.Add(oc);
            }

            // Uložení a obnova UI
            _db.SaveZaznam(ZaznamVEditaci);
            NactiHistorii();
            IsEditDialogVisible = false;
        }

        [RelayCommand]
        private void ZrusitEditaci()
        {
            IsEditDialogVisible = false;
        }

        [RelayCommand]
        private void OtevritPridatCvikDialog()
        {
            HledanyText = "";
            VybranaKategorie = "All";
            AplikujFiltry();
            IsAddExerciseDialogVisible = true;
        }

        partial void OnHledanyTextChanged(string value) => AplikujFiltry();
        partial void OnVybranaKategorieChanged(string value) => AplikujFiltry();

        private void AplikujFiltry()
        {
            var vyfiltrovano = _vsechnyCvikyDB.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(HledanyText))
                vyfiltrovano = vyfiltrovano.Where(c => c.Nazev.Contains(HledanyText, StringComparison.OrdinalIgnoreCase));

            if (VybranaKategorie != "All")
                vyfiltrovano = vyfiltrovano.Where(c => c.Kategorie.Any(k => k.Equals(VybranaKategorie, StringComparison.OrdinalIgnoreCase)));

            ZobrazeneCvikyKVyberu.Clear();
            foreach (var cvik in vyfiltrovano) ZobrazeneCvikyKVyberu.Add(cvik);
        }

        [RelayCommand]
        private void ZmenKategorii(string kategorie) => VybranaKategorie = kategorie;

        [RelayCommand]
        private void VybratCvik(Cvik cvik)
        {
            if (cvik == null) return;

            var novyCvik = new WorkoutCvikWrapper { CvikId = cvik.Id, CvikNazev = cvik.Nazev };
            novyCvik.Serie.Add(new WorkoutSerieWrapper { Poradi = 1 });

            EditCviky.Add(novyCvik);
            IsAddExerciseDialogVisible = false;
        }

        [RelayCommand]
        private void ZavritPridatCvikDialog() => IsAddExerciseDialogVisible = false;
        
        // Dialog pro mazání
        [RelayCommand]
        private void OtevritSmazatDialog(ZaznamTreninku zaznam)
        {
            if (zaznam == null) return;
            ZaznamKeSmazani = zaznam;
            IsDeleteDialogVisible = true;
        }

        [RelayCommand]
        private void PotvrditSmazani()
        {
            if (ZaznamKeSmazani != null)
            {
                _db.DeleteZaznam(ZaznamKeSmazani.Id);
                HistorieTreninku.Remove(ZaznamKeSmazani);
            }
            IsDeleteDialogVisible = false;
        }

        [RelayCommand]
        private void ZrusitSmazani()
        {
            IsDeleteDialogVisible = false;
        }
    }
}