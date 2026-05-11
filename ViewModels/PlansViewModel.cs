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
    // Pomocná třída pro zaškrtávací seznam cviků při tvorbě plánu
    public partial class VyberCvikViewModel : ObservableObject
    {
        [ObservableProperty]
        private Cvik _cvikDetail;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private int _poradi;
    }

    public partial class PlansViewModel : ViewModelBase
    {
        private readonly DatabaseService _db = new DatabaseService();

        // Seznam plánů pro zobrazení na hlavní obrazovce
        [ObservableProperty]
        private ObservableCollection<Plan> _vsechnyPlany = new ObservableCollection<Plan>();

        // Dialog nového plánu - Stavy a vstupy
        [ObservableProperty]
        private bool _isAddPlanDialogVisible;

        [ObservableProperty]
        private string _novyPlanNazev = "";

        partial void OnNovyPlanNazevChanged(string value)
        {
            if (value?.Length > 30) NovyPlanNazev = value.Substring(0, 30);
        }

        [ObservableProperty]
        private string _novyPlanUroven = "Začátečník";

        public List<string> UrovneList { get; } = new List<string> { "Začátečník", "Pokročilý", "Expert" };

        [ObservableProperty]
        private string _chybaNovyPlan = "";

        // Trackování plánu, který zrovna upravujeme
        private Plan _editovanyPlan = null;

        // Filtrování a hledání v dialogu
        [ObservableProperty]
        private string _hledanyText = string.Empty;

        partial void OnHledanyTextChanged(string value)
        {
            AplikujFiltry();
        }

        public ObservableCollection<string> KategorieList { get; } = new ObservableCollection<string>
        {
            "All", "Chest", "Back", "Arms", "Shoulders", "Legs", "Glutes", "Core", "Cardio", "Full Body", "Other"
        };

        [ObservableProperty]
        private string _vybranaKategorie = "All";

        partial void OnVybranaKategorieChanged(string value)
        {
            AplikujFiltry();
        }

        // Interní seznam všech cviků
        private List<VyberCvikViewModel> _vsechnyCvikyKVyberu = new List<VyberCvikViewModel>();

        // Seznam, který skutečně vidíme v UI podle filtrů
        [ObservableProperty]
        private ObservableCollection<VyberCvikViewModel> _zobrazeneCvikyKVyberu = new ObservableCollection<VyberCvikViewModel>();

        // Dialog smazání - stavy
        [ObservableProperty]
        private bool _isDeleteDialogVisible;

        [ObservableProperty]
        private Plan _planKeSmazani;

        public PlansViewModel()
        {
            NactiPlany();
        }

        public void NactiPlany()
        {
            VsechnyPlany.Clear();
            var planyZDb = _db.GetAllPlany();
            foreach (var plan in planyZDb)
            {
                VsechnyPlany.Add(plan);
            }
        }

        // Dialog tvorby plánu
        [RelayCommand]
        private void OtevritNovyPlanDialog()
        {
            _editovanyPlan = null;
            NovyPlanNazev = "";
            NovyPlanUroven = "Začátečník";
            ChybaNovyPlan = "";
            
            _vsechnyCvikyKVyberu.Clear();
            var vsechnyCviky = _db.GetAllCviky();
            
            foreach (var cvik in vsechnyCviky)
            {
                _vsechnyCvikyKVyberu.Add(new VyberCvikViewModel { CvikDetail = cvik, IsSelected = false, Poradi = 0 });
            }

            HledanyText = "";
            VybranaKategorie = "All";
            AplikujFiltry();

            IsAddPlanDialogVisible = true;
        }

        [RelayCommand]
        private void OtevritEditovatDialog(Plan plan)
        {
            if (plan == null) return;
            
            _editovanyPlan = plan;
            NovyPlanNazev = plan.Nazev;
            NovyPlanUroven = plan.Uroven;
            ChybaNovyPlan = "";
            
            _vsechnyCvikyKVyberu.Clear();
            var vsechnyCviky = _db.GetAllCviky();
            
            foreach (var cvik in vsechnyCviky)
            {
                int poradiIndex = plan.CvikyIds.IndexOf(cvik.Id);
                bool isSelected = poradiIndex >= 0;
                int poradi = isSelected ? poradiIndex + 1 : 0;
                
                _vsechnyCvikyKVyberu.Add(new VyberCvikViewModel { CvikDetail = cvik, IsSelected = isSelected, Poradi = poradi });
            }

            HledanyText = "";
            VybranaKategorie = "All";
            AplikujFiltry();

            IsAddPlanDialogVisible = true;
        }

        [RelayCommand]
        private void PrepnoutVyberCviku(VyberCvikViewModel vc)
        {
            if (vc == null) return;

            if (vc.IsSelected)
            {
                // Odznačení cviku
                vc.IsSelected = false;
                int zrusenePoradi = vc.Poradi;
                vc.Poradi = 0;

                // Přepočítáme pořadí u všech zbývajících vybraných cviků (aby nevznikla díra)
                foreach (var item in _vsechnyCvikyKVyberu.Where(x => x.IsSelected && x.Poradi > zrusenePoradi))
                {
                    item.Poradi--;
                }
            }
            else
            {
                // Označení cviku a přidělení nejvyššího čísla
                vc.IsSelected = true;
                int maxPoradi = _vsechnyCvikyKVyberu.Any(x => x.IsSelected) 
                    ? _vsechnyCvikyKVyberu.Where(x => x.IsSelected).Max(x => x.Poradi) 
                    : 0;
                vc.Poradi = maxPoradi + 1;
            }
        }

        private void AplikujFiltry()
        {
            var vyfiltrovano = _vsechnyCvikyKVyberu.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(HledanyText))
            {
                vyfiltrovano = vyfiltrovano.Where(c => c.CvikDetail.Nazev.Contains(HledanyText, StringComparison.OrdinalIgnoreCase));
            }

            if (VybranaKategorie != "All")
            {
                vyfiltrovano = vyfiltrovano.Where(c => 
                    c.CvikDetail.Kategorie.Any(k => k.Equals(VybranaKategorie, StringComparison.OrdinalIgnoreCase)));
            }

            ZobrazeneCvikyKVyberu.Clear();
            
            // Řazení - oblíbené (true), pak zbytek (false), pak abecedně
            var serazeneCviky = vyfiltrovano
                .OrderByDescending(c => c.CvikDetail.JeOblibeny)
                .ThenBy(c => c.CvikDetail.Nazev);

            foreach (var cvik in serazeneCviky)
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
        private void UlozitNovyPlan()
        {
            if (string.IsNullOrWhiteSpace(NovyPlanNazev))
            {
                ChybaNovyPlan = "Název plánu nesmí být prázdný.";
                return;
            }

            // Najdeme vybrané cviky a seřadíme
            var vybraneCvikyIds = _vsechnyCvikyKVyberu
                .Where(c => c.IsSelected)
                .OrderBy(c => c.Poradi)
                .Select(c => c.CvikDetail.Id)
                .ToList();

            if (!vybraneCvikyIds.Any())
            {
                ChybaNovyPlan = "Musíte vybrat alespoň jeden cvik.";
                return;
            }

            if (_editovanyPlan != null)
            {
                // Aktualizace existujícího plánu
                _editovanyPlan.Nazev = NovyPlanNazev.Trim();
                _editovanyPlan.Uroven = NovyPlanUroven;
                _editovanyPlan.CvikyIds = vybraneCvikyIds;
                
                _db.SavePlan(_editovanyPlan);
                
                // Překreslení karty v UI
                var index = VsechnyPlany.IndexOf(_editovanyPlan);
                if (index >= 0)
                {
                    VsechnyPlany[index] = _editovanyPlan;
                }
            }
            else
            {
                // Vytvoření nového plánu
                var novyPlan = new Plan
                {
                    Nazev = NovyPlanNazev.Trim(),
                    Uroven = NovyPlanUroven,
                    CvikyIds = vybraneCvikyIds,
                    JeVlastni = true
                };

                _db.SavePlan(novyPlan);
                VsechnyPlany.Add(novyPlan);
            }

            IsAddPlanDialogVisible = false;
        }

        [RelayCommand]
        private void ZrusitNovyPlan()
        {
            IsAddPlanDialogVisible = false;
        }
        
        [RelayCommand]
        private void OtevritSmazatDialog(Plan plan)
        {
            if (plan == null) return;
            PlanKeSmazani = plan;
            IsDeleteDialogVisible = true;
        }

        [RelayCommand]
        private void PotvrditSmazani()
        {
            if (PlanKeSmazani != null)
            {
                _db.DeletePlan(PlanKeSmazani.Id);
                VsechnyPlany.Remove(PlanKeSmazani);
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