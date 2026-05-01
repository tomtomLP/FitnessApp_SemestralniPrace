using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FitnessApp.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentPage = new DashboardViewModel();

        public void ZobrazDashboard()
        {
            CurrentPage = new DashboardViewModel();
        }

        public void ZibrazHistorii()
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