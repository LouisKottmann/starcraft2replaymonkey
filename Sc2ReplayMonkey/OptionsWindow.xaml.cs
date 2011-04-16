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
using Microsoft.Win32;
using TheUpsideDownLemur;

namespace Sc2ReplayMonkey
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow(MainWindow main)
        {
            Resources = main.Resources;
            InitializeComponent();

            m_Config = IoC.FindMonkey<IConfig>();

            textBoxRelocatePath.Text = m_Config.RelocatePath;
            checkBoxAutoRelocate.IsChecked = m_Config.AutoRelocate;
        }

        private void buttonRelocatePath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = fbDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                m_Config.RelocatePath = fbDialog.SelectedPath;
                textBoxRelocatePath.Text = m_Config.RelocatePath;
            }
        }

        private void checkBoxAutoRelocate_Checked(object sender, RoutedEventArgs e)
        {
            m_Config.AutoRelocate = true;
        }

        private void checkBoxAutoRelocate_Unchecked(object sender, RoutedEventArgs e)
        {
            m_Config.AutoRelocate = false;
        }

        private void buttonCloseOptionsWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void checkBoxFullDelete_Checked(object sender, RoutedEventArgs e)
        {
            m_Config.FullDelete = true;
        }

        private void checkBoxFullDelete_Unchecked(object sender, RoutedEventArgs e)
        {
            m_Config.FullDelete = false;
        }

        IConfig m_Config = null;
    }
}
