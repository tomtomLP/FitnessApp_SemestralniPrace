using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitnessApp.Models;
using FitnessApp.Services;

namespace FitnessApp.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly DatabaseService _db = new DatabaseService();
        private Uzivatel _uzivatel;

        [ObservableProperty]
        private bool _isProfileVisible;

        [ObservableProperty] 
        private string _jmeno = "";
        
        partial void OnJmenoChanged(string value)
        {
            if (value?.Length > 30) Jmeno = value.Substring(0, 30);
        }

        public List<string> MoznostiPohlavi { get; } = new List<string> { "Muž", "Žena", "Jiné" };
        
        [ObservableProperty] 
        private string _vybranePohlavi = "Muž";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Vek))]
        private DateTimeOffset _datumNarozeni;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Bmi))]
        [NotifyPropertyChangedFor(nameof(BmiText))]
        [NotifyPropertyChangedFor(nameof(BmiSloupec))]
        private double? _vyskaCm;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Bmi))]
        [NotifyPropertyChangedFor(nameof(BmiText))]
        [NotifyPropertyChangedFor(nameof(BmiSloupec))]
        private double? _aktualniVaha;

        [ObservableProperty] 
        private string _chybaVyska = "";

        [ObservableProperty] 
        private string _zpravaUlozeno = "";

        [ObservableProperty] 
        private string _profilovkaCesta = "👤";

        [ObservableProperty] 
        private bool _sledovatVahuSkutecnost;

        [ObservableProperty] 
        private double? _cilovaVahaKg;

        private bool _sledovatVahuUI;
        public bool SledovatVahuUI
        {
            get => _sledovatVahuUI;
            set
            {
                if (_sledovatVahuUI != value)
                {
                    SetProperty(ref _sledovatVahuUI, value);

                    if (value)
                    {
                        DialogCilovaVaha = CilovaVahaKg > 0 ? CilovaVahaKg : null;
                        IsTargetWeightDialogVisible = true;
                    }
                    else
                    {
                        IsDisableTrackingDialogVisible = true;
                    }
                }
            }
        }

        // Dialog - stavy
        [ObservableProperty] private bool _isTargetWeightDialogVisible;
        [ObservableProperty] private bool _isDisableTrackingDialogVisible;
        [ObservableProperty] private bool _isLogWeightDialogVisible;
        
        [ObservableProperty] private bool _isResetDialogVisible;

        [ObservableProperty] private double? _dialogCilovaVaha;
        [ObservableProperty] private double? _dialogAktualniVaha;

        [ObservableProperty]
        private string _vybraneTema = "Cyber Future";

        [ObservableProperty]
        private bool _restTimerEnabled = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RestTimerDisplay))]
        private int _restTimerSeconds = 180;

        public string RestTimerDisplay => $"{RestTimerSeconds / 60}:{RestTimerSeconds % 60:D2}";

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

        public double Bmi
        {
            get
            {
                if (VyskaCm == null || VyskaCm <= 0) return 0;
                double vaha = AktualniVaha ?? 0;
                if (vaha <= 0) return 0;

                double vyskaM = VyskaCm.Value / 100.0;
                return Math.Round(vaha / (vyskaM * vyskaM), 1);
            }
        }

        public string BmiText
        {
            get
            {
                double bmi = Bmi;
                if (bmi == 0) return "Nezadáno";
                if (bmi < 18.5) return "Podváha";
                if (bmi >= 18.5 && bmi < 25) return "Normální váha";
                if (bmi >= 25 && bmi < 30) return "Nadváha";
                return "Obezita";
            }
        }

        public int BmiSloupec
        {
            get
            {
                double bmi = Bmi;
                if (bmi == 0 || bmi < 18.5) return 0;
                if (bmi >= 18.5 && bmi < 25) return 1;
                if (bmi >= 25 && bmi < 30) return 2;
                return 3;
            }
        }

        public SettingsViewModel()
        {
            NactiData();
        }

        public void NactiData()
        {
            _uzivatel = _db.GetUzivatel();

            if (_uzivatel == null)
            {
                _uzivatel = new Uzivatel();
            }

            Jmeno = string.IsNullOrWhiteSpace(_uzivatel.Jmeno) ? "Uživatel" : _uzivatel.Jmeno;
            VybranePohlavi = MoznostiPohlavi.Contains(_uzivatel.Pohlavi) ? _uzivatel.Pohlavi : "Muž";
            
            if (_uzivatel.DatumNarozeni.Year < 1900) _uzivatel.DatumNarozeni = new DateTime(2000, 1, 1);
            DatumNarozeni = new DateTimeOffset(_uzivatel.DatumNarozeni);
            
            VyskaCm = _uzivatel.VyskaCm > 0 ? _uzivatel.VyskaCm : null;
            CilovaVahaKg = _uzivatel.CilovaVahaKg > 0 ? _uzivatel.CilovaVahaKg : null;
            AktualniVaha = _uzivatel.VahaKg > 0 ? _uzivatel.VahaKg : null; 
            
            SledovatVahuSkutecnost = _uzivatel.SledovatVahu;
            
            RestTimerEnabled = _uzivatel.RestTimerZapnuty;
            RestTimerSeconds = _uzivatel.RestTimerSekundy > 0 ? _uzivatel.RestTimerSekundy : 180;

            _sledovatVahuUI = _uzivatel.SledovatVahu;
            OnPropertyChanged(nameof(SledovatVahuUI));
        }

        [RelayCommand]
        private void OtevritProfil()
        {
            IsProfileVisible = true;
            ZpravaUlozeno = "";
            ChybaVyska = "";
        }

        [RelayCommand]
        private void ZavritProfil()
        {
            UlozNastaveni();
            if (string.IsNullOrEmpty(ChybaVyska))
            {
                IsProfileVisible = false;
            }
        }

        [RelayCommand]
        private void UlozNastaveni()
        {
            ChybaVyska = "";
            ZpravaUlozeno = "";

            if (VyskaCm != null && VyskaCm != 0 && (VyskaCm < 50 || VyskaCm > 250))
            {
                ChybaVyska = "Zadejte reálnou výšku (50 - 250 cm) nebo nechte prázdné.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Jmeno)) Jmeno = "Uživatel";

            _uzivatel.Jmeno = Jmeno;
            _uzivatel.Pohlavi = VybranePohlavi;
            _uzivatel.DatumNarozeni = DatumNarozeni.Date;
            _uzivatel.VyskaCm = VyskaCm ?? 0;
            _uzivatel.CilovaVahaKg = CilovaVahaKg ?? 0;
            _uzivatel.VahaKg = AktualniVaha ?? 0;
            _uzivatel.SledovatVahu = SledovatVahuSkutecnost;
            
            _uzivatel.RestTimerZapnuty = RestTimerEnabled;
            _uzivatel.RestTimerSekundy = RestTimerSeconds;

            _db.SaveUzivatel(_uzivatel);
            ZpravaUlozeno = "Úspěšně uloženo!";
        }

        [RelayCommand]
        private void PotvrditCilovouVahu()
        {
            double vaha = DialogCilovaVaha ?? 0;
            if (vaha > 0)
            {
                CilovaVahaKg = vaha;
                SledovatVahuSkutecnost = true;
                IsTargetWeightDialogVisible = false;
                UlozNastaveni();
            }
        }

        [RelayCommand]
        private void ZrusitCilovouVahu()
        {
            _sledovatVahuUI = false;
            OnPropertyChanged(nameof(SledovatVahuUI));
            IsTargetWeightDialogVisible = false;
        }

        [RelayCommand]
        private void PotvrditZruseniSledovani()
        {
            if (_uzivatel.HistorieVahy != null)
            {
                _uzivatel.HistorieVahy.Clear();
            }
            AktualniVaha = null;
            CilovaVahaKg = null;
            SledovatVahuSkutecnost = false;
            IsDisableTrackingDialogVisible = false;
            
            UlozNastaveni();
            
            OnPropertyChanged(nameof(Bmi));
            OnPropertyChanged(nameof(BmiText));
            OnPropertyChanged(nameof(BmiSloupec));
        }

        [RelayCommand]
        private void ZrusitZruseniSledovani()
        {
            _sledovatVahuUI = true;
            OnPropertyChanged(nameof(SledovatVahuUI));
            IsDisableTrackingDialogVisible = false;
        }

        [RelayCommand]
        private void OtevritZaznamVahyDialog()
        {
            DialogAktualniVaha = AktualniVaha;
            IsLogWeightDialogVisible = true;
        }

        [RelayCommand]
        private void PotvrditZaznamVahy()
        {
            double novaVaha = DialogAktualniVaha ?? 0;
            if (novaVaha <= 0) return;

            AktualniVaha = novaVaha;
            var dnes = DateTime.Today;

            if (_uzivatel.HistorieVahy == null)
            {
                _uzivatel.HistorieVahy = new List<ZaznamVahy>();
            }

            var existujiciZaznam = _uzivatel.HistorieVahy.FirstOrDefault(z => z.Datum.Date == dnes);

            if (existujiciZaznam != null) 
            {
                existujiciZaznam.Vaha = novaVaha; 
            }
            else 
            {
                _uzivatel.HistorieVahy.Add(new ZaznamVahy { Datum = dnes, Vaha = novaVaha });
            }

            _uzivatel.VahaKg = novaVaha;
            _db.SaveUzivatel(_uzivatel);
            
            IsLogWeightDialogVisible = false;

            OnPropertyChanged(nameof(Bmi));
            OnPropertyChanged(nameof(BmiText));
            OnPropertyChanged(nameof(BmiSloupec));
            
            ZpravaUlozeno = "Dnešní váha zaznamenána!";
        }

        [RelayCommand]
        private void ZrusitZaznamVahy()
        {
            IsLogWeightDialogVisible = false;
        }

        [RelayCommand]
        private void ZmenitProfilovouFotku()
        {
            ZpravaUlozeno = "Výběr fotky není podporován.";
        }
        
        // Reset aplikace
        [RelayCommand]
        private void ResetovatAplikaci()
        {
            IsResetDialogVisible = true;
        }

        [RelayCommand]
        private void ZrusitReset()
        {
            IsResetDialogVisible = false;
        }

        [RelayCommand]
        private void PotvrditReset()
        {
            // Smazání všech plánů
            foreach (var plan in _db.GetAllPlany())
            {
                _db.DeletePlan(plan.Id);
            }

            // Smazání celé historie tréninků
            foreach (var zaznam in _db.GetAllZaznamy())
            {
                _db.DeleteZaznam(zaznam.Id);
            }

            // Přepsání uživatelského profilu
            _uzivatel = new Uzivatel();
            _db.SaveUzivatel(_uzivatel);

            // Zavření dialogu
            NactiData();
            IsResetDialogVisible = false;

            // Aktualizace BMI ukazatelů
            OnPropertyChanged(nameof(Bmi));
            OnPropertyChanged(nameof(BmiText));
            OnPropertyChanged(nameof(BmiSloupec));
        }

        [RelayCommand]
        private void UberCas()
        {
            if (RestTimerSeconds >= 15)
            {
                RestTimerSeconds -= 15;
                UlozNastaveni();
            }
        }

        [RelayCommand]
        private void PridejCas()
        {
            RestTimerSeconds += 15;
            UlozNastaveni();
        }

        partial void OnRestTimerEnabledChanged(bool value)
        {
            UlozNastaveni();
        }
    }
}