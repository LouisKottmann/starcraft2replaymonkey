using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheMonkeyWhoPlayedWithFire;
using System.ComponentModel;

namespace Sc2ReplayMonkey
{
    /// <summary>
    /// Interaction logic for PleaseWaitWindow.xaml
    /// </summary>
    public partial class PleaseWaitWindow : Window
    {
        public PleaseWaitWindow(IFileHandlingBaboon iFileHandlingBaboon, List<String> replaysToParse, MainWindow main)
        {
            m_IFileHandlingBaboon = iFileHandlingBaboon;
            m_ReplaysToParse = replaysToParse;

            Resources = main.Resources;
            InitializeComponent();  
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            worker.DoWork -= new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            this.Close();  
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            m_IFileHandlingBaboon.HandleFiles(m_ReplaysToParse);
        }

        BackgroundWorker worker = new BackgroundWorker();
        IFileHandlingBaboon m_IFileHandlingBaboon = null;
        List<String> m_ReplaysToParse = new List<String>();
    }
}
