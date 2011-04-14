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
using System.Collections.ObjectModel;

namespace Sc2ReplayMonkey
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SkinSelectorWindow : Window
    {
        public SkinSelectorWindow(MainWindow main)
        {
            if (base.Resources.MergedDictionaries.Count > 0)
            {
                base.Resources.MergedDictionaries.Clear();
            }
            base.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"Sc2ReplayMonkey;;;component\Resources/Skins/BaseSkin.xaml", UriKind.Relative)) as ResourceDictionary);            
            InitializeComponent();
            m_Main = main;
        }
        private void buttonZergSkin_Click(object sender, RoutedEventArgs e)
        {
            Collection<ResourceDictionary> mergedDicts = m_Main.Resources.MergedDictionaries;
            if (mergedDicts.Count > 0)
            {
                mergedDicts.Clear();
            }
            mergedDicts.Add((ResourceDictionary)Application.LoadComponent(new Uri(@"Sc2ReplayMonkey;;;component\Resources/Skins/ZergSkin.xaml", UriKind.Relative)));
            this.Close();
        }

        MainWindow m_Main = null;
    }
}
