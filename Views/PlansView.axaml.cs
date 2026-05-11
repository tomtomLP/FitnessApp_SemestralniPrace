using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FitnessApp.Views
{
    public partial class PlansView : UserControl
    {
        public PlansView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}