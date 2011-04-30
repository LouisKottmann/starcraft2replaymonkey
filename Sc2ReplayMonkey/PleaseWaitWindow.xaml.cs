/*This file is part of Sc2ReplayMonkey
    Sc2ReplayMonkey is a starcraft II replay analyzer tool built upon SC2PArserApe  
 
    Copyright (C) 2011  Louis Kottmann louis.kottmann@gmail.com

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.DoWork -= new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            this.Close();  
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                m_IFileHandlingBaboon.HandleFiles(m_ReplaysToParse);
            }
            catch (Exception except)
            {
                MessageBox.Show("Error encoutered: " + except.Message + "\nStackTrace: " + except.StackTrace, "Error while parsing the replay");
            }
        }

        IFileHandlingBaboon m_IFileHandlingBaboon = null;
        List<String> m_ReplaysToParse = new List<String>();
    }
}
