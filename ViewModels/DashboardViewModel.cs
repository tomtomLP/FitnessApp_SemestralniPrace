using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FitnessApp.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FullGreeting))]
        private string _greeting = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FullGreeting))]
        private string _userName = "Uživatel"; // Defaultní hodnota

        [ObservableProperty]
        private string _currentDateText = "";

        public string FullGreeting => $"{Greeting},  {UserName}!";

        public DashboardViewModel()
        {
            NastavDatumACas();
        }

        private void NastavDatumACas()
        {
            var culture = new CultureInfo("cs-CZ");
            string datum = DateTime.Now.ToString("dddd, d. MMMM", culture);

            CurrentDateText = char.ToUpper(datum[0]) + datum.Substring(1);
            
            int hour = DateTime.Now.Hour;
            if (hour >= 5 && hour < 12)
                Greeting = "Dobré ráno";
            else if (hour == 12)
                Greeting = "Dobré poledne";
            else if (hour > 12 && hour < 18)
                Greeting = "Dobré odpoledne";
            else
                Greeting = "Dobrý večer";
        }
    }
}