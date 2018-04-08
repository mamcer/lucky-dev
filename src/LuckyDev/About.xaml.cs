using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LuckyDev
{
    public partial class About
    {
        public About()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Storyboard closeAnimation = (Storyboard)FindResource("CloseAnimation");
            closeAnimation.Completed += CloseAnimation_Completed;
            closeAnimation.Begin(this);
        }

        private void CloseAnimation_Completed(object sender, EventArgs e)
        {
            DialogResult = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}