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

using System.Windows.Media.Animation;

namespace TomatoTimerWPF
{
    /// <summary>
    /// Interaction logic for Page_Buttons.xaml
    /// </summary>
    public partial class Page_Buttons : UserControl
    {
        
        private System.Windows.Point m_MousePosition;
        public DateTime m_MouseDownTime;
        public bool m_bIsMouseDown = false;
        
        private MainWindow m_window;
        public void SetMainWindow(MainWindow window)
        {
            m_window = window;
            this.ToggleAlwaysOnTop();
        }

        Storyboard m_sbAniOut;
        Storyboard m_sbAniIn;

        public Page_Buttons(MainWindow window)
        {
            InitializeComponent();

            m_window = window;

            btnAlwaysOnTop.IsChecked = TomatoTimerWPF.Properties.Settings.Default.AlwaysOnTop;
            if (btnAlwaysOnTop.IsChecked == true)
                this.ToggleAlwaysOnTop();

            m_MousePosition.X = m_MousePosition.Y = 0;
            
            spButtonStackPanel.Opacity = 0;
            btnGotoSetting.Opacity = 0;
            spWindowControlStackPanel.Opacity = 0;
            labelTime_small.Opacity = 0;
            grLabelGrid.Opacity = 1;

            
            m_sbAniOut = new Storyboard();
            DoubleAnimation daFadeOut = new DoubleAnimation();
            daFadeOut.Duration = 200.Milliseconds();
            daFadeOut.To = 0.0;

            m_sbAniOut.Children.Add(daFadeOut);
            Storyboard.SetTargetProperty(daFadeOut, new PropertyPath(UIElement.OpacityProperty));

            m_sbAniIn = new Storyboard();
            DoubleAnimation daFadeIn = new DoubleAnimation();
            daFadeIn.Duration = 200.Milliseconds();
            daFadeIn.From = 0.0;
            daFadeIn.To = 1.0;

            m_sbAniIn.Children.Add(daFadeIn);
            Storyboard.SetTargetProperty(daFadeIn, new PropertyPath(UIElement.OpacityProperty));


            m_MouseDownTime = DateTime.Now;
        }

        #region ISwitchable Members
        //public void UtilizeState(object state)
        //{
        //    throw new NotImplementedException();
        //}

        private void Button_GotoSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //Switcher.Switch(new Page_Settings());
            m_window.SwitchToSettings();
        }
        #endregion


        private void OnButtonStackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            //spButtonStackPanel.Opacity = 1;
            //grLabelGrid.Opacity = 0;
       

        }

        private void OnButtonStackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            //spButtonStackPanel.Opacity = 0;
            //grLabelGrid.Opacity = 1;
        }

        private void Button_Close_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_window.Close();
        }

        private void OnButtonMove_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            m_MousePosition = e.GetPosition(btnMove);
            //MessageBox.Show("MD");
           // Switcher.GetBaseWindow().DragMove();

        }

        private void OnButtonMove_MouseMove_1(object sender, MouseEventArgs e)
        {
            //MessageBox.Show("MM");
            var currentPoint = e.GetPosition(btnMove);
            if (e.LeftButton == MouseButtonState.Pressed
                &&
                btnMove.IsMouseCaptured &&
                (Math.Abs(currentPoint.X - m_MousePosition.X) >
                    SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(currentPoint.Y - m_MousePosition.Y) >
                    SystemParameters.MinimumVerticalDragDistance))
            {
                //MessageBox.Show("DragMove");
                // Prevent Click from firing
                btnMove.ReleaseMouseCapture();
                //Switcher.GetBaseWindow().DragMove();
                m_window.DragMove();
            }

            

        }

        private void Grid_MouseEnter_1(object sender, MouseEventArgs e)
        {
            //btnGotoSetting.Opacity = 1;
            //spWindowControlStackPanel.Opacity = 1;
            //spButtonStackPanel.Opacity = 1;
            //labelTime_small.Opacity = 1;
            //grLabelGrid.Opacity = 0;

            m_sbAniIn.Begin(btnGotoSetting);
            m_sbAniIn.Begin(spWindowControlStackPanel);
            m_sbAniIn.Begin(spButtonStackPanel);
            m_sbAniIn.Begin(labelTime_small);
            m_sbAniOut.Begin(grLabelGrid);
        }

        private void Grid_MouseLeave_1(object sender, MouseEventArgs e)
        {
            m_sbAniOut.Begin(btnGotoSetting);
            m_sbAniOut.Begin(spWindowControlStackPanel);
            m_sbAniOut.Begin(spButtonStackPanel);
            m_sbAniOut.Begin(labelTime_small);
            m_sbAniIn.Begin(grLabelGrid);

            //btnGotoSetting.Opacity = 0;
            //spWindowControlStackPanel.Opacity = 0;
            //spButtonStackPanel.Opacity = 0;
            //labelTime_small.Opacity = 0;
            //grLabelGrid.Opacity = 1;
        }

        private void Button_GotoWork_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //Switcher.GetBaseWindow().StartWork();
            m_window.StartWork();
        }

        private void Button_GotoRelax_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //Switcher.GetBaseWindow().StartRelax();
            m_window.StartRelax();
            m_bIsMouseDown = false;
            //TimeSpan downTime = DateTime.Now - m_MouseDownTime;
            //MessageBox.Show(downTime.ToString());
        }

        private void Button_Pause_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            m_window.Pause();
        }

        private void Button_Resume_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_window.Resume();
        }

        private void Button_Reset_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_window.Reset();
        }

        private void btnRelax_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            m_MouseDownTime = DateTime.Now;
            m_bIsMouseDown = true;
        }

        public bool GetIsLongRest()
        {
            return m_bIsMouseDown && !(DateTime.Now - m_MouseDownTime - 3.Seconds()).IsNegativeOrZero();
        }

        private void btnAlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleAlwaysOnTop();
            TomatoTimerWPF.Properties.Settings.Default.AlwaysOnTop = this.m_window.Topmost;
        }

        private void ToggleAlwaysOnTop()
        {
            if (this.btnAlwaysOnTop != null && this.m_window != null)
            {
                this.m_window.Topmost = btnAlwaysOnTop.IsChecked.HasValue && btnAlwaysOnTop.IsChecked.Value;
            }
        }
    }


}
