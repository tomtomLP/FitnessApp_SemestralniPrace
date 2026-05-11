using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FitnessApp.Views
{
    public partial class ExercisesView : UserControl
    {
        public ExercisesView()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}