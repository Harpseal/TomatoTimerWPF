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

namespace TomatoTimerWPF
{
    /// <summary>
    /// Interaction logic for Page_Buttons.xaml
    /// </summary>
    public partial class Page_Buttons : UserControl , ISwitchable
    {
        
        private System.Windows.Point m_MousePosition;
        public DateTime m_MouseDownTime;
        public bool m_bIsMouseDown = false;

        public Page_Buttons()
        {
            m_MousePosition.X = m_MousePosition.Y = 0;
            InitializeComponent();
            spButtonStackPanel.Opacity = 0;
            btnGotoSetting.Opacity = 0;
            spWindowControlStackPanel.Opacity = 0;
            labelTime_small.Opacity = 0;
            grLabelGrid.Opacity = 1;

            m_MouseDownTime = DateTime.Now;
        }

        #region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        private void Button_GotoSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Switcher.Switch(new Page_Settings());
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
            Switcher.Close();
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
                Switcher.GetBaseWindow().DragMove();
            }

            

        }

        private void Grid_MouseEnter_1(object sender, MouseEventArgs e)
        {
            btnGotoSetting.Opacity = 1;
            spWindowControlStackPanel.Opacity = 1;
            spButtonStackPanel.Opacity = 1;
            labelTime_small.Opacity = 1;
            grLabelGrid.Opacity = 0;
        }

        private void Grid_MouseLeave_1(object sender, MouseEventArgs e)
        {
            btnGotoSetting.Opacity = 0;
            spWindowControlStackPanel.Opacity = 0;
            spButtonStackPanel.Opacity = 0;
            labelTime_small.Opacity = 0;
            grLabelGrid.Opacity = 1;
        }

        private void Button_GotoWork_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Switcher.GetBaseWindow().StartWork();
        }

        private void Button_GotoRelax_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Switcher.GetBaseWindow().StartRelax();
            m_bIsMouseDown = false;
            //TimeSpan downTime = DateTime.Now - m_MouseDownTime;
            //MessageBox.Show(downTime.ToString());
        }

        private void Button_Pause_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Switcher.GetBaseWindow().Pause();
        }

        private void Button_Resume_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Switcher.GetBaseWindow().Resume();
        }

        private void Button_Reset_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Switcher.GetBaseWindow().Reset();
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
    }


}
