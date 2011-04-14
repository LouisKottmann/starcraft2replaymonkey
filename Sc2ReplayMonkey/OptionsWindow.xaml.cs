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

        IConfig m_Config = null;
    }
}
