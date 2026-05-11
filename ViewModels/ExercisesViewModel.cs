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
    public partial class ExercisesViewModel : ViewModelBase
    {
        private readonly DatabaseService _db = new DatabaseService();

        private List<Cvik> _vsechnyCviky = new List<Cvik>();

        [ObservableProperty]
        private ObservableCollection<Cvik> _zobrazeneCviky = new ObservableCollection<Cvik>();

        [ObservableProperty]
        private string _hledanyText = string.Empty;

        partial void OnHledanyTextChanged(string value)
        {
            AplikujFiltry();
        }

        // Kategorie
        public ObservableCollection<string> KategorieList { get; } = new ObservableCollection<string>
        {
            "All", "Chest", "Back", "Arms", "Shoulders", "Legs", "Glutes", "Core", "Cardio", "Full Body", "Other"
        };
        
        // Nový cvik - kategorie
        public ObservableCollection<string> KategorieProNovyCvik { get; } = new ObservableCollection<string>
        {
            "Chest", "Back", "Arms", "Shoulders", "Legs", "Glutes", "Core", "Cardio", "Full Body", "Other"
        };

        [ObservableProperty]
        private string _vybranaKategorie = "All";

        partial void OnVybranaKategorieChanged(string value)
        {
            AplikujFiltry();
        }

        // Dialog vlastního cviku
        [ObservableProperty] 
        private bool _isAddExerciseDialogVisible;

        [ObservableProperty] 
        private string _novyCvikNazev = "";

        partial void OnNovyCvikNazevChanged(string value)
        {
            if (value?.Length > 50) NovyCvikNazev = value.Substring(0, 50);
        }

        [ObservableProperty] 
        private string _novyCvikPopis = "";

        partial void OnNovyCvikPopisChanged(string value)
        {
            if (value?.Length > 500) NovyCvikPopis = value.Substring(0, 500);
        }

        [ObservableProperty] 
        private string _novyCvikKategorie = "Full Body";

        [ObservableProperty] 
        private string _chybaNovyCvik = "";

        // Dialog detailu cviku
        [ObservableProperty]
        private bool _isDetailDialogVisible;

        [ObservableProperty]
        private Cvik _vybranyCvikDetail;
        
        [ObservableProperty]
        private bool _isDeleteDialogVisible;

        [ObservableProperty]
        private Cvik _cvikKeSmazani;

        public ExercisesViewModel()
        {
            NactiCviky();
        }

        private void NactiCviky()
        {
            _vsechnyCviky = _db.GetAllCviky();

            if (!_vsechnyCviky.Any())
            {
                VytvorVychoziCviky();
                _vsechnyCviky = _db.GetAllCviky();
            }

            AplikujFiltry();
        }

        private void AplikujFiltry()
        {
            var vyfiltrovano = _vsechnyCviky.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(HledanyText))
            {
                vyfiltrovano = vyfiltrovano.Where(c => c.Nazev.Contains(HledanyText, StringComparison.OrdinalIgnoreCase));
            }

            if (VybranaKategorie != "All")
            {
                vyfiltrovano = vyfiltrovano.Where(c => 
                    c.Kategorie.Any(k => k.Equals(VybranaKategorie, StringComparison.OrdinalIgnoreCase)));
            }

            ZobrazeneCviky.Clear();
            
            var serazeneCviky = vyfiltrovano
                .OrderByDescending(c => c.JeOblibeny)
                .ThenBy(c => c.Nazev);

            foreach (var cvik in serazeneCviky)
            {
                ZobrazeneCviky.Add(cvik);
            }
        }

        [RelayCommand]
        private void ZmenKategorii(string kategorie)
        {
            VybranaKategorie = kategorie;
        }

        [RelayCommand]
        private void PrepnoutOblibene(Cvik cvik)
        {
            if (cvik == null) return;
            
            cvik.JeOblibeny = !cvik.JeOblibeny;
            _db.SaveCvik(cvik);
            
            AplikujFiltry();
        }

        [RelayCommand]
        private void PridatVlastniCvik()
        {
            NovyCvikNazev = "";
            NovyCvikPopis = "";
            NovyCvikKategorie = "Full Body";
            ChybaNovyCvik = "";
            IsAddExerciseDialogVisible = true;
        }

        [RelayCommand]
        private void PotvrditNovyCvik()
        {
            if (string.IsNullOrWhiteSpace(NovyCvikNazev))
            {
                ChybaNovyCvik = "Název cviku nesmí být prázdný.";
                return;
            }

            var novyCvik = new Cvik
            {
                Nazev = NovyCvikNazev.Trim(),
                Popis = NovyCvikPopis?.Trim() ?? "",
                Kategorie = new List<string> { NovyCvikKategorie },
                JeOblibeny = false
            };

            _db.SaveCvik(novyCvik);
            _vsechnyCviky.Add(novyCvik);
            
            IsAddExerciseDialogVisible = false;
            AplikujFiltry();
        }

        [RelayCommand]
        private void ZrusitNovyCvik()
        {
            IsAddExerciseDialogVisible = false;
        }
        
        [RelayCommand]
        private void OtevritSmazatDialog(Cvik cvik)
        {
            if (cvik == null) return;
            CvikKeSmazani = cvik;
            IsDeleteDialogVisible = true;
        }

        [RelayCommand]
        private void PotvrditSmazani()
        {
            if (CvikKeSmazani != null)
            {
                _db.DeleteCvik(CvikKeSmazani.Id);
                _vsechnyCviky.Remove(CvikKeSmazani);
                AplikujFiltry();
            }
            IsDeleteDialogVisible = false;
        }

        [RelayCommand]
        private void ZrusitSmazani()
        {
            IsDeleteDialogVisible = false;
        }
        
        [RelayCommand]
        private void ZobrazitDetail(Cvik cvik)
        {
            if (cvik == null) return;
            VybranyCvikDetail = cvik;
            IsDetailDialogVisible = true;
        }

        [RelayCommand]
        private void ZavritDetail()
        {
            IsDetailDialogVisible = false;
        }

        private void VytvorVychoziCviky()
        {
            var vychoziCviky = new List<Cvik>
            {
                new Cvik { Nazev = "Kliky (Push Up)", Kategorie = new List<string> { "Chest", "Arms", "Core" }, Popis = "1. Zaujměte pozici prkna s rukama na šířku ramen. 2. Zpevněte břicho a hýždě, tělo držte v jedné linii. 3. S nádechem klesejte hrudníkem těsně nad zem, lokty směřují šikmo vzad (cca 45 stupňů). 4. S výdechem se vytlačte zpět do výchozí pozice." },
                new Cvik { Nazev = "Shyby nadhmatem (Pull Up)", Kategorie = new List<string> { "Back", "Arms" }, Popis = "1. Zavěste se na hrazdu nadhmatem na šířku širší než ramena. 2. Zpevněte lopatky a mírně se prohněte v hrudníku. 3. S výdechem se přitáhněte bradou nad úroveň hrazdy, lokty tlačte dolů k tělu. 4. S nádechem se kontrolovaně spusťte do plného vyvěšení." },
                new Cvik { Nazev = "Shyby podhmatem (Chin Up)", Kategorie = new List<string> { "Back", "Arms" }, Popis = "1. Zavěste se na hrazdu podhmatem (dlaně k sobě) na šířku ramen. 2. Zpevněte střed těla. 3. S výdechem se přitáhněte bradou nad hrazdu, soustřeďte se na biceps a záda. 4. S nádechem se pomalu spusťte dolů." },
                new Cvik { Nazev = "Kliky na bradlech (Dip)", Kategorie = new List<string> { "Arms", "Chest", "Shoulders" }, Popis = "1. Vzepřete se na bradlech s nataženýma rukama. 2. Mírně se předkloňte pro zacílení na prsa (nebo stůjte rovně pro triceps). 3. S nádechem pokrčte lokty a klesejte, dokud nejsou ramena pod úrovní loktů. 4. S výdechem se vytlačte zpět nahoru." },
                new Cvik { Nazev = "Dřepy s vl. vahou (Air Squat)", Kategorie = new List<string> { "Legs", "Glutes", "Core" }, Popis = "1. Postavte se s nohama na šířku ramen, špičky mírně ven. 2. S nádechem posouvejte boky dozadu a dolů, záda držte rovná. 3. Klesněte alespoň do úrovně, kde jsou stehna rovnoběžně se zemí. 4. S výdechem se přes paty zvedněte zpět, v horní pozici zatněte hýždě." },
                new Cvik { Nazev = "Výpady (Lunge)", Kategorie = new List<string> { "Legs", "Glutes", "Core" }, Popis = "1. Stůjte rovně, nohy u sebe. 2. Udělejte dlouhý krok vpřed jednou nohou. 3. Klesejte dolů, dokud se zadní koleno téměř nedotkne země (obě kolena svírají 90 stupňů). 4. Odtlačte se přední nohou zpět do výchozí pozice a vystřídejte nohy." },
                new Cvik { Nazev = "Plank", Kategorie = new List<string> { "Core" }, Popis = "1. Opřete se o předloktí a špičky nohou, lokty jsou přímo pod rameny. 2. Zpevněte břicho, hýždě a stehna. 3. Držte tělo v dokonalé rovině (neprohýbejte se v bedrech ani nevystrkujte zadek). 4. Dýchejte pravidelně a držte pozici po určený čas." },
                new Cvik { Nazev = "Angličáky (Burpee)", Kategorie = new List<string> { "Cardio", "Legs", "Glutes", "Chest" }, Popis = "1. Ze stoje jděte do dřepu a položte ruce na zem. 2. Odskočte nohama dozadu do pozice kliku. 3. Udělejte klik (hrudník na zem). 4. Přiskočte nohama zpět k rukám a vyskočte do výšky s tlesknutím nad hlavou." },
                new Cvik { Nazev = "Horolezec (Mountain Climber)", Kategorie = new List<string> { "Cardio", "Core" }, Popis = "1. Zaujměte pozici vzporu (jako na horní pozici kliku). 2. Zpevněte břicho. 3. Střídavě a dynamicky přitahujte kolena směrem k hrudníku. 4. Udržujte rychlé tempo a rovná záda." },
                new Cvik { Nazev = "Zvedání nohou ve visu (Hanging Leg Raise)", Kategorie = new List<string> { "Core" }, Popis = "1. Zavěste se na hrazdu. 2. Zpevněte břicho a bez švihu zvedejte natažené nohy před sebe. 3. Snažte se dostat nohy alespoň do pravého úhlu s tělem. 4. Kontrolovaně spusťte nohy dolů (nenechte je spadnout)." },
                new Cvik { Nazev = "Muscle Up", Kategorie = new List<string> { "Other", "Back", "Chest" }, Popis = "1. Začněte dynamickým shybem s mírným zhoupnutím. 2. Explozivně se přitáhněte co nejvýše (až k pasu). 3. V horní fázi rychle přetočte zápěstí a dostaňte hrudník nad hrazdu. 4. Dokončete pohyb vytlačením (klikem) na hrazdě do narovnaných rukou." },
                new Cvik { Nazev = "Výskoky na bednu (Box Jump)", Kategorie = new List<string> { "Legs", "Glutes", "Cardio" }, Popis = "1. Postavte se před bednu na šířku ramen. 2. Jděte do mírného podřepu a švihněte rukama dozadu. 3. Explozivně vyskočte snožmo na bednu. 4. Dopadněte měkce do podřepu a postavte se do plného narovnání." },
                new Cvik { Nazev = "Diamantové kliky (Diamond Push Up)", Kategorie = new List<string> { "Chest", "Arms" }, Popis = "1. Zaujměte pozici kliku, ale spojte ukazováčky a palce rukou pod hrudníkem do tvaru diamantu. 2. Zpevněte střed těla. 3. Klesejte hrudníkem k rukám, lokty držte u těla. 4. Vytlačte se zpět, soustřeďte se na triceps." },
                new Cvik { Nazev = "Pistole (Pistol Squat)", Kategorie = new List<string> { "Legs", "Glutes" }, Popis = "1. Postavte se na jednu nohu, druhou přednožte. 2. Předpažte ruce pro rovnováhu. 3. Klesejte na stojné noze do hlubokého dřepu, patu držte na zemi. 4. Silou stehna se vytlačte zpět nahoru." },
                new Cvik { Nazev = "Boční Plank (Side Plank)", Kategorie = new List<string> { "Core", "Glutes" }, Popis = "1. Lehněte si na bok a opřete se o předloktí. 2. Zvedněte boky ze země tak, aby tělo tvořilo přímku. 3. Druhou ruku můžete zvednout ke stropu. 4. Držte pozici a dýchejte, poté vyměňte strany." },
                new Cvik { Nazev = "Výpony na lýtka (Calf Raise)", Kategorie = new List<string> { "Legs" }, Popis = "1. Postavte se na kraj schodu nebo podložky špičkami nohou. 2. Paty nechte volně klesnout pod úroveň schodu (protažení). 3. Výdechem se zvedněte co nejvýše na špičky. 4. Vteřinu podržte a pomalu vraťte dolů." },
                new Cvik { Nazev = "Australské shyby (Inverted Row)", Kategorie = new List<string> { "Back" }, Popis = "1. Najděte si nízkou hrazdu (cca ve výši pasu). 2. Lehněte si pod ni, chytněte ji a natáhněte tělo (opora o paty). 3. Přitáhněte hrudník k hrazdě stahováním lopatek k sobě. 4. Pomalu se spusťte zpět do natažených rukou." },
                new Cvik { Nazev = "Sklapovačky (V-Up)", Kategorie = new List<string> { "Core" }, Popis = "1. Lehněte si na záda, ruce natáhněte za hlavu, nohy propněte. 2. Najednou zvedněte trup i nohy a snažte se dotknout rukama špiček nohou. 3. Tělo tvoří písmeno V. 4. Kontrolovaně se položte zpět na zem." },
                new Cvik { Nazev = "Dřep s činkou (Barbell Squat)", Kategorie = new List<string> { "Legs", "Glutes", "Core" }, Popis = "1. Umístěte velkou činku na horní část trapézů. 2. Rozkročte se na šířku ramen, špičky mírně ven. 3. S nádechem jděte do dřepu, záda držte pevná a rovná, kolena jdou směrem za špičkami. 4. S výdechem tlačte do pat a vraťte se do stoje.", JeOblibeny = true },
                new Cvik { Nazev = "Mrtvý tah (Deadlift)", Kategorie = new List<string> { "Back", "Legs", "Glutes", "Core" }, Popis = "1. Postavte se k čince, holeně se téměř dotýkají tyče. 2. Uchopte činku nadhmatem nebo střídavým úchopem, záda držte rovná, hrudník vypnutý. 3. S výdechem zvedejte činku narovnáním v bocích a kolenou, tyč kopíruje nohy. 4. V horní pozici zatněte hýždě (nezaklánějte se) a kontrolovaně položte činku zpět.", JeOblibeny = true },
                new Cvik { Nazev = "Bench Press", Kategorie = new List<string> { "Chest", "Arms" }, Popis = "1. Lehněte si na lavici, oči jsou pod tyčí. 2. Uchopte činku šířeji než ramena, lopatky stáhněte k sobě. 3. S nádechem spusťte činku na střed hrudníku, lokty svírají úhel cca 45 stupňů s tělem. 4. S výdechem vytlačte činku zpět nahoru.", JeOblibeny = true },
                new Cvik { Nazev = "Tlaky nad hlavu (Overhead Press)", Kategorie = new List<string> { "Shoulders", "Arms" }, Popis = "1. Stůjte s činkou položenou na předních deltech (úchop na šířku ramen). 2. Zpevněte břicho a hýždě. 3. Tlačte činku kolmo nad hlavu, uhněte jí mírně hlavou vzad. 4. V horní pozici propněte lokty a s nádechem spusťte zpět na ramena." },
                new Cvik { Nazev = "Přítahy v předklonu (Bent Over Row)", Kategorie = new List<string> { "Back", "Arms" }, Popis = "1. Uchopte činku, mírně pokrčte kolena a předkloňte se (trup téměř rovnoběžně se zemí). 2. Záda držte rovná. 3. Přitáhněte činku k dolní části břicha, lokty táhněte podél těla. 4. Kontrolovaně spusťte činku zpět do natažených rukou." },
                new Cvik { Nazev = "Tlaky s jednoručkami (Dumbbell Press)", Kategorie = new List<string> { "Chest", "Arms" }, Popis = "1. Lehněte si na lavici s jednoručkami v natažených rukou nad hrudníkem. 2. S nádechem pokrčte lokty a spusťte činky po stranách hrudníku. 3. Jděte do hloubky pro protažení prsních svalů. 4. S výdechem vytlačte činky zpět k sobě nad hrudník." },
                new Cvik { Nazev = "Tlaky na ramena (Dumbbell Shoulder Press)", Kategorie = new List<string> { "Shoulders", "Arms" }, Popis = "1. Posaďte se na lavici s opěrkou, činky držte v úrovni uší. 2. Dlaně směřují vpřed. 3. S výdechem vytlačte činky nad hlavu, nahoře se téměř dotknou. 4. S nádechem je pomalu spusťte zpět do úrovně uší." },
                new Cvik { Nazev = "Bicepsový zdvih (Dumbbell Curl)", Kategorie = new List<string> { "Arms" }, Popis = "1. Stůjte s jednoručkami podél těla, dlaně k tělu. 2. S výdechem zvedejte činky a vytáčejte dlaně vzhůru (supinace). 3. V horní fázi silně zatněte biceps. 4. S nádechem pomalu spouštějte činky zpět." },
                new Cvik { Nazev = "Francouzské tlaky (Skullcrusher)", Kategorie = new List<string> { "Arms" }, Popis = "1. Lehněte si na lavici, činku (EZ tyč) držte v natažených rukou nad čelem. 2. Lokty držte na místě a s nádechem krčte pouze předloktí. 3. Spusťte činku těsně nad čelo. 4. S výdechem narovnejte ruce zpět do výchozí pozice." },
                new Cvik { Nazev = "Rumunský mrtvý tah (RDL)", Kategorie = new List<string> { "Legs", "Glutes", "Back" }, Popis = "1. Stůjte s činkou v natažených rukou. 2. Mírně pokrčte kolena a tento úhel už neměňte. 3. S nádechem posouvejte boky dozadu a předklánějte se s rovnými zády. 4. Jakmile ucítíte tah v hamstrinzích (cca pod koleny), s výdechem se vraťte zpět." },
                new Cvik { Nazev = "Bulharský dřep (Bulgarian Split Squat)", Kategorie = new List<string> { "Legs", "Glutes" }, Popis = "1. Postavte se zády k lavici a jednu nohu na ni položte nártem. 2. Druhou nohou stůjte vpřed. 3. Klesejte v bocích kolmo dolů, dokud zadní koleno není těsně nad zemí. 4. Tlačte do přední paty a zvedněte se zpět." },
                new Cvik { Nazev = "Upažování (Lateral Raise)", Kategorie = new List<string> { "Shoulders" }, Popis = "1. Stůjte mírně předkloněni s jednoručkami podél těla. 2. S výdechem zvedejte činky obloukem do stran až do výše ramen. 3. Lokty mějte mírně pokrčené, malíčky tlačte výše než palce. 4. Pomalu spusťte činky zpět k bokům." },
                new Cvik { Nazev = "Rozpažování (Dumbbell Fly)", Kategorie = new List<string> { "Chest" }, Popis = "1. Lehněte si na lavici, činky držte nad hrudníkem dlaněmi k sobě. 2. Lokty mějte mírně pokrčené. 3. S nádechem otevírejte náruč a spouštějte činky do stran (velký oblouk). 4. S výdechem stáhněte činky zpět nad hrudník." },
                new Cvik { Nazev = "Incline Bench Press (Horní prsa)", Kategorie = new List<string> { "Chest", "Arms" }, Popis = "1. Nastavte lavici na sklon 30-45 stupňů. 2. Lehněte si a uchopte činku nadhmatem. 3. Spouštějte činku na horní část hrudníku (pod klíční kosti). 4. Vytlačte činku kolmo vzhůru." },
                new Cvik { Nazev = "Přítahy jednoručky v opře (One Arm Row)", Kategorie = new List<string> { "Back", "Arms" }, Popis = "1. Klekněte si jedním kolenem na lavici a opřete se o ni rukou. 2. Do druhé ruky vezměte činku, záda držte rovná. 3. Přitáhněte činku k boku, loket tlačte vysoko a blízko těla. 4. Spusťte činku zpět k zemi a protáhněte záda." },
                new Cvik { Nazev = "Výpady s činkami (Weighted Lunge)", Kategorie = new List<string> { "Legs", "Glutes" }, Popis = "1. Vezměte do každé ruky jednoručku, stůjte rovně. 2. Vykročte vpřed a klesněte do výpadu. 3. Trup držte vzpřímený. 4. Odtlačte se zpět a vystřídejte nohy." },
                new Cvik { Nazev = "Hip Thrust", Kategorie = new List<string> { "Glutes", "Legs" }, Popis = "1. Sedněte si na zem, zády se opřete o lavici (lopatkami). 2. Přes boky si položte velkou činku (ideálně s polstrováním). 3. Zvedejte boky nahoru, dokud tělo není v rovině, zatněte hýždě. 4. Kontrolovaně spusťte boky dolů." },
                new Cvik { Nazev = "Kladivové zdvihy (Hammer Curl)", Kategorie = new List<string> { "Arms" }, Popis = "1. Stůjte s jednoručkami, dlaně směřují k tělu (neutrální úchop). 2. Zvedejte činky k ramenům bez vytáčení zápěstí. 3. Lokty držte fixované u těla. 4. Spusťte činky zpět." },
                new Cvik { Nazev = "Zapažování v předklonu (Reverse Fly)", Kategorie = new List<string> { "Shoulders", "Back" }, Popis = "1. Předkloňte se s jednoručkami, záda rovná. 2. Paže visí dolů, lokty mírně pokrčené. 3. Zvedejte činky do stran (rozpažujte) se zaměřením na zadní ramena. 4. Pomalu vraťte zpět." },
                new Cvik { Nazev = "Goblet Dřep", Kategorie = new List<string> { "Legs", "Glutes" }, Popis = "1. Držte jednu jednoručku nebo kettlebell oběma rukama u hrudníku. 2. Postavte se na šířku ramen. 3. Jděte do hlubokého dřepu, lokty se mohou dotknout vnitřní strany kolen. 4. Vytlačte se zpět nahoru." },
                new Cvik { Nazev = "Leg Press", Kategorie = new List<string> { "Legs", "Glutes" }, Popis = "1. Posaďte se do stroje, chodidla umístěte na desku na šířku ramen. 2. Odjistěte pojistku a pomalu krčte kolena směrem k hrudníku. 3. Klesejte co nejníže, aniž by se zvedala pánev. 4. Tlačte do desky (hlavně patami) a vytlačte zátěž zpět (nepropínejte kolena úplně)." },
                new Cvik { Nazev = "Stahování kladky (Lat Pulldown)", Kategorie = new List<string> { "Back", "Arms" }, Popis = "1. Posaďte se, zapřete kolena pod válce. 2. Uchopte adaptér širokým nadhmatem. 3. S výdechem stahujte kladku k horní části hrudníku, lokty tlačte dolů a vzad. 4. S nádechem pomalu pouštějte kladku nahoru do protažení." },
                new Cvik { Nazev = "Předkopávání (Leg Extension)", Kategorie = new List<string> { "Legs" }, Popis = "1. Posaďte se a nastavte válec tak, aby byl na spodní části holení. 2. Držte se madel po stranách. 3. S výdechem narovnejte nohy a v horní pozici zatněte kvadricepsy. 4. Pomalu spouštějte nohy dolů (nepokládejte úplně závaží)." },
                new Cvik { Nazev = "Zakopávání (Seated Leg Curl)", Kategorie = new List<string> { "Legs" }, Popis = "1. Posaďte se (nebo lehněte dle typu stroje), válec je za kotníky. 2. Stehna zafixujte opěrkou. 3. S výdechem krčte nohy pod sebe co nejdále. 4. Pomalu vracejte nohy do natažení." },
                new Cvik { Nazev = "Chest Press Machine", Kategorie = new List<string> { "Chest", "Arms" }, Popis = "1. Nastavte sedátko tak, aby madla byla v úrovni středu hrudníku. 2. Uchopte madla, lopatky tlačte do opěrky. 3. Vytlačte madla před sebe, ale nepropínejte lokty. 4. Pomalu vracejte madla k hrudníku." },
                new Cvik { Nazev = "Veslování na stroji (Seated Cable Row)", Kategorie = new List<string> { "Back" }, Popis = "1. Posaďte se k dolní kladce nebo stroji, nohy mírně pokrčené. 2. Uchopte adaptér, záda držte kolmo k zemi. 3. Přitáhněte adaptér k pasu, stáhněte lopatky k sobě. 4. S nádechem se nechte protáhnout dopředu." },
                new Cvik { Nazev = "Peck Deck (Fly Machine)", Kategorie = new List<string> { "Chest" }, Popis = "1. Posaďte se, lokty a předloktí opřete o polštáře (nebo chyťte madla). 2. Lokty by měly být v úrovni ramen. 3. S výdechem tlačte ruce k sobě před hrudník. 4. S nádechem pomalu rozevírejte ruce do protažení prsou." },
                new Cvik { Nazev = "Shyby s dopomocí (Assisted Pull Up)", Kategorie = new List<string> { "Back", "Arms" }, Popis = "1. Nastavte protizávaží (čím více, tím je cvik lehčí). 2. Klekněte si na plošinu. 3. Uchopte hrazdu a provádějte technický shyb s dopomocí stroje. 4. Kontrolovaně se spouštějte dolů." },
                new Cvik { Nazev = "Hack Dřep (Hack Squat)", Kategorie = new List<string> { "Legs", "Glutes" }, Popis = "1. Postavte se do stroje, ramena zapřete pod opěrky. 2. Nohy dejte na plošinu (mírně dopředu). 3. Odjistěte stroj a klesejte do hlubokého dřepu. 4. Vytlačte se zpět nahoru silou stehen." },
                new Cvik { Nazev = "Snožování (Adductor Machine)", Kategorie = new List<string> { "Legs" }, Popis = "1. Posaďte se, kolena opřete o vnější stranu opěrek. 2. Odjistěte páku. 3. S výdechem tlačte nohy k sobě silou vnitřních stehen. 4. Pomalu nohy rozevírejte." },
                new Cvik { Nazev = "Roznožování (Abductor Machine)", Kategorie = new List<string> { "Glutes", "Legs" }, Popis = "1. Posaďte se, kolena jsou uvnitř opěrek. 2. Mírně se předkloňte pro lepší zapojení hýždí. 3. Tlačte kolena od sebe co nejdále. 4. Pomalu vracejte k sobě." },
                new Cvik { Nazev = "Tlaky na ramena na stroji (Machine Shoulder Press)", Kategorie = new List<string> { "Shoulders", "Arms" }, Popis = "1. Nastavte výšku sedadla tak, aby madla byla u ramen. 2. Uchopte madla nadhmatem. 3. Vytlačte váhu nad hlavu. 4. Kontrolovaně spusťte zpět dolů." },
                new Cvik { Nazev = "T-Bar Row (Přítahy T-osy)", Kategorie = new List<string> { "Back", "Arms" }, Popis = "1. Postavte se na plošinu stroje a opřete hrudník o opěrku. 2. Uchopte madla. 3. Přitáhněte zátěž k hrudníku, lokty jdou podél těla. 4. Spusťte zátěž do plného protažení zad." },
                new Cvik { Nazev = "Bicepsový stroj (Preacher Curl Machine)", Kategorie = new List<string> { "Arms" }, Popis = "1. Posaďte se a opřete paže o opěrku (podpaží přes hranu). 2. Uchopte madla podhmatem. 3. Krčte lokty a zvedejte madla k obličeji. 4. Pomalu spouštějte do natažených rukou." },
                new Cvik { Nazev = "Stahování kladky (Tricep Pushdown)", Kategorie = new List<string> { "Arms" }, Popis = "1. Postavte se čelem ke kladce, uchopte lano nebo tyč. 2. Lokty přilepte k tělu a držte je tam. 3. S výdechem stlačte kladku dolů až do propnutí paží. 4. S nádechem vraťte předloktí nahoru (lokty se nehýbou)." },
                new Cvik { Nazev = "Bicepsový zdvih na kladce (Cable Curl)", Kategorie = new List<string> { "Arms" }, Popis = "1. Uchopte spodní kladku (rovnou tyč nebo lano). 2. Stůjte rovně, lokty u těla. 3. S výdechem přitáhněte kladku k hrudníku. 4. S nádechem spusťte dolů." },
                new Cvik { Nazev = "Face Pull (Cables)", Kategorie = new List<string> { "Shoulders", "Back" }, Popis = "1. Nastavte kladku do výše očí, uchopte lano nadhmatem. 2. Tahejte lano směrem k čelu (nebo uším). 3. Zároveň táhněte ruce od sebe a lokty dozadu (lopatky k sobě). 4. Vraťte ruce před sebe." },
                new Cvik { Nazev = "Kladky protisměrné (Cable Crossover)", Kategorie = new List<string> { "Chest" }, Popis = "1. Uchopte horní kladky do obou rukou, stoupněte si doprostřed. 2. Mírně se předkloňte a pokrčte lokty. 3. S výdechem obloukem stahujte ruce k sobě před pas. 4. S nádechem kontrolovaně rozevírejte paže." },
                new Cvik { Nazev = "Woodchopper (Cables)", Kategorie = new List<string> { "Core" }, Popis = "1. Nastavte kladku vysoko na stranu. 2. Uchopte madlo oběma rukama, bokem ke kladce. 3. Rotací trupu stahujte kladku šikmo dolů k protějšímu koleni. 4. Pomalu vraťte zpět stejnou dráhou." },
                new Cvik { Nazev = "Upažování na kladce (Lateral Cable Raise)", Kategorie = new List<string> { "Shoulders" }, Popis = "1. Postavte se bokem ke spodní kladce. 2. Vzdálenější rukou uchopte madlo. 3. Tahejte kladku obloukem do strany a nahoru do výše ramene. 4. Pomalu spusťte ruku před tělo." },
                new Cvik { Nazev = "Zkracovačky na kladce (Cable Crunch)", Kategorie = new List<string> { "Core" }, Popis = "1. Klekněte si zády k horní kladce (nebo čelem), držte lano za hlavou. 2. S výdechem se \"zabalte\" a stáhněte lokty směrem ke kolenům silou břicha. 3. S nádechem se pomalu narovnejte (ale nepovolte břicho úplně)." },
                new Cvik { Nazev = "Stahování horní kladky s nataženýma rukama (Straight Arm Pulldown)", Kategorie = new List<string> { "Back", "Arms" }, Popis = "1. Stůjte čelem ke kladce, uchopte tyč nadhmatem. 2. Mírně se předkloňte, ruce mějte téměř natažené. 3. S výdechem stahujte tyč obloukem dolů ke stehnům (pohyb vychází z ramen a zad). 4. Pomalu vraťte tyč do výše očí." },
                new Cvik { Nazev = "Tricepsové stahování za hlavou (Overhead Cable Ext)", Kategorie = new List<string> { "Arms" }, Popis = "1. Uchopte lano na spodní kladce, otočte se zády a dejte ruce nad hlavu. 2. Lokty směřují ke stropu. 3. Vytlačte lano nahoru do propnutí rukou. 4. Pokrčte lokty a spusťte lano za hlavu." },
                new Cvik { Nazev = "Zanožování na kladce (Cable Kickback)", Kategorie = new List<string> { "Glutes", "Legs" }, Popis = "1. Připněte si adaptér na kotník, postavte se čelem ke kladce. 2. Držte se stroje pro stabilitu. 3. Zanožte nohu dozadu a nahoru, zatněte hýždě. 4. Vraťte nohu k druhé noze." },
                new Cvik { Nazev = "Spodní protisměrné kladky (Low Cable Crossover)", Kategorie = new List<string> { "Chest" }, Popis = "1. Nastavte kladky úplně dolů. 2. Stoupněte si doprostřed, vykročte a mírně se předkloňte. 3. S výdechem táhněte kladky obloukem nahoru před hrudník (zaměření na horní prsa). 4. S nádechem pomalu spouštějte zpět dolů." },
                new Cvik { Nazev = "Stahování horních kladek (High Cable Crossover)", Kategorie = new List<string> { "Chest" }, Popis = "1. Nastavte kladky co nejvýše. 2. Stoupněte si doprostřed a předkloňte se více než u klasických protisměrných kladek. 3. S výdechem stahujte kladky směrem dolů k pasu (zaměření na spodní prsa). 4. S nádechem kontrolovaně vracejte nahoru." },
                new Cvik { Nazev = "Tlak na prsa s kladkou jednoruč (Single Arm Cable Press)", Kategorie = new List<string> { "Chest", "Arms" }, Popis = "1. Nastavte kladku do výše ramen. 2. Postavte se zády ke kladce ve výpadu. 3. Jednou rukou uchopte madlo u ramene a s výdechem ho vytlačte vpřed. 4. S nádechem pomalu vracejte k rameni, udržujte pevný střed." },
                new Cvik { Nazev = "Běh na páse (Treadmill)", Kategorie = new List<string> { "Cardio", "Legs" }, Popis = "1. Nastavte si rychlost a sklon pásu. 2. Běžte vzpřímeně, nedržte se madel. 3. Došlapujte na střed chodidla. 4. Dýchejte pravidelně a držte tempo." },
                new Cvik { Nazev = "Veslování (Rowing Machine)", Kategorie = new List<string> { "Cardio", "Back", "Legs" }, Popis = "1. Odrazte se nohama. 2. Následně zakloňte trup. 3. Nakonec přitáhněte madlo k břichu. 4. Vraťte se v opačném pořadí (ruce, trup, nohy)." },
                new Cvik { Nazev = "Eliptický trenažér", Kategorie = new List<string> { "Cardio" }, Popis = "1. Postavte se na pedály a chyťte madla. 2. Plynulým pohybem střídejte nohy a zapojujte i paže. 3. Držte vzpřímený postoj, nehrbte se. 4. Udržujte konstantní odpor a rychlost." },
                new Cvik { Nazev = "Assault Bike (Air Bike)", Kategorie = new List<string> { "Cardio", "Arms", "Legs" }, Popis = "1. Posaďte se a chytněte madla. 2. Začněte šlapat a zároveň tlačit a tahat rukama. 3. Čím rychleji jedete, tím větší je odpor vzduchu. 4. Snažte se o maximální intenzitu." },
                new Cvik { Nazev = "Schody (Stair Climber)", Kategorie = new List<string> { "Cardio", "Glutes", "Legs" }, Popis = "1. Nastavte rychlost schodů. 2. Kráčejte vzhůru, snažte se příliš neopírat o madla. 3. Došlapujte na celé chodidlo, abyste zapojili hýždě. 4. Udržujte vzpřímený trup." },
                new Cvik { Nazev = "Skákání přes švihadlo", Kategorie = new List<string> { "Cardio", "Legs" }, Popis = "1. Držte lokty u těla, pohyb vychází ze zápěstí. 2. Skákejte nízko, jen na špičkách. 3. Udržujte rytmus a lehké dopady. 4. Střídejte snožmo, střídavě nebo vajíčko." },
                new Cvik { Nazev = "Kettlebell Swing", Kategorie = new List<string> { "Cardio", "Glutes", "Legs", "Core" }, Popis = "1. Rozkročte se, kettlebell je na zemi před vámi. 2. S rovnými zády ho uchopte a švihněte mezi nohy. 3. Explozivním pohybem boků (jako u skoku) vystřelte kettlebell před sebe do výše očí. 4. Nechte ho volně spadnout zpět mezi nohy a opakujte." },
                new Cvik { Nazev = "Jumping Jacks (Panák)", Kategorie = new List<string> { "Cardio", "Full Body" }, Popis = "1. Stůjte snožmo, ruce podél těla. 2. Výskokem roznožte a zároveň tleskněte rukama nad hlavou. 3. Výskokem se vraťte do snožného postoje s rukama u těla. 4. Opakujte v rychlém tempu." },
                new Cvik { Nazev = "Vysoká kolena (High Knees)", Kategorie = new List<string> { "Cardio", "Legs" }, Popis = "1. Běžte na místě. 2. Zvedejte kolena co nejvýše, ideálně do úrovně pasu. 3. Zapojte i pohyb paží. 4. Udržujte vysokou frekvenci kroků." },
                new Cvik { Nazev = "Battle Ropes (Lana)", Kategorie = new List<string> { "Cardio", "Full Body" }, Popis = "1. Stůjte v podřepu, v každé ruce konec tlustého lana. 2. Střídavě nebo soupažně vlněte lany co nejrychleji. 3. Udržujte pevný střed těla. 4. Vydržte pracovat po nastavený časový interval." }
            };

            foreach (var c in vychoziCviky)
            {
                _db.SaveCvik(c);
            }
        }
    }
}