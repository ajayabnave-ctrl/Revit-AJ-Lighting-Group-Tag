using System;
using System.Windows;
using System.Windows.Input;

namespace AJ.LightingGroupTag.UI
{
    /// <summary>
    /// Interaction logic for LightingGroupTagWindow.xaml
    /// </summary>
    public partial class LightingGroupTagWindow : Window
    {
        public LightingGroupTagViewModel ViewModel { get; }

        public LightingGroupTagWindow(LightingGroupTagViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            ViewModel.SetWindow(this);
            DataContext = ViewModel;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
