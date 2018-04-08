using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Avalon.Windows.Controls;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.Win32;

namespace LuckyDev
{
    public partial class MainWindow
    {
        private string _uri;
        private string _projectName;
        private TeamFoundationServer _server;
        private WorkItemStore _store;
        private Project _project;
        private CodeReviews _codeReviews;
        private BackgroundWorker _backgroundWorker;
        private RegistryKey _runRegistryKey;

        public MainWindow()
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(TfsUri) || string.IsNullOrEmpty(TfsProjectName))
            {
                MessageBox.Show("Falta la definición de la URI del servidor TFS o el nombre del proyecto TFS en el archivo de configuración.\nLa aplicación no puede continuar", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(0);
            }

            _server = null;
            _store = null;
            _project = null;

            if (!AuthenticateUser())
            {
                Application.Current.Shutdown(0);
            }
            else
            {
                RunReviewBackgroundWorker();
            }

            if (RunRegistryKey.GetValue("LuckyDev") == null)
            {
                MIStartWithWindows.IsChecked = false;
            }
            else
            {
                MIStartWithWindows.IsChecked = true;
            }

            RunRegistryKey = null;

            Loaded += MainWindow_Loaded;
        }

        private static string TfsUser => ConfigurationManager.AppSettings["TFSUser"];

        private static string TfsPassword => ConfigurationManager.AppSettings["TFSPassword"];

        private string TfsUri
        {
            get
            {
                if (string.IsNullOrEmpty(_uri))
                {
                    try
                    {
                        _uri = ConfigurationManager.AppSettings["TFSUri"];
                    }
                    catch
                    {
                        _uri = string.Empty;
                    }
                }

                return _uri;
            }
        }

        private string TfsProjectName
        {
            get
            {
                if (string.IsNullOrEmpty(_projectName))
                {
                    try
                    {
                        _projectName = ConfigurationManager.AppSettings["TFSProject"];
                    }
                    catch
                    {
                        _projectName = string.Empty;
                    }
                }

                return _projectName;
            }
        }

        private TeamFoundationServer TfsServer
        {
            get
            {
                if (_server == null)
                {
                    NetworkCredential credentials = GetTfsNetworkCredential();
                    if (credentials != null)
                    {
                        _server = new TeamFoundationServer(TfsUri, credentials);
                    }
                    else
                    {
                        _server = null;
                    }
                }

                return _server;
            }
        }

        private WorkItemStore TfsItemStore
        {
            get
            {
                if (_store == null && TfsServer != null)
                {
                    _store = (WorkItemStore) TfsServer.GetService(typeof(WorkItemStore));
                }

                return _store;
            }
        }

        private Project TfsProject
        {
            get
            {
                if (_project == null && TfsItemStore != null)
                {
                    _project = TfsItemStore.Projects[TfsProjectName];
                }

                return _project;
            }
        }

        private CodeReviews CodeReviewCollection
        {
            get
            {
                if (_codeReviews == null)
                {
                    _codeReviews = new CodeReviews();
                }

                return _codeReviews;
            }
        }

        private bool ShowBallonTooltip { get; set; }

        private RegistryKey RunRegistryKey
        {
            get
            {
                if (_runRegistryKey == null)
                {
                    _runRegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                }

                return _runRegistryKey;
            }
            set => _runRegistryKey = value;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Properties["ArgName"] != null)
            {
                string parameter = Application.Current.Properties["ArgName"].ToString();

                if (parameter == "/minimized")
                {
                    WindowState = WindowState.Minimized;
                    ShowInTaskbar = false;
                }
            }
        }

        private void RunReviewBackgroundWorker()
        {
            lstNoCodeReviews.Items.Clear();
            top01.Content = string.Empty;
            top02.Content = string.Empty;
            top03.Content = string.Empty;
            top04.Content = string.Empty;
            top05.Content = string.Empty;
            lblWinnerIs.Content = string.Empty;
            lblUpdate.IsEnabled = false;
            btnLuckyDev.IsEnabled = false;
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync();
        }
        
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            AddProjectMembers();
            RetrieveCodeReviews();
        }

        private void BackgroundWorker_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            UpdateReviews();
            _backgroundWorker.DoWork -= BackgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted -= BackgroundWorker_RunWorkerCompleted;
            _backgroundWorker.Dispose();
        }

        private void UpdateReviews()
        {
            // The Work to perform on another thread
            void Start()
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(UpdateUi));
            }

            new Thread(Start).Start();
        }

        private void UpdateUi()
        {
            Dictionary<string, int> topFive = CodeReviewCollection.GetTopFive();
            string[] members = topFive.Keys.ToArray();
            if (topFive[members[0]] > 0)
            {
                top01.Content = $"{members[0]} ({topFive[members[0]]})";
            }
            else
            {
                top01.Content = string.Empty;
            }

            if (topFive[members[1]] > 0)
            {
                top02.Content = $"{members[1]} ({topFive[members[1]]})";
            }
            else
            {
                top02.Content = string.Empty;
            }

            if (topFive[members[2]] > 0)
            {
                top03.Content = $"{members[2]} ({topFive[members[2]]})";
            }
            else
            {
                top03.Content = string.Empty;
            }

            if (topFive[members[3]] > 0)
            {
                top04.Content = $"{members[3]} ({topFive[members[3]]})";
            }
            else
            {
                top04.Content = string.Empty;
            }

            if (topFive[members[4]] > 0)
            {
                top05.Content = $"{members[4]} ({topFive[members[4]]})";
            }
            else
            {
                top05.Content = string.Empty;
            }

            List<string> noCodeReviews = CodeReviewCollection.GetTeamMembersWithoutCodeReviews();
            if (noCodeReviews.Count > 0)
            {
                foreach (string member in noCodeReviews)
                {
                    lstNoCodeReviews.Items.Add(member);
                }
            }
            else
            {
                List<string> lessCodeReviews = CodeReviewCollection.GetTeamMembersWithLessCodeReviews();
                MessageBox.Show("No existen personas sin code review asignados.\nSe muestran las personas que tienen menos code reviews pendientes", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                foreach (string member in lessCodeReviews)
                {
                    lstNoCodeReviews.Items.Add(member);
                }
            }

            lblUpdate.IsEnabled = true;
            btnLuckyDev.IsEnabled = true;
            if (ShowBallonTooltip)
            {
                notifyIcon.ShowBalloonTip(10000, "The winner is", GiveMeAReviewer(), NotifyBalloonIcon.Info);
                ShowBallonTooltip = false;
            }
        }

        private void RetrieveCodeReviews()
        {
            RetrieveWorkItems();

            foreach (WorkItem wi in RetrieveWorkItems())
            {
                CodeReviewCollection.AddCodeReview(wi["Required Attendee 1"].ToString());
            }
        }

        private void AddProjectMembers()
        {
            string projectMembers = ConfigurationManager.AppSettings["ProjectMembers"];
            string[] teamMembers = projectMembers.Split(';');
            foreach (string teamMember in teamMembers)
            {
                CodeReviewCollection.AddProjectMember(teamMember);
            }
        }

        private string GiveMeAValidPassword(string user)
        {
            Authenticate authenticateWindow = new Authenticate(user);
            if (authenticateWindow.ShowDialog() == true)
            {
                return authenticateWindow.Password;
            }
            else
            {
                return string.Empty;
            }
        }

        private NetworkCredential GetTfsNetworkCredential()
        {
            string user = TfsUser;
            string password = TfsPassword;

            if (string.IsNullOrEmpty(password))
            {
                // si ingresa 3 veces mal el password, chau
                for (int i = 0; i < 3 && string.IsNullOrEmpty(password); i++)
                {
                    password = GiveMeAValidPassword(user);
                }
            }

            if (!string.IsNullOrEmpty(password))
            {
                return new NetworkCredential(user, password, "[domain]");
            }
            else
            {
                return null;
            }
        }

        private bool AuthenticateUser()
        {
            try
            {
                TfsServer.Authenticate();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Usuario o contraseña inválido" + ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void BtnLuckyDev_Click(object sender, RoutedEventArgs e)
        {
            lblWinnerIs.Content = GiveMeAReviewer() + " ";
        }

        private string GiveMeAReviewer()
        {
            if (lstNoCodeReviews.Items.Count > 0)
            {
                Random random = new Random();
                int selectedMember = random.Next(0, lstNoCodeReviews.Items.Count);

                return lstNoCodeReviews.Items[selectedMember].ToString();
            }
            else
            {
                return "Nadie disponible, así que... porque no te lo asignas a vos? ";
            }
        }

        private WorkItemCollection RetrieveWorkItems()
        {
            Hashtable parameters = BuildParameters();
            string query = BuildQuery();
            return TfsItemStore.Query(query, parameters);
        }

        private string BuildQuery()
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT [System.Id], [System.Title], [Microsoft.VSTS.CMMI.RequiredAttendee1], [Conchango.VSTS.Scrum.SprintNumber] ");
            queryBuilder.Append("FROM WorkItems ");
            queryBuilder.Append("WHERE [System.TeamProject] = @project ");
            queryBuilder.Append("AND  [System.WorkItemType] = 'Sprint Backlog Item' ");
            queryBuilder.Append("AND  [System.State] = 'Pending Review' ");
            queryBuilder.Append("ORDER BY [Microsoft.VSTS.CMMI.RequiredAttendee1]");
            return queryBuilder.ToString();
        }

        private Hashtable BuildParameters()
        {
            Hashtable parameters = new Hashtable {{"project", TfsProject.Name}};
            return parameters;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void LblAbout_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowAboutBox();
        }

        private void ShowAboutBox()
        {
            About about = new About();
            Effect = new System.Windows.Media.Effects.BlurEffect();
            about.ShowDialog();
            Effect = null;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void LblUpdate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CodeReviewCollection.ResetCodeReviews();
            RunReviewBackgroundWorker();
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            if (WindowState == WindowState.Minimized)
            {
                CodeReviewCollection.ResetCodeReviews();
                RunReviewBackgroundWorker();
                WindowState = WindowState.Normal;
                ShowInTaskbar = true;
            }
        }

        private void BtnToSystray_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
        }

        private void MIShow_Click(object sender, RoutedEventArgs e)
        {
            ShowMainWindow();
        }

        private void MIAbout_Click(object sender, RoutedEventArgs e)
        {
            ShowAboutBox();
        }

        private void MIReviewer_Click(object sender, RoutedEventArgs e)
        {
            CodeReviewCollection.ResetCodeReviews();
            RunReviewBackgroundWorker();
            ShowBallonTooltip = true;
        }

        private void MIExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void StartWithWindows_Click(object sender, RoutedEventArgs e)
        {
            bool isRunAtStartupChecked = MIStartWithWindows.IsChecked;
            if (isRunAtStartupChecked)
            {
                RunRegistryKey.SetValue("LuckyDev", "\"" + Assembly.GetExecutingAssembly().Location + "\" /minimized");
            }
            else
            {
                RunRegistryKey.DeleteValue("LuckyDev", false);
            }
        }
    }
}
