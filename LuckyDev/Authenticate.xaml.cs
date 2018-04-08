using System.Windows;
using System.Windows.Input;

namespace LuckyDev
{
    public partial class Authenticate
    {
        public Authenticate(string user)
        {
            InitializeComponent();
            
            if (user != string.Empty)
            {
                txtUser.Text = user;
                txtPassword.Focus();
            }
            else
            {
                txtUser.Focus();
            }
        }

        public string Password => txtPassword.Password;

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            CheckNullPassword();
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

        private void Key_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckNullPassword();
            }
        }
    }
}