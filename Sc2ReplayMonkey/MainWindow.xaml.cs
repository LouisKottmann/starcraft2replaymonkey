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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using TheUpsideDownLemur;
using TheMonkeyWhoPlayedWithFire;
using SC2ParserApe;
using Microsoft.Win32;
using System.Diagnostics;

namespace Sc2ReplayMonkey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (base.Resources.MergedDictionaries.Count > 0)
            {
                base.Resources.MergedDictionaries.Clear();
            }
            base.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"Sc2ReplayMonkey;;;component\Resources/Skins/BaseSkin.xaml", UriKind.Relative)) as ResourceDictionary);
            base.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri(@"Sc2ReplayMonkey;;;component\Resources/Skins/ZergSkin.xaml", UriKind.Relative)));

            InitializeComponent();

            m_IMonkeyDeserializer = IoC.FindMonkey<IMonkeyDeserializer>();
            m_IFileHandlingBaboon = IoC.FindMonkey<IFileHandlingBaboon>();
            m_IConfig = IoC.FindMonkey<IConfig>();
            CurrentDataParsed = m_IMonkeyDeserializer.CurrentReplayData;
            m_Logic = new ControlsLogic(this);
            m_Logic.RefreshListBox();
        }

        private void buttonSkin_Click(object sender, RoutedEventArgs e)
        {
            SkinSelectorWindow skinSelector = new SkinSelectorWindow(this);
            skinSelector.Owner = this;
            skinSelector.ShowDialog();
        }

        private void buttonAddFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Sc2 Replays|*.sc2replay";
            dialog.Multiselect = true;
            dialog.Title = "Select the replays you want to add";
            Nullable<bool> result = dialog.ShowDialog(this);

            if (result == true)
            {
                PleaseWaitWindow waitWindow = new PleaseWaitWindow(m_IFileHandlingBaboon, dialog.FileNames.ToList(), this);
                waitWindow.Owner = this;
                waitWindow.ShowDialog();
            }

            m_Logic.RefreshListBox();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Ensures that no ApeWriter processes remain when the main window is closed.
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.StartsWith("TheApeWriter"))
                {
                    clsProcess.Kill();
                }
            }

            //Save config.
            m_IConfig.SerializeConfig();
        }

        private void listBoxAvailableReplays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxAvailableReplays.SelectedItem != null)
            {
                m_Logic.UpdateDisplay(m_IFileHandlingBaboon.AvailableReplays[listBoxAvailableReplays.SelectedItem.ToString()]);
            }
        }

        private void buttonShowWinner_Click(object sender, RoutedEventArgs e)
        {
            m_Logic.ShowWinner();
        }

        private void buttonMapImage_Click(object sender, RoutedEventArgs e)
        {
            m_Logic.EnlargeMapImage();
        }

        private void buttonWatchReplay_Click(object sender, RoutedEventArgs e)
        {
            m_Logic.WatchReplay();
        }

        private void buttonRelocateReplay_Click(object sender, RoutedEventArgs e)
        {
            m_Logic.RelocateReplay();
        }

        private void buttonDeleteReplay_Click(object sender, RoutedEventArgs e)
        {
            m_Logic.DeleteReplay();
        }

        private void buttonOptions_Click(object sender, RoutedEventArgs e)
        {
            m_Logic.ShowOptions();
        }

        ParsedData CurrentDataParsed = new ParsedData();
        IMonkeyDeserializer m_IMonkeyDeserializer = null;
        IFileHandlingBaboon m_IFileHandlingBaboon = null;
        IConfig m_IConfig = null;
        ControlsLogic m_Logic = null;

        //Initializes IoC content.        
        MonkeyDeserializer m_MonkeyDeserializer = new MonkeyDeserializer();
        FileHandlingBaboon m_FileHandlingBaboon = new FileHandlingBaboon();
        Config m_Config = new Config();
    }
}
