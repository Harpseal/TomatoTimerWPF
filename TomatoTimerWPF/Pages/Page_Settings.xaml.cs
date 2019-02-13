using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Text.RegularExpressions;

namespace TomatoTimerWPF
{
    /// <summary>
    /// Interaction logic for Page_Settings.xaml
    /// </summary>
    public partial class Page_Settings : UserControl
    {
        private bool m_bVersionMode;
        private MainWindow m_window;
        public void SetMainWindow(MainWindow window)
        {
            m_window = window;
        }

        public Page_Settings(MainWindow  window)
        {
            InitializeComponent();

            m_bVersionMode = true;
            labelVersion_small.Content = "Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

            m_window = window;
            SyncSettingsToUIValue();
            cbRelaxTimeComboBox.SelectionChanged += ComboBox_SelectionChanged;
            cbWorkTimeComboBox.SelectionChanged += ComboBox_SelectionChanged;
            cbLongRelaxTimeComboBox.SelectionChanged += ComboBox_SelectionChanged;
        }

        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        private void Button_GotoButtons_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SyncUIValueToSettings();
            m_window.SwitchToButtons();
            //Switcher.Switch(new Page_Buttons());
            
        }
        #endregion

        private void SyncSettingsToUIValue()
        {
            TextBox tbox;

            tbox = cbWorkTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                tbox.Text = "" + TomatoTimerWPF.TimerSettings.Default.WorkTime_Uesr;

            tbox = cbRelaxTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                tbox.Text = "" + TomatoTimerWPF.TimerSettings.Default.RelaxTime_Uesr;

            tbox = cbLongRelaxTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                tbox.Text = "" + TomatoTimerWPF.TimerSettings.Default.Relax_Time_Long_User;


            if (TomatoTimerWPF.TimerSettings.Default.Work_Time == 30)
                cbWorkTimeComboBox.SelectedIndex = 0;
            else if (TomatoTimerWPF.TimerSettings.Default.Work_Time == 25)
                cbWorkTimeComboBox.SelectedIndex = 1;
            else
            {
                cbWorkTimeComboBox.SelectedIndex = 2;
                tbox = cbWorkTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    tbox.Text = "" + TomatoTimerWPF.TimerSettings.Default.Work_Time;
                }
            }

            if (TomatoTimerWPF.TimerSettings.Default.Relax_Time == 10)
                cbRelaxTimeComboBox.SelectedIndex = 0;
            else if (TomatoTimerWPF.TimerSettings.Default.Relax_Time == 5)
                cbRelaxTimeComboBox.SelectedIndex = 1;
            else
            {
                cbRelaxTimeComboBox.SelectedIndex = 2;
                tbox = cbRelaxTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    tbox.Text = "" + TomatoTimerWPF.TimerSettings.Default.Relax_Time;
                }
            }

            if (TomatoTimerWPF.TimerSettings.Default.Relax_Time_Long == 30)
                cbLongRelaxTimeComboBox.SelectedIndex = 0;
            else if (TomatoTimerWPF.TimerSettings.Default.Relax_Time_Long == 15)
                cbLongRelaxTimeComboBox.SelectedIndex = 1;
            else
            {
                cbLongRelaxTimeComboBox.SelectedIndex = 2;
                tbox = cbLongRelaxTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    tbox.Text = "" + TomatoTimerWPF.TimerSettings.Default.Relax_Time_Long;
                }
            }

            tbGoogleCal_src.Text = TomatoTimerWPF.TimerSettings.Default.GoogleCal_src;
            tbGoogleCal_text.Text = TomatoTimerWPF.TimerSettings.Default.GoogleCal_text;
            cbEnableGCal.IsChecked = TomatoTimerWPF.TimerSettings.Default.GoogleCal_EnableEvent;

            if (cbEnableGCal.IsChecked == true)
            {
                spGoogleEventTitle.IsEnabled = true;
                spGoogleID.IsEnabled = true;
                menuTestGoogleCal.IsEnabled = true;
            }
            else
            {
                spGoogleEventTitle.IsEnabled = false;
                spGoogleID.IsEnabled = false;
                menuTestGoogleCal.IsEnabled = false;
            }
        }

        private void SyncUIValueToSettings()
        {
            TextBox tbox;
            if (cbRelaxTimeComboBox.SelectedIndex == 0)
                TomatoTimerWPF.TimerSettings.Default.Relax_Time = 10;
            else if (cbRelaxTimeComboBox.SelectedIndex == 1)
                TomatoTimerWPF.TimerSettings.Default.Relax_Time = 5;
            else
            {
                tbox = cbRelaxTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    TomatoTimerWPF.TimerSettings.Default.Relax_Time = Int32.Parse(tbox.Text);
                }
            }


            if (cbLongRelaxTimeComboBox.SelectedIndex == 0)
                TomatoTimerWPF.TimerSettings.Default.Relax_Time_Long = 30;
            else if (cbLongRelaxTimeComboBox.SelectedIndex == 1)
                TomatoTimerWPF.TimerSettings.Default.Relax_Time_Long = 15;
            else
            {
                tbox = cbLongRelaxTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    TomatoTimerWPF.TimerSettings.Default.Relax_Time_Long = Int32.Parse(tbox.Text);
                }
            }

            if (cbWorkTimeComboBox.SelectedIndex == 0)
                TomatoTimerWPF.TimerSettings.Default.Work_Time = 30;
            else if (cbWorkTimeComboBox.SelectedIndex == 1)
                TomatoTimerWPF.TimerSettings.Default.Work_Time = 25;
            else
            {
                tbox = cbWorkTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    TomatoTimerWPF.TimerSettings.Default.Work_Time = Int32.Parse(tbox.Text);
                }
            }


            tbox = cbWorkTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                TomatoTimerWPF.TimerSettings.Default.WorkTime_Uesr = Int32.Parse(tbox.Text);

            tbox = cbRelaxTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                TomatoTimerWPF.TimerSettings.Default.RelaxTime_Uesr = Int32.Parse(tbox.Text);

            tbox = cbLongRelaxTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                TomatoTimerWPF.TimerSettings.Default.Relax_Time_Long_User = Int32.Parse(tbox.Text);


            TomatoTimerWPF.TimerSettings.Default.GoogleCal_src = tbGoogleCal_src.Text;
            TomatoTimerWPF.TimerSettings.Default.GoogleCal_text = tbGoogleCal_text.Text;
            TomatoTimerWPF.TimerSettings.Default.GoogleCal_EnableEvent = (cbEnableGCal.IsChecked == true);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SyncUIValueToSettings();
            
        }

        private void Button_Close_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //Switcher.Close();
            m_window.Close();
        }

        private void OnClick_Test_GoogleCalender(object sender, System.Windows.RoutedEventArgs e)
        {
            TomatoTimerWPF.TimerSettings.Default.GoogleCal_src = tbGoogleCal_src.Text;
            TomatoTimerWPF.TimerSettings.Default.GoogleCal_text = tbGoogleCal_text.Text;
            m_window.OpenGoogleCalender(DateTime.Now - TimeSpan.FromMinutes(TomatoTimerWPF.TimerSettings.Default.Work_Time), DateTime.Now);
            //Switcher.GetBaseWindow().OpenGoogleCalender(DateTime.Now - TimeSpan.FromMinutes(TomatoTimerWPF.TimerSettings.Default.Work_Time), DateTime.Now);
        }

        Regex NumEx = new Regex(@"^-?\d*\.?\d*$");

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox)
            {
                string text = (sender as TextBox).Text + e.Text;

                e.Handled = !NumEx.IsMatch(text);
            }
            else
                throw new NotImplementedException("TextBox_PreviewTextInput Can only Handle TextBoxes");
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SyncSettingsToUIValue();
        }

        private void myGif_MediaEnded(object sender, RoutedEventArgs e)
        {
            //myGif.Position = new TimeSpan(0, 0, 1);
            //myGif.Play();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == cbEnableGCal)
            {
                if (cbEnableGCal.IsChecked == true)
                {
                    spGoogleEventTitle.IsEnabled = true;
                    spGoogleID.IsEnabled = true;
                    menuTestGoogleCal.IsEnabled = true;
                }
                else
                {
                    spGoogleEventTitle.IsEnabled = false;
                    spGoogleID.IsEnabled = false;
                    menuTestGoogleCal.IsEnabled = false;
                }
                TomatoTimerWPF.TimerSettings.Default.GoogleCal_EnableEvent = (cbEnableGCal.IsChecked == true);
            }//
            else if (sender == cbCopyLinkToClipBoard)
            {
                TomatoTimerWPF.TimerSettings.Default.GoogleCal_CopyToClipboard = (cbCopyLinkToClipBoard.IsChecked == true);
            }
        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            gifHowToDemo.FrameIndex = 0;
            gifHowToDemo.StopAnimation();
            gifHowToDemo.StartAnimation();
        }

        private void cbComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SyncUIValueToSettings();    
        }

        private void menuClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("menuClose_MouseLeftButtonUp");
            m_window.SavePropertiesAndClose(true);
        }

        private void menuCloseDontSave_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("menuCloseDontSave_Click");
            m_window.SavePropertiesAndClose(false);
        }

        private void menuCloseSave_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("menuCloseSave_Click");
            m_window.SavePropertiesAndClose(true);
        }

        private void btnGotoSoundSettings_Click(object sender, RoutedEventArgs e)
        {
            m_window.SwitchFromSettingToSound();
        }

        private void labelVersion_small_MouseDown(object sender, MouseButtonEventArgs e)
        {
            m_bVersionMode = !m_bVersionMode;
            if (m_bVersionMode)
                labelVersion_small.Content = "Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            else
            {
                Version ver = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                var date = new DateTime( 2000, 01, 01 ).AddDays( ver.Build ).AddSeconds( ver.Revision * 2 );
                labelVersion_small.Content = "Build date: " + date.ToString();
            }

        }


        private void btnGCal_Click(object sender, RoutedEventArgs e)
        {
            Button btnSelf = (sender as Button);

            btnSelf.ContextMenu.IsEnabled = true;
            btnSelf.ContextMenu.PlacementTarget = btnSelf;
            btnSelf.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            //btnSelf.ContextMenu.VerticalOffset = (btnSelf.ActualHeight);
            //btnSelf.ContextMenu.HorizontalOffset = (btnSelf.ContextMenu.ActualWidth / 2);
            btnSelf.ContextMenu.IsOpen = true;

        }
    }
}
