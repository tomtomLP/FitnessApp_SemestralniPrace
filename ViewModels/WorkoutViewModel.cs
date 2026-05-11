using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitnessApp.Models;
using FitnessApp.Services;

namespace FitnessApp.ViewModels
{
    public partial class WorkoutSerieWrapper : ObservableObject
    {
        [ObservableProperty] private int _poradi;
        [ObservableProperty] private double? _vaha;
        [ObservableProperty] private int? _opakovani;
        [ObservableProperty] private bool _jeWarmup;
        [ObservableProperty] private bool _jeHotovo;

        partial void OnVahaChanged(double? value)
        {
            if (value < 0) Vaha = 0;
            if (value > 999) Vaha = 999;
        }

        partial void OnOpakovaniChanged(int? value)
        {
            if (value < 0) Opakovani = 0;
            if (value > 999) Opakovani = 999;
        }
    }

    public partial class WorkoutCvikWrapper : ObservableObject
    {
        public Guid CvikId { get; set; }
        public string CvikNazev { get; set; } = string.Empty;

        public ObservableCollection<WorkoutSerieWrapper> Serie { get; } = new ObservableCollection<WorkoutSerieWrapper>();

        [RelayCommand]
        private void PridatSerii()
        {
            var predchozi = Serie.LastOrDefault();
            var novaSerie = new WorkoutSerieWrapper
            {
                Poradi = Serie.Count + 1,
                Vaha = predchozi?.Vaha,
                Opakovani = predchozi?.Opakovani,
                JeWarmup = false,
                JeHotovo = false
            };
            Serie.Add(novaSerie);
        }

        [RelayCommand]
        private void OdebratSerii(WorkoutSerieWrapper s)
        {
            if (s != null && Serie.Contains(s))
            {
                Serie.Remove(s);
                for (int i = 0; i < Serie.Count; i++)
                {
                    Serie[i].Poradi = i + 1;
                }
            }
        }

        [RelayCommand]
        private void PrepnoutWarmup(WorkoutSerieWrapper s)
        {
            if (s != null) s.JeWarmup = !s.JeWarmup;
        }

        [RelayCommand]
        private void PrepnoutHotovo(WorkoutSerieWrapper s)
        {
            if (s != null) s.JeHotovo = !s.JeHotovo;
        }
    }

    
    public partial class WorkoutViewModel : ViewModelBase
    {
        private readonly DatabaseService _db = new DatabaseService();
        private DispatcherTimer _timer;

        // Stav pro udržení aktivního tréninku
        private static DateTime? s_startTime;
        private static ObservableCollection<WorkoutCvikWrapper> s_cvikyVTréninku = new ObservableCollection<WorkoutCvikWrapper>();
        private static string s_treninkNazev = "Volný trénink";
        private static bool s_isWorkoutActive = false;

        private static DateTime? s_restEndTime;
        private static bool s_isRestTimerActive = false;

        // Přepínání obrazovek
        public bool IsSelectionScreenVisible => !s_isWorkoutActive;
        public bool IsWorkoutScreenVisible => s_isWorkoutActive;

        [ObservableProperty]
        private ObservableCollection<Plan> _dostupnePlany = new ObservableCollection<Plan>();

        public string TreninkNazev
        {
            get => s_treninkNazev;
            set
            {
                if (s_treninkNazev != value)
                {
                    s_treninkNazev = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<WorkoutCvikWrapper> CvikyVTréninku => s_cvikyVTréninku;

        public string StopkyDisplay
        {
            get
            {
                if (!s_startTime.HasValue) return "00:00";
                var duration = DateTime.Now - s_startTime.Value;
                int hodiny = (int)duration.TotalHours;
                int minuty = duration.Minutes;
                int sekundy = duration.Seconds;

                if (hodiny > 0)
                    return $"{hodiny}:{minuty:D2}:{sekundy:D2}";
                
                return $"{minuty:D2}:{sekundy:D2}";
            }
        }

        public bool IsRestTimerVisible => s_isRestTimerActive;
        
        public string RestStopkyDisplay
        {
            get
            {
                if (!s_isRestTimerActive || !s_restEndTime.HasValue) return "";
                var remaining = s_restEndTime.Value - DateTime.Now;
                if (remaining.TotalSeconds <= 0) return "00:00";
                
                return $"{(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2}";
            }
        }

        // Stavy pro dialogy
        [ObservableProperty] private bool _isAddExerciseDialogVisible;
        [ObservableProperty] private string _hledanyText = string.Empty;
        [ObservableProperty] private string _vybranaKategorie = "All";
        [ObservableProperty] private ObservableCollection<Cvik> _zobrazeneCvikyKVyberu = new ObservableCollection<Cvik>();
        
        [ObservableProperty] private bool _isFinishConfirmationVisible;
        [ObservableProperty] private bool _isWorkoutFinishedDialogVisible;
        
        // Dialog zrušení tréninku
        [ObservableProperty] private bool _isCancelConfirmationVisible;

        [ObservableProperty] private bool _isDetailDialogVisible;
        [ObservableProperty] private Cvik _vybranyCvikDetail;

        public ObservableCollection<string> KategorieList { get; } = new ObservableCollection<string>
        {
            "All", "Chest", "Back", "Arms", "Shoulders", "Legs", "Glutes", "Core", "Cardio", "Full Body", "Other"
        };

        private List<Cvik> _vsechnyCvikyDB = new List<Cvik>();


        public WorkoutViewModel()
        {
            NactiPlany();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) =>
            {
                if (s_isWorkoutActive)
                {
                    OnPropertyChanged(nameof(StopkyDisplay));

                    if (s_isRestTimerActive && s_restEndTime.HasValue)
                    {
                        var remaining = s_restEndTime.Value - DateTime.Now;
                        if (remaining.TotalSeconds <= 0)
                        {
                            s_isRestTimerActive = false;
                            OnPropertyChanged(nameof(IsRestTimerVisible));
                        }
                        OnPropertyChanged(nameof(RestStopkyDisplay));
                    }
                }
            };
            _timer.Start();

            _vsechnyCvikyDB = _db.GetAllCviky().OrderByDescending(c => c.JeOblibeny).ThenBy(c => c.Nazev).ToList();
        }

        public void NactiPlany()
        {
            DostupnePlany.Clear();
            var planyZDb = _db.GetAllPlany();
            foreach (var plan in planyZDb)
            {
                DostupnePlany.Add(plan);
            }
        }

        // Spuštění tréninku
        [RelayCommand]
        private void SpustitVolnyTrenink()
        {
            ZahajitTrenink("Volný trénink");
        }

        [RelayCommand]
        private void SpustitPlan(Plan plan)
        {
            if (plan == null) return;
            ZahajitTrenink(plan.Nazev);

            foreach (var cvikId in plan.CvikyIds)
            {
                var cvikDb = _vsechnyCvikyDB.FirstOrDefault(c => c.Id == cvikId);
                if (cvikDb != null)
                {
                    var novyCvik = new WorkoutCvikWrapper { CvikId = cvikDb.Id, CvikNazev = cvikDb.Nazev };
                    NacistHistoriiCvik(novyCvik);
                    CvikyVTréninku.Add(novyCvik);
                }
            }
        }

        private void ZahajitTrenink(string nazev)
        {
            s_isWorkoutActive = true;
            s_startTime = DateTime.Now;
            s_cvikyVTréninku.Clear();
            s_treninkNazev = nazev;
            s_isRestTimerActive = false;

            OnPropertyChanged(nameof(IsSelectionScreenVisible));
            OnPropertyChanged(nameof(IsWorkoutScreenVisible));
            OnPropertyChanged(nameof(TreninkNazev));
            OnPropertyChanged(nameof(StopkyDisplay));
            OnPropertyChanged(nameof(IsRestTimerVisible));
        }

        private void NacistHistoriiCvik(WorkoutCvikWrapper novyCvik)
        {
            var vsechnyZaznamy = _db.GetAllZaznamy();
            
            var posledniZaznam = vsechnyZaznamy
                .Where(z => z.OdcviceneCviky.Any(c => c.CvikId == novyCvik.CvikId))
                .OrderByDescending(z => z.Datum)
                .FirstOrDefault();

            if (posledniZaznam != null)
            {
                var staryCvik = posledniZaznam.OdcviceneCviky.First(c => c.CvikId == novyCvik.CvikId);
                foreach (var staraSerie in staryCvik.Serie.OrderBy(s => s.Poradi))
                {
                    novyCvik.Serie.Add(new WorkoutSerieWrapper
                    {
                        Poradi = staraSerie.Poradi,
                        Vaha = staraSerie.Vaha,
                        Opakovani = staraSerie.Opakovani,
                        JeWarmup = staraSerie.JeWarmup,
                        JeHotovo = false 
                    });
                }
            }
            else
            {
                novyCvik.Serie.Add(new WorkoutSerieWrapper { Poradi = 1 });
            }
        }
        

        [RelayCommand]
        private void OdebratCvikZTreninku(WorkoutCvikWrapper cvik)
        {
            if (cvik != null)
            {
                CvikyVTréninku.Remove(cvik);
            }
        }

        [RelayCommand]
        private void SpustitRestTimer()
        {
            if (s_isRestTimerActive)
            {
                s_isRestTimerActive = false;
            }
            else
            {
                var uzivatel = _db.GetUzivatel();
                int pauzaSekundy = uzivatel?.RestTimerSekundy > 0 ? uzivatel.RestTimerSekundy : 180;

                s_restEndTime = DateTime.Now.AddSeconds(pauzaSekundy);
                s_isRestTimerActive = true;
            }
            OnPropertyChanged(nameof(IsRestTimerVisible));
            OnPropertyChanged(nameof(RestStopkyDisplay));
        }

        [RelayCommand]
        private void DokoncitTrenink()
        {
            IsFinishConfirmationVisible = true;
        }

        [RelayCommand]
        private void ZrusitDokonceni()
        {
            IsFinishConfirmationVisible = false;
        }

        [RelayCommand]
        private void PotvrditDokonceni()
        {
            IsFinishConfirmationVisible = false;

            var zaznam = new ZaznamTreninku
            {
                Nazev = TreninkNazev,
                CelkovyCasSekundy = s_startTime.HasValue ? (int)(DateTime.Now - s_startTime.Value).TotalSeconds : 0,
                Datum = DateTime.Now
            };

            foreach (var cvikWrapper in CvikyVTréninku)
            {
                var odcvicenyCvik = new OdcvicenyCvik
                {
                    CvikId = cvikWrapper.CvikId,
                    CvikNazev = cvikWrapper.CvikNazev
                };

                foreach (var serieWrapper in cvikWrapper.Serie)
                {
                    odcvicenyCvik.Serie.Add(new Serie
                    {
                        Poradi = serieWrapper.Poradi,
                        Vaha = serieWrapper.Vaha ?? 0,
                        Opakovani = serieWrapper.Opakovani ?? 0,
                        JeWarmup = serieWrapper.JeWarmup,
                        JeHotovo = serieWrapper.JeHotovo 
                    });
                }
                zaznam.OdcviceneCviky.Add(odcvicenyCvik);
            }

            _db.SaveZaznam(zaznam);
            
            s_isWorkoutActive = false;
            s_cvikyVTréninku.Clear();
            s_isRestTimerActive = false;
            s_startTime = null;

            OnPropertyChanged(nameof(IsSelectionScreenVisible));
            OnPropertyChanged(nameof(IsWorkoutScreenVisible));

            IsWorkoutFinishedDialogVisible = true;
        }


        // Ukončení bez uložení
        [RelayCommand]
        private void OtevritZrusitTreninkDialog()
        {
            IsCancelConfirmationVisible = true;
        }

        [RelayCommand]
        private void ZavritZrusitTreninkDialog()
        {
            IsCancelConfirmationVisible = false;
        }

        [RelayCommand]
        private void PotvrditZruseniTreninku()
        {
            IsCancelConfirmationVisible = false;
            
            s_isWorkoutActive = false;
            s_cvikyVTréninku.Clear();
            s_isRestTimerActive = false;
            s_startTime = null;
            
            OnPropertyChanged(nameof(IsSelectionScreenVisible));
            OnPropertyChanged(nameof(IsWorkoutScreenVisible));
        }
        

        // Dialog výběr cviku
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
            foreach (var cvik in vyfiltrovano)
            {
                ZobrazeneCvikyKVyberu.Add(cvik);
            }
        }

        [RelayCommand]
        private void ZmenKategorii(string kategorie)
        {
            VybranaKategorie = kategorie;
        }

        [RelayCommand]
        private void VybratCvik(Cvik cvik)
        {
            if (cvik == null) return;

            var novyCvik = new WorkoutCvikWrapper
            {
                CvikId = cvik.Id,
                CvikNazev = cvik.Nazev
            };

            NacistHistoriiCvik(novyCvik);

            CvikyVTréninku.Add(novyCvik);
            IsAddExerciseDialogVisible = false;
        }

        [RelayCommand]
        private void ZavritPridatCvikDialog()
        {
            IsAddExerciseDialogVisible = false;
        }

        [RelayCommand]
        private void ZobrazitDetail(WorkoutCvikWrapper cvikWrapper)
        {
            if (cvikWrapper == null) return;
            VybranyCvikDetail = _vsechnyCvikyDB.FirstOrDefault(c => c.Id == cvikWrapper.CvikId);
            if (VybranyCvikDetail != null)
            {
                IsDetailDialogVisible = true;
            }
        }

        [RelayCommand]
        private void ZavritDetail()
        {
            IsDetailDialogVisible = false;
        }

        [RelayCommand]
        private void ZavritDokoncenyTrenink()
        {
            IsWorkoutFinishedDialogVisible = false;
        }
    }
}