using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitnessApp.Models;

namespace FitnessApp.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentPage;
        
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly HistoryViewModel _historyViewModel;
        private readonly WorkoutViewModel _workoutViewModel;
        private readonly PlansViewModel _plansViewModel;
        private readonly ExercisesViewModel _exercisesViewModel;
        private readonly SettingsViewModel _settingsViewModel;

        public MainWindowViewModel()
        {
            _dashboardViewModel = new DashboardViewModel();
            _historyViewModel = new HistoryViewModel();
            _workoutViewModel = new WorkoutViewModel();
            _plansViewModel = new PlansViewModel();
            _exercisesViewModel = new ExercisesViewModel();
            _settingsViewModel = new SettingsViewModel();
            
            CurrentPage = _dashboardViewModel; // Defaultní stránka
            
            // Propojení tlačítek z dashboardu
            
            _dashboardViewModel.ProfilZadan = ZobrazNastaveni;
            _dashboardViewModel.HistorieZadana = ZobrazHistorii;
            
            _dashboardViewModel.VolnyTreninkZadan = () => 
            {
                ZobrazTrenink();
                _workoutViewModel.SpustitVolnyTreninkCommand.Execute(null); 
            };
            
            _dashboardViewModel.PlanZadan = (plan) => 
            {
                ZobrazTrenink();
                _workoutViewModel.SpustitPlanCommand.Execute(plan);
            };
        }

        // Commandy pro tlačítka vlevo

        [RelayCommand]
        private void ZobrazDashboard()
        {
            _dashboardViewModel.NactiData();
            CurrentPage = _dashboardViewModel;
        }

        [RelayCommand]
        private void ZobrazHistorii()
        {
            _historyViewModel.NactiHistorii();
            CurrentPage = _historyViewModel;
        }

        [RelayCommand]
        private void ZobrazTrenink()
        {
            _workoutViewModel.NactiPlany();
            CurrentPage = _workoutViewModel;
        }

        [RelayCommand]
        private void ZobrazPlany()
        {
            _plansViewModel.NactiPlany();
            CurrentPage = _plansViewModel;
        }

        [RelayCommand]
        private void ZobrazCviky()
        {
            CurrentPage = _exercisesViewModel;
        }

        [RelayCommand]
        private void ZobrazNastaveni()
        {
            _settingsViewModel.NactiData();
            CurrentPage = _settingsViewModel;
        }
    }
}