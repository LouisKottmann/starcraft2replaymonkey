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
