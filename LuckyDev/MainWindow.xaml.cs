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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Avalon.Windows.Controls;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.Win32;

namespace LuckyDev
{
    public partial class MainWindow : Window
    {
        #region private members

        private string uri;
        private string projectName;
        private TeamFoundationServer server;
        private WorkItemStore store;
        private Project project;
        private CodeReviews codeReviews;
        private BackgroundWorker backgroundWorker;
        private bool showBallonTooltip;
        private RegistryKey runRegistryKey = null;

        #endregion

        #region constructor

        public MainWindow()
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(this.TFSUri) || string.IsNullOrEmpty(this.TFSProjectName))
            {
                MessageBox.Show("Falta la definición de la URI del servidor TFS o el nombre del proyecto TFS en el archivo de configuración.\nLa aplicación no puede continuar", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(0);
            }

            this.server = null;
            this.store = null;
            this.project = null;

            if (this.AuthenticateUser() == false)
            {
                Application.Current.Shutdown(0);
            }
            else
            {
                this.RunReviewBackgroundWorker();
            }

            if (this.RunRegistryKey.GetValue("LuckyDev") == null)
            {
                MIStartWithWindows.IsChecked = false;
            }
            else
            {
                MIStartWithWindows.IsChecked = true;
            }

            this.RunRegistryKey = null;

            this.Loaded += new RoutedEventHandler(this.MainWindow_Loaded);
        }

        #endregion

        #region private properties

        private string TFSUser
        {
            get
            { 
                return ConfigurationManager.AppSettings["TFSUser"];
            }
        }

        private string TFSPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["TFSPassword"];
            }
        }

        private string TFSUri
        {
            get
            {
                if (string.IsNullOrEmpty(this.uri))
                {
                    try
                    {
                        this.uri = ConfigurationManager.AppSettings["TFSUri"];
                    }
                    catch
                    {
                        this.uri = string.Empty;
                    }
                }

                return this.uri;
            }
        }

        private string TFSProjectName
        {
            get
            {
                if (string.IsNullOrEmpty(this.projectName))
                {
                    try
                    {
                        this.projectName = ConfigurationManager.AppSettings["TFSProject"];
                    }
                    catch
                    {
                        this.projectName = string.Empty;
                    }
                }

                return this.projectName;
            }
        }

        private TeamFoundationServer TFSServer
        {
            get
            {
                if (this.server == null)
                {
                    NetworkCredential credentials = this.GetTFSNetworkCredential();
                    if (credentials != null)
                    {
                        this.server = new TeamFoundationServer(this.TFSUri, credentials);
                    }
                    else
                    {
                        this.server = null;
                    }
                }

                return this.server;
            }
        }

        private WorkItemStore TFSItemStore
        {
            get
            {
                if (this.store == null)
                {
                    if (this.TFSServer != null)
                    {
                        this.store = (WorkItemStore)this.TFSServer.GetService(typeof(WorkItemStore));
                    }
                }

                return this.store;
            }
        }

        private Project TFSProject
        {
            get
            {
                if (this.project == null)
                {
                    if (this.TFSItemStore != null)
                    {
                        this.project = this.TFSItemStore.Projects[this.TFSProjectName];
                    }
                }

                return this.project;
            }
        }

        private CodeReviews CodeReviewCollection
        {
            get
            {
                if (this.codeReviews == null)
                {
                    this.codeReviews = new CodeReviews();
                }

                return this.codeReviews;
            }
        }

        private bool ShowBallonTooltip
        {
            get
            {
                return this.showBallonTooltip;
            }

            set
            {
                this.showBallonTooltip = value;
            }
        }

        private RegistryKey RunRegistryKey
        {
            get
            {
                if (this.runRegistryKey == null)
                {
                    this.runRegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                }

                return this.runRegistryKey;
            }

            set
            {
                this.runRegistryKey = value;
            }
        }

        #endregion

        #region private methods

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Properties["ArgName"] != null)
            {
                string parameter = Application.Current.Properties["ArgName"].ToString();

                if (parameter == "/minimized")
                {
                    this.WindowState = WindowState.Minimized;
                    this.ShowInTaskbar = false;
                }
            }
        }

        private void RunReviewBackgroundWorker()
        {
            this.lstNoCodeReviews.Items.Clear();
            this.top01.Content = string.Empty;
            this.top02.Content = string.Empty;
            this.top03.Content = string.Empty;
            this.top04.Content = string.Empty;
            this.top05.Content = string.Empty;
            this.lblWinnerIs.Content = string.Empty;
            this.lblUpdate.IsEnabled = false;
            this.btnLuckyDev.IsEnabled = false;
            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.DoWork += this.BackgroundWorker_DoWork;
            this.backgroundWorker.RunWorkerCompleted += this.BackgroundWorker_RunWorkerCompleted;
            this.backgroundWorker.RunWorkerAsync();
        }
        
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.AddProjectMembers();
            this.RetrieveCodeReviews();
        }

        private void BackgroundWorker_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            this.UpdateReviews();
            this.backgroundWorker.DoWork -= this.BackgroundWorker_DoWork;
            this.backgroundWorker.RunWorkerCompleted -= this.BackgroundWorker_RunWorkerCompleted;
            this.backgroundWorker.Dispose();
        }

        private void UpdateReviews()
        {
            // The Work to perform on another thread
            ThreadStart start = delegate
                                    {
                                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(this.UpdateUI));
                                    };

            new Thread(start).Start();
        }

        private void UpdateUI()
        {
            Dictionary<string, int> topFive = this.CodeReviewCollection.GetTopFive();
            string[] members = topFive.Keys.ToArray();
            if (topFive[members[0]] > 0)
            {
                this.top01.Content = string.Format("{0} ({1})", members[0], topFive[members[0]]);
            }
            else
            {
                this.top01.Content = string.Empty;
            }

            if (topFive[members[1]] > 0)
            {
                this.top02.Content = string.Format("{0} ({1})", members[1], topFive[members[1]]);
            }
            else
            {
                this.top02.Content = string.Empty;
            }

            if (topFive[members[2]] > 0)
            {
                this.top03.Content = string.Format("{0} ({1})", members[2], topFive[members[2]]);
            }
            else
            {
                this.top03.Content = string.Empty;
            }

            if (topFive[members[3]] > 0)
            {
                this.top04.Content = string.Format("{0} ({1})", members[3], topFive[members[3]]);
            }
            else
            {
                this.top04.Content = string.Empty;
            }

            if (topFive[members[4]] > 0)
            {
                this.top05.Content = string.Format("{0} ({1})", members[4], topFive[members[4]]);
            }
            else
            {
                this.top05.Content = string.Empty;
            }

            List<string> noCodeReviews = this.CodeReviewCollection.GetTeamMembersWithoutCodeReviews();
            if (noCodeReviews.Count > 0)
            {
                foreach (string member in noCodeReviews)
                {
                    this.lstNoCodeReviews.Items.Add(member);
                }
            }
            else
            {
                List<string> lessCodeReviews = this.CodeReviewCollection.GetTeamMembersWithLessCodeReviews();
                MessageBox.Show("No existen personas sin code review asignados.\nSe muestran las personas que tienen menos code reviews pendientes", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                foreach (string member in lessCodeReviews)
                {
                    this.lstNoCodeReviews.Items.Add(member);
                }
            }

            this.lblUpdate.IsEnabled = true;
            this.btnLuckyDev.IsEnabled = true;
            if (this.ShowBallonTooltip)
            {
                this.notifyIcon.ShowBalloonTip(10000, "The winner is", this.GiveMeAReviewer(), NotifyBalloonIcon.Info);
                this.ShowBallonTooltip = false;
            }
        }

        private void RetrieveCodeReviews()
        {
            WorkItemCollection reviews = this.RetrieveWorkItems();

            foreach (WorkItem wi in this.RetrieveWorkItems())
            {
                this.CodeReviewCollection.AddCodeReview(wi["Required Attendee 1"].ToString());
            }
        }

        private void AddProjectMembers()
        {
            string projectMembers = ConfigurationManager.AppSettings["ProjectMembers"];
            string[] teamMembers = projectMembers.Split(';');
            foreach (string teamMember in teamMembers)
            {
                this.CodeReviewCollection.AddProjectMember(teamMember);
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

        private NetworkCredential GetTFSNetworkCredential()
        {
            string user = this.TFSUser;
            string password = this.TFSPassword;

            if (string.IsNullOrEmpty(password))
            {
                // si ingresa 3 veces mal el password, chau
                for (int i = 0; i < 3 && string.IsNullOrEmpty(password); i++)
                {
                    password = this.GiveMeAValidPassword(user);
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
                this.TFSServer.Authenticate();
            }
            catch
            {
                MessageBox.Show("Usuario o contraseña inválido", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void BtnLuckyDev_Click(object sender, RoutedEventArgs e)
        {
            this.lblWinnerIs.Content = this.GiveMeAReviewer() + " ";
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
            Hashtable parameters = this.BuildParameters();
            string query = this.BuildQuery();
            return this.TFSItemStore.Query(query, parameters);
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
            Hashtable parameters = new Hashtable();
            parameters.Add("project", this.TFSProject.Name);
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
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void LblAbout_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.ShowAboutBox();
        }

        private void ShowAboutBox()
        {
            About about = new About();
            this.Effect = new System.Windows.Media.Effects.BlurEffect();
            about.ShowDialog();
            this.Effect = null;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void LblUpdate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.CodeReviewCollection.ResetCodeReviews();
            this.RunReviewBackgroundWorker();
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.CodeReviewCollection.ResetCodeReviews();
                this.RunReviewBackgroundWorker();
                this.WindowState = WindowState.Normal;
                this.ShowInTaskbar = true;
            }
        }

        private void BtnToSystray_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        private void MIShow_Click(object sender, RoutedEventArgs e)
        {
            this.ShowMainWindow();
        }

        private void MIAbout_Click(object sender, RoutedEventArgs e)
        {
            this.ShowAboutBox();
        }

        private void MIReviewer_Click(object sender, RoutedEventArgs e)
        {
            this.CodeReviewCollection.ResetCodeReviews();
            this.RunReviewBackgroundWorker();
            this.ShowBallonTooltip = true;
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
                this.RunRegistryKey.SetValue("LuckyDev", "\"" + Assembly.GetExecutingAssembly().Location + "\" /minimized");
            }
            else
            {
                this.RunRegistryKey.DeleteValue("LuckyDev", false);
            }
        }
        
        #endregion
    }
}
