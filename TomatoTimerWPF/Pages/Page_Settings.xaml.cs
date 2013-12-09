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
    public partial class Page_Settings : UserControl , ISwitchable
    {
        public Page_Settings()
        {
            InitializeComponent();
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
            Switcher.Switch(new Page_Buttons());
            
        }
        #endregion

        private void SyncSettingsToUIValue()
        {
            TextBox tbox;

            tbox = cbWorkTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                tbox.Text = "" + TomatoTimerWPF.Properties.Settings.Default.WorkTime_Uesr;

            tbox = cbRelaxTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                tbox.Text = "" + TomatoTimerWPF.Properties.Settings.Default.RelaxTime_Uesr;

            tbox = cbLongRelaxTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                tbox.Text = "" + TomatoTimerWPF.Properties.Settings.Default.RelaxTimeLong_User;


            if (TomatoTimerWPF.Properties.Settings.Default.Work_Time == 30)
                cbWorkTimeComboBox.SelectedIndex = 0;
            else if (TomatoTimerWPF.Properties.Settings.Default.Work_Time == 25)
                cbWorkTimeComboBox.SelectedIndex = 1;
            else
            {
                cbWorkTimeComboBox.SelectedIndex = 2;
                tbox = cbWorkTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    tbox.Text = "" + TomatoTimerWPF.Properties.Settings.Default.Work_Time;
                }
            }

            if (TomatoTimerWPF.Properties.Settings.Default.Relax_Time == 10)
                cbRelaxTimeComboBox.SelectedIndex = 0;
            else if (TomatoTimerWPF.Properties.Settings.Default.Relax_Time == 5)
                cbRelaxTimeComboBox.SelectedIndex = 1;
            else
            {
                cbRelaxTimeComboBox.SelectedIndex = 2;
                tbox = cbRelaxTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    tbox.Text = "" + TomatoTimerWPF.Properties.Settings.Default.Relax_Time;
                }
            }

            if (TomatoTimerWPF.Properties.Settings.Default.Relax_Time_Long == 30)
                cbLongRelaxTimeComboBox.SelectedIndex = 0;
            else if (TomatoTimerWPF.Properties.Settings.Default.Relax_Time_Long == 15)
                cbLongRelaxTimeComboBox.SelectedIndex = 1;
            else
            {
                cbLongRelaxTimeComboBox.SelectedIndex = 2;
                tbox = cbLongRelaxTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    tbox.Text = "" + TomatoTimerWPF.Properties.Settings.Default.Relax_Time_Long;
                }
            }

            tbGoogleCal_src.Text = TomatoTimerWPF.Properties.Settings.Default.GoogleCal_src;
            tbGoogleCal_text.Text = TomatoTimerWPF.Properties.Settings.Default.GoogleCal_text;
            cbEnableGCal.IsChecked = TomatoTimerWPF.Properties.Settings.Default.EnableGCalEvent;

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
                TomatoTimerWPF.Properties.Settings.Default.Relax_Time = 10;
            else if (cbRelaxTimeComboBox.SelectedIndex == 1)
                TomatoTimerWPF.Properties.Settings.Default.Relax_Time = 5;
            else
            {
                tbox = cbRelaxTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    TomatoTimerWPF.Properties.Settings.Default.Relax_Time = Int32.Parse(tbox.Text);
                }
            }


            if (cbLongRelaxTimeComboBox.SelectedIndex == 0)
                TomatoTimerWPF.Properties.Settings.Default.Relax_Time_Long = 30;
            else if (cbLongRelaxTimeComboBox.SelectedIndex == 1)
                TomatoTimerWPF.Properties.Settings.Default.Relax_Time_Long = 15;
            else
            {
                tbox = cbLongRelaxTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    TomatoTimerWPF.Properties.Settings.Default.Relax_Time_Long = Int32.Parse(tbox.Text);
                }
            }

            if (cbWorkTimeComboBox.SelectedIndex == 0)
                TomatoTimerWPF.Properties.Settings.Default.Work_Time = 30;
            else if (cbWorkTimeComboBox.SelectedIndex == 1)
                TomatoTimerWPF.Properties.Settings.Default.Work_Time = 25;
            else
            {
                tbox = cbWorkTimeComboBox.Items[2] as TextBox;
                if (tbox != null)
                {
                    TomatoTimerWPF.Properties.Settings.Default.Work_Time = Int32.Parse(tbox.Text);
                }
            }


            tbox = cbWorkTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                TomatoTimerWPF.Properties.Settings.Default.WorkTime_Uesr = Int32.Parse(tbox.Text);

            tbox = cbRelaxTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                TomatoTimerWPF.Properties.Settings.Default.RelaxTime_Uesr = Int32.Parse(tbox.Text);

            tbox = cbLongRelaxTimeComboBox.Items[2] as TextBox;
            if (tbox != null)
                TomatoTimerWPF.Properties.Settings.Default.RelaxTimeLong_User = Int32.Parse(tbox.Text);


            TomatoTimerWPF.Properties.Settings.Default.GoogleCal_src = tbGoogleCal_src.Text;
            TomatoTimerWPF.Properties.Settings.Default.GoogleCal_text = tbGoogleCal_text.Text;
            TomatoTimerWPF.Properties.Settings.Default.EnableGCalEvent = (cbEnableGCal.IsChecked == true);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SyncUIValueToSettings();
            
        }

        private void Button_Close_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Switcher.Close();
        }

        private void OnClick_Test_GoogleCalender(object sender, System.Windows.RoutedEventArgs e)
        {
            TomatoTimerWPF.Properties.Settings.Default.GoogleCal_src = tbGoogleCal_src.Text;
            TomatoTimerWPF.Properties.Settings.Default.GoogleCal_text = tbGoogleCal_text.Text;
            Switcher.GetBaseWindow().OpenGoogleCalender(DateTime.Now - TimeSpan.FromMinutes(TomatoTimerWPF.Properties.Settings.Default.Work_Time), DateTime.Now);
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
            TomatoTimerWPF.Properties.Settings.Default.EnableGCalEvent = (cbEnableGCal.IsChecked == true);
        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            gifimgDemo.FrameIndex = 0;
            gifimgDemo.StopAnimation();
            gifimgDemo.StartAnimation();
        }

        private void cbComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SyncUIValueToSettings();    
        }
    }
}
