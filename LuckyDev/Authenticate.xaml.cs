using System.Windows;
using System.Windows.Input;

namespace LuckyDev
{
    /// <summary>
    /// Interaction logic for Authenticate.xaml
    /// </summary>
    public partial class Authenticate : Window
    {
        #region constructor

        public Authenticate(string user)
        {
            InitializeComponent();
            
            if (user != string.Empty)
            {
                this.txtUser.Text = user;
                this.txtPassword.Focus();
            }
            else
            {
                this.txtUser.Focus();
            }
        }

        #endregion

        #region public properties
       
        public string Password
        {
            get
            {
                return this.txtPassword.Password;
            }
        }

        #endregion

        #region private methods

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.CheckNullPassword();
        }

        private void CheckNullPassword()
        {
            lblError.Visibility = Visibility.Hidden;
            if (!string.IsNullOrEmpty(txtUser.Text) && !string.IsNullOrEmpty(txtPassword.Password))
            {
                DialogResult = true;
            }
            else
            {
                lblError.Visibility = Visibility.Visible;
            }
        }

        private void Key_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
        }

        private void BtnClose_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DialogResult = false;
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.CheckNullPassword();
            }
        }

        #endregion
    }
}
