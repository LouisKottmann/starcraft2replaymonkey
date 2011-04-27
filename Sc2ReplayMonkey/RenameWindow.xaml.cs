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
using TheUpsideDownLemur;

namespace Sc2ReplayMonkey
{
    /// <summary>
    /// Interaction logic for RenameWindow.xaml
    /// </summary>
    public partial class RenameWindow : Window
    {
        public RenameWindow(MainWindow main, MidLevelLogic midLevelLogic)
        {
            m_MidLevelLogic = midLevelLogic;
            Resources = main.Resources;
            InitializeComponent();

            m_Config = IoC.FindMonkey<IConfig>();

            textBoxRename.Text = m_Config.RenameFormat;
            InitComboFormatType();
        }

        private void InitComboFormatType()
        {
            switch (m_Config.ChosenRenamingType)
            {
                case RenamingType.Personalized:
                    comboRenameStyle.SelectedIndex = 0;
                    expanderKeywords.IsExpanded = false;
                    break;
                case RenamingType.Formatted1:
                    comboRenameStyle.SelectedIndex = 1;
                    expanderKeywords.IsExpanded = false;
                    break;
                case RenamingType.Formatted2:
                    comboRenameStyle.SelectedIndex = 2;
                    expanderKeywords.IsExpanded = false;
                    break;
                case RenamingType.Formatted3:
                    comboRenameStyle.SelectedIndex = 3;
                    expanderKeywords.IsExpanded = false;
                    break;
                case RenamingType.Custom:
                    comboRenameStyle.SelectedIndex = 4;
                    expanderKeywords.IsExpanded = true;
                    break;
                default:
                    expanderKeywords.IsExpanded = false;
                    break;
            }
        }

        private void buttonGeneratePreview_Click(object sender, RoutedEventArgs e)
        {
            String newName = String.Empty;
            if (m_MidLevelLogic.GenerateNewName(out newName))
            {
                labelPreview.Content = newName;
            }
        }

        private void buttonRename_Click(object sender, RoutedEventArgs e)
        {
            String newReplayName = String.Empty;
            if (m_MidLevelLogic.GenerateNewName(out newReplayName))
            {
                if (!String.IsNullOrEmpty(newReplayName))
                {
                    m_MidLevelLogic.CallRelocate(newReplayName);
                }
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void comboRenameStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (comboRenameStyle.SelectedIndex)
            {
                case 0:
                    m_Config.ChosenRenamingType = RenamingType.Personalized;
                    expanderKeywords.IsExpanded = false;
                    break;
                case 1:
                    m_Config.ChosenRenamingType = RenamingType.Formatted1;
                    expanderKeywords.IsExpanded = false;
                    break;
                case 2:
                    m_Config.ChosenRenamingType = RenamingType.Formatted2;
                    expanderKeywords.IsExpanded = false;
                    break;
                case 3:
                    m_Config.ChosenRenamingType = RenamingType.Formatted3;
                    expanderKeywords.IsExpanded = false;
                    break;
                case 4:
                    m_Config.ChosenRenamingType = RenamingType.Custom;
                    expanderKeywords.IsExpanded = true;
                    break;
                default:
                    m_Config.ChosenRenamingType = RenamingType.None;
                    expanderKeywords.IsExpanded = false;
                    break;
            }
        }

        private void textBoxRename_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_Config.RenameFormat = textBoxRename.Text;
        }

        private IConfig m_Config = null;
        private MidLevelLogic m_MidLevelLogic = null;
    }
}
