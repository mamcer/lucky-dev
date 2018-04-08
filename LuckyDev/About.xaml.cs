using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LuckyDev
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        #region constructor

        public About()
        {
            this.InitializeComponent();
        }

        #endregion

        #region private methods

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Storyboard closeAnimation = (Storyboard)FindResource("CloseAnimation");
            closeAnimation.Completed += new EventHandler(this.CloseAnimation_Completed);
            closeAnimation.Begin(this);
        }

        private void CloseAnimation_Completed(object sender, EventArgs e)
        {
            DialogResult = true;
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        #endregion
    }
}