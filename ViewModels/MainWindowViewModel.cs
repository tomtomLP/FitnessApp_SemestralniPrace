using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FitnessApp.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentPage = new DashboardViewModel(); // Výchozí stránka po zapnutí

        // Metody pro tlačítka vlevo
        public void ZobrazDashboard()
        {
            CurrentPage = new DashboardViewModel();
        }
        
        public void ZobrazHistorii()
        {
            CurrentPage = new HistoryViewModel();
        }
        
        public void ZobrazTrenink()
        {
            CurrentPage = new WorkoutViewModel();
        }
        
        public void ZobrazPlany()
        {
            CurrentPage = new PlansViewModel();
        }
        
        public void ZobrazCviky()
        {
            CurrentPage = new ExercisesViewModel();
        }
        
        public void ZobrazNastaveni()
        {
            CurrentPage = new SettingsViewModel();
        }
    }
}