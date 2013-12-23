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

using Microsoft.WindowsAPICodePack.Taskbar;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace TomatoTimerWPF
{
    using Res = Properties.Resources;
    using Setting = Properties.Settings;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("user32.dll")]
        static extern Int32 FlashWindowEx(ref FLASHWINFO pwfi);
        //static extern uint SetClassLong(HandleRef hWnd, int nIndex, uint dwNewLong);
        //static extern uint GetClassLong(HandleRef hWnd, int nIndex, uint dwNewLong);

        [//Obsolete("This method will crash on 64-bit operating systems. Use SetClassLongPtr instead"),
        DllImport("user32.dll", SetLastError = true)]
        static extern uint SetClassLong(HandleRef hWnd, int nIndex, uint dwNewLong);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public Int32 dwFlags;
            public UInt32 uCount;
            public Int32 dwTimeout;
        }


        enum TimerMode{
            MODE_WORK,
            MODE_RELAX,
            MODE_RELAX_LONG
        };

        private System.Windows.Forms.Timer m_timer;
        private bool m_bIsPause = false;
        private DateTime m_TimeDateStart;
        private DateTime m_TimeDatePauseStart;
        private TimeSpan m_TimeSpan;
        private TimeSpan m_TimeSpanPause;
        private TimerMode m_mode = TimerMode.MODE_WORK;
        private int m_OverlayIconLastMin;
        private bool m_bIsSupportTaskbarManager = false;
        private bool m_bIsOverTime = false;

        private System.Media.SoundPlayer m_resumeSound;
        private System.Media.SoundPlayer m_pauseSound;
        private System.Media.SoundPlayer m_endSound;

        public IntPtr m_hwnd;

        private ThumbnailToolbarButton m_btnReset;
        private ThumbnailToolbarButton m_btnPlay;
        private ThumbnailToolbarButton m_btnPause;
        private ThumbnailToolbarButton m_btnGoToWork;
        private ThumbnailToolbarButton m_btnGoToRest;

        public event Action m_ActionPlay = () => { };
        public event Action m_ActionPause = () => { };
        public event Action m_ActionReset = () => { };
        public event Action m_ActionGoToWork = () => { };
        public event Action m_ActionTakeABreak = () => { };

        private Page_Buttons m_pageButtons;
        private Page_Settings m_pageSettings;

        Storyboard m_sbAniOut;
        Storyboard m_sbAniIn;

        ThicknessAnimation m_taSlideOut;
        ThicknessAnimation m_taSlideIn;

        public MainWindow()
        {
            InitializeComponent();

            m_OverlayIconLastMin = -99999;

            m_pageButtons = new Page_Buttons(this);
            m_pageSettings = new Page_Settings(this);

            //Initialize animations
            m_sbAniOut = new Storyboard();
            DoubleAnimation daFadeOut = new DoubleAnimation();
            daFadeOut.Duration = 200.Milliseconds();
            daFadeOut.To = 0.0;

            ThicknessAnimation taSlideOut = new ThicknessAnimation();
            taSlideOut.Duration = 200.Milliseconds();
            taSlideOut.To = new Thickness(0, this.Height, 0, 0);
            taSlideOut.From = new Thickness(0, 0, 0, 0);

            m_sbAniOut.Completed += Storyboard_Completed;
            m_sbAniOut.Children.Add(daFadeOut);
            m_sbAniOut.Children.Add(taSlideOut);
            Storyboard.SetTargetProperty(daFadeOut, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTargetProperty(taSlideOut, new PropertyPath(FrameworkElement.MarginProperty));
            

            m_sbAniIn = new Storyboard();
            DoubleAnimation daFadeIn = new DoubleAnimation();
            daFadeIn.Duration = 200.Milliseconds();
            daFadeIn.From = 0.0;
            daFadeIn.To = 1.0;

            ThicknessAnimation taSlideIn = new ThicknessAnimation();
            taSlideIn.Duration = 200.Milliseconds();
            taSlideIn.From = new Thickness(0, -this.Height, 0, 0);
            taSlideIn.To = new Thickness(0, 0, 0, 0);

            m_sbAniIn.Completed += Storyboard_Completed;
            m_sbAniIn.Children.Add(daFadeIn);
            m_sbAniIn.Children.Add(taSlideIn);
            Storyboard.SetTargetProperty(daFadeIn, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTargetProperty(taSlideIn, new PropertyPath(FrameworkElement.MarginProperty));
            
            m_taSlideOut = taSlideOut;
            m_taSlideIn = taSlideIn;

            try
            {
                Rect bounds = Rect.Parse(TomatoTimerWPF.Properties.Settings.Default.WindowRestoreBounds);
                int iInsideLT, iInsideRB;
                iInsideLT = iInsideRB = -1;
                for (int s = 0; s < System.Windows.Forms.Screen.AllScreens.Length;s++ )
                {
                    System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.AllScreens[s];
                    if (bounds.Left >= screen.Bounds.Left && bounds.Top >= screen.Bounds.Top &&
                        bounds.Left < screen.Bounds.Right && bounds.Top < screen.Bounds.Bottom)
                    {
                        iInsideLT = s;
                    }

                    if (bounds.Right >= screen.Bounds.Left && bounds.Bottom >= screen.Bounds.Top &&
                        bounds.Right < screen.Bounds.Right && bounds.Bottom < screen.Bounds.Bottom)
                    {
                        iInsideRB = s;
                    }
                }
                if (iInsideLT != -1 || iInsideRB != -1)
                {
                    int recheckScreen = -1;

                    if (iInsideLT == -1)
                        recheckScreen = iInsideRB;
                    else if (iInsideRB == -1)
                        recheckScreen = iInsideLT;
                    
                    if (recheckScreen!=-1)
                    {
                        System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.AllScreens[recheckScreen];

                        if (bounds.X < screen.Bounds.Left)
                            bounds.X = screen.Bounds.Left;
                        if (bounds.Y < screen.Bounds.Top)
                            bounds.Y = screen.Bounds.Top;
                        if (bounds.Right > screen.Bounds.Right)
                            bounds.X -= bounds.Right - screen.Bounds.Right;
                        if (bounds.Bottom > screen.Bounds.Bottom)
                            bounds.Y -= bounds.Bottom - screen.Bounds.Bottom;
                      
                    }
                    this.Top = bounds.Top;
                    this.Left = bounds.Left;
                    this.Width = bounds.Width;
                    this.Height = bounds.Height;
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("[" + TomatoTimerWPF.Properties.Settings.Default.WindowRestoreBounds + "]");
            }
        }

        private ThumbnailToolbarButton CreateToolbarButton(Icon icon, string toolTip, Action onClick)
        {
            return new ThumbnailToolbarButton(icon, toolTip)
            {
                DismissOnClick = true
            }
            .Chain(btn => btn.Click += (o, e) => onClick());
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            m_hwnd = new WindowInteropHelper(this).Handle;

            if (!TaskbarManager.IsPlatformSupported)
                m_bIsSupportTaskbarManager = false;
            else
            {
                m_bIsSupportTaskbarManager = true;

                m_ActionPlay += Resume;
                m_ActionPause += Pause;
                m_ActionReset += Reset;
                m_ActionGoToWork += StartWork;
                m_ActionTakeABreak += StartRelax;

                m_btnReset = CreateToolbarButton(Res._9_av_replay, "Reset", () => m_ActionReset());
                m_btnPlay = CreateToolbarButton(Res._9_av_play, "Play", () => m_ActionPlay());
                m_btnPause = CreateToolbarButton(Res._9_av_pause, "Pause", () => m_ActionPause());
                m_btnGoToWork = CreateToolbarButton(Res._4_collections_view_as_list, "Go to Work", () => m_ActionGoToWork());
                m_btnGoToRest = CreateToolbarButton(Res._12_hardware_gamepad, "Take a break", () => m_ActionTakeABreak());

                TaskbarManager.Instance.ThumbnailToolbars.AddButtons(
                    m_hwnd,
                    m_btnReset,
                    m_btnPlay,
                    m_btnPause,
                    m_btnGoToWork,
                    m_btnGoToRest);


                m_btnReset.Visible = false;
                m_btnPlay.Visible = false;
                m_btnPause.Visible = false;
                m_btnGoToWork.Visible = false;
                m_btnGoToRest.Visible = false;

            }

            m_resumeSound = new System.Media.SoundPlayer(Res.windows_logon);
            m_endSound = new System.Media.SoundPlayer(Res.windows_user_account);
            m_pauseSound = new System.Media.SoundPlayer(Res.system_notification);


            try
            {
                DateTime restoreStart = DateTime.Parse(TomatoTimerWPF.Properties.Settings.Default.TimerRestoreDateTime);
                m_TimeDateStart = restoreStart;
                m_TimeDatePauseStart = restoreStart;
                m_mode = (TimerMode)TomatoTimerWPF.Properties.Settings.Default.TimerRestoreMode;
            }
            catch(Exception ex)
            {
                m_TimeDateStart = DateTime.Now;
                m_TimeDatePauseStart = DateTime.Now;
            }


            this.Content = m_pageButtons;
            UpdateUI();

            m_timer = new System.Windows.Forms.Timer();
            m_timer.Interval = 1000;
            m_timer.Tick += new EventHandler(Timer_Tick);
            m_timer.Start();

        }

        void Timer_Tick(object sender, EventArgs e)
        {
            UpdateUI();
        }

        public void Navigate(UserControl nextPage)
        {
            this.Content = nextPage;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TomatoTimerWPF.Properties.Settings.Default.WindowRestoreBounds = this.RestoreBounds.ToString();

            TomatoTimerWPF.Properties.Settings.Default.TimerRestoreDateTime = m_TimeDateStart.ToString();
            TomatoTimerWPF.Properties.Settings.Default.TimerRestoreMode = (int)m_mode;

            TomatoTimerWPF.Properties.Settings.Default.Save();
        }

        public void UpdateUI()
        {


            TimeSpan pauseSpan = m_TimeSpanPause;
            if (m_bIsPause)
                pauseSpan += DateTime.Now - m_TimeDatePauseStart;

            TimeSpan timerSpan, modeSpan;
            if (m_mode == TimerMode.MODE_WORK)
                modeSpan = TimeSpan.FromMinutes(TomatoTimerWPF.Properties.Settings.Default.Work_Time) + 800.Milliseconds();
            else if (m_mode == TimerMode.MODE_RELAX)
                modeSpan = TimeSpan.FromMinutes(TomatoTimerWPF.Properties.Settings.Default.Relax_Time) + 800.Milliseconds();
            else if (m_mode == TimerMode.MODE_RELAX_LONG)
                modeSpan = TimeSpan.FromMinutes(TomatoTimerWPF.Properties.Settings.Default.Relax_Time_Long) + 800.Milliseconds();
            else
                modeSpan = 1.Seconds();

            if (m_bIsPause)
                timerSpan = modeSpan - m_TimeSpan;
            else
                timerSpan = modeSpan - (DateTime.Now - m_TimeDateStart);

            if (!m_bIsOverTime && timerSpan.IsNegativeOrZero())
            {
                SetWindowFlash(true);
                m_endSound.Play();
            }

            m_bIsOverTime = timerSpan.IsNegativeOrZero();

            double progressValue = 100.0 - ((timerSpan.TotalMilliseconds * 100.0 / modeSpan.TotalMilliseconds));
            if (progressValue < 0.0)
                progressValue = 0.0;
            else if (progressValue > 100.0)
                progressValue = 100.0;
            

            Page_Buttons pageButtons = m_pageButtons;//pageTransitionControl.CurrentPage as Page_Buttons;
            if (pageButtons != null)
            {
                if (m_mode == TimerMode.MODE_WORK)
                {
                    pageButtons.btnWork.Visibility = false.ToVisibility();
                    pageButtons.btnWork.IsEnabled = false;

                    pageButtons.btnRelax.Visibility = true.ToVisibility();
                    pageButtons.btnRelax.IsEnabled = true;

                    if (m_bIsPause)
                    {
                        pageButtons.btnPause.Visibility = false.ToVisibility();
                        pageButtons.btnPause.IsEnabled = false;

                        pageButtons.btnPlay.Visibility = true.ToVisibility();
                        pageButtons.btnPlay.IsEnabled = true;
                        pageButtons.labelTime.Opacity = 0.5;
                    }
                    else
                    {

                        pageButtons.btnPause.Visibility = (!m_bIsPause).ToVisibility();
                        pageButtons.btnPause.IsEnabled = (!m_bIsPause);

                        pageButtons.btnPlay.Visibility = false.ToVisibility();
                        pageButtons.btnPlay.IsEnabled = false;
                        pageButtons.labelTime.Opacity = 1;
                    }
                }
                else
                {
                    pageButtons.btnWork.Visibility = true.ToVisibility();
                    pageButtons.btnWork.IsEnabled = true;

                    pageButtons.btnRelax.Visibility = false.ToVisibility();
                    pageButtons.btnRelax.IsEnabled = false;

                    pageButtons.btnPause.Visibility = false.ToVisibility();
                    pageButtons.btnPause.IsEnabled = false;

                    pageButtons.btnPlay.Visibility = false.ToVisibility();
                    pageButtons.btnPlay.IsEnabled = false;
                    pageButtons.labelTime.Opacity = 1;
                }

                String info, time;
                if (m_TimeDateStart.Day != DateTime.Now.Day || m_TimeDateStart.Month != DateTime.Now.Month)
                    info = m_TimeDateStart.ToString("MM/dd H:mm");
                else
                    info = "Start @ " + m_TimeDateStart.ToString("H:mm");

                if (!pauseSpan.IsNegativeOrZero())
                {
                    if (pauseSpan.Hours != 0 || pauseSpan.Days != 0)
                        info += "\r\nPause: {0}:{1:00}:{2:00}".ToFormat(Math.Abs(pauseSpan.Hours + pauseSpan.Days * 24), Math.Abs(pauseSpan.Minutes), Math.Abs(pauseSpan.Seconds));
                    else
                        info += "\r\nPause: {0}:{1:00}".ToFormat(Math.Abs(pauseSpan.Minutes), Math.Abs(pauseSpan.Seconds));
                }

                pageButtons.labelInfo.Content = info;
                

                if (m_mode == TimerMode.MODE_WORK)
                    time = "Work: ";
                else if (m_mode == TimerMode.MODE_RELAX || m_mode == TimerMode.MODE_RELAX_LONG)
                    time = "Rest: ";
                else
                    time = "";


                if (timerSpan.Hours != 0 || timerSpan.Days != 0)
                    time += "{0}:{1:00}:{2:00}".ToFormat(Math.Abs(timerSpan.Hours + timerSpan.Days * 24), Math.Abs(timerSpan.Minutes), Math.Abs(timerSpan.Seconds));
                else
                    time += "{0}:{1:00}".ToFormat(Math.Abs(timerSpan.Minutes), Math.Abs(timerSpan.Seconds));

                pageButtons.labelTime.Content = time;
                pageButtons.labelTimeWhite.Content = time;

                if (pageButtons.GetIsLongRest())
                    pageButtons.labelTime_small.Content = "Long rest";
                else
                    pageButtons.labelTime_small.Content = time;


                if (m_bIsPause)
                {
                    pageButtons.pbarTimer.Foreground = System.Windows.Media.Brushes.Green;
                    pageButtons.pbarTimer.IsIndeterminate = true;
                    pageButtons.pbarTimer.Value = progressValue;
                    pageButtons.labelTimeWhite.Opacity = 0;
                }
                else if (m_bIsOverTime)
                {
                    pageButtons.pbarTimer.Foreground = System.Windows.Media.Brushes.Red;
                    pageButtons.pbarTimer.IsIndeterminate = false;
                    pageButtons.pbarTimer.Value = 100;
                    pageButtons.labelTimeWhite.Opacity = 1;
                }
                else
                {
                    if (progressValue > 80)
                    {
                        pageButtons.pbarTimer.Foreground = System.Windows.Media.Brushes.Yellow;
                        pageButtons.labelTimeWhite.Opacity = 0;
                    }
                    else
                    {
                        pageButtons.pbarTimer.Foreground = System.Windows.Media.Brushes.Green;
                        pageButtons.labelTimeWhite.Opacity = 1;
                    }
                    pageButtons.pbarTimer.IsIndeterminate = false;
                    pageButtons.pbarTimer.Value = progressValue;
                }


                //var v = VisualTreeHelper.GetOffset(pageButtons.pbarTimer);

                //TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(
                //    new WindowInteropHelper(this).Handle,
                //    new System.Drawing.Rectangle((int)v.X, (int)v.Y, (int)pageButtons.pbarTimer.RenderSize.Width, (int)pageButtons.pbarTimer.RenderSize.Height));

                
            }


            int tMin = timerSpan.Hours * 24 + timerSpan.Minutes;

            if (m_bIsSupportTaskbarManager)
            {

                if (m_OverlayIconLastMin != (tMin == 0 ? timerSpan.Seconds : tMin) + (timerSpan.IsNegativeOrZero() ? 1 : 0) + (m_bIsPause ? 3 : 1))
                {
                    Bitmap bmp = new Bitmap(16, 16);


                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.FillRectangle(System.Drawing.Brushes.Black, 0, 0, 16, 16);
                        if (m_bIsPause)
                            g.FillRectangle(System.Drawing.Brushes.Gray, 1, 1, 14, 14);
                        else if (timerSpan.IsNegativeOrZero())
                            g.FillRectangle(System.Drawing.Brushes.Red, 1, 1, 14, 14);
                        else if (m_mode == TimerMode.MODE_WORK)
                            g.FillRectangle(System.Drawing.Brushes.BlueViolet, 1, 1, 14, 14);
                        else
                            g.FillRectangle(System.Drawing.Brushes.MediumSeaGreen, 1, 1, 14, 14);

                        if (timerSpan.Hours != 0 || timerSpan.Days != 0)
                        {
                            if (Math.Abs(timerSpan.Hours + timerSpan.Days * 24) <= 99)
                            {
                                g.DrawString("{0:00}".ToFormat(Math.Abs(timerSpan.Hours + timerSpan.Days * 24)), new Font("Arial", 6), new SolidBrush(System.Drawing.Color.White), 2, -1);
                                g.DrawString("{0:00}".ToFormat(Math.Abs(timerSpan.Minutes)), new Font("Arial", 6), new SolidBrush(System.Drawing.Color.White), 2, 6);
                            }
                            else
                            {
                                g.DrawString("99", new Font("Impact", 6), new SolidBrush(System.Drawing.Color.White), 2, -1);
                                g.DrawString("59", new Font("Impact", 6), new SolidBrush(System.Drawing.Color.White), 2, 6);
                            }
                        }
                        else if (tMin == 0 && !timerSpan.IsNegativeOrZero() && !m_bIsPause)
                            g.DrawString("{0:00}".ToFormat(Math.Abs(timerSpan.Seconds)), new Font("Courier New", 9), new SolidBrush(timerSpan.Seconds % 2 == 0 ? System.Drawing.Color.White : System.Drawing.Color.Black), -1, 0);
                        else
                            g.DrawString("{0:00}".ToFormat(Math.Abs(timerSpan.Minutes)), new Font("Courier New", 9), new SolidBrush(System.Drawing.Color.White), -1, 0);

                        TaskbarManager.Instance.SetOverlayIcon(System.Drawing.Icon.FromHandle(bmp.GetHicon()), "icon");
                    }
                    m_OverlayIconLastMin = (tMin == 0 ? timerSpan.Seconds : tMin) + (timerSpan.IsNegativeOrZero() ? 1 : 0) + (m_bIsPause ? 3 : 1);
                }


                if (m_bIsPause)
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate);
                else if (m_bIsOverTime)
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);
                else
                {
                    if (progressValue > 80)
                        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Paused);
                    else
                        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                    TaskbarManager.Instance.SetProgressValue((int)progressValue, 100);
                }


                if (m_mode == TimerMode.MODE_WORK)
                {

                    m_btnGoToWork.Visible = m_btnGoToWork.Enabled = false;
                    //pageButtons.btnWork.Visibility = false.ToVisibility();

                    m_btnGoToRest.Visible = m_btnGoToRest.Enabled = true;
                    //pageButtons.btnRelax.Visibility = true.ToVisibility();

                    m_btnPause.Visible = m_btnPause.Enabled = !m_bIsPause;
                    //pageButtons.btnPause.Visibility = (!m_bIsPause).ToVisibility();

                    m_btnPlay.Visible = m_btnPlay.Enabled = m_bIsPause;
                    //pageButtons.btnPlay.Visibility = (m_bIsPause).ToVisibility();
                    //pageButtons.labelTime.Opacity = m_bIsPause ? 0.5 : 1;

                }
                else
                {
                    m_btnGoToWork.Visible = m_btnGoToWork.Enabled = true;
                    //pageButtons.btnWork.Visibility = true.ToVisibility();

                    m_btnGoToRest.Visible = m_btnGoToRest.Enabled = false;
                    //pageButtons.btnRelax.Visibility = false.ToVisibility();

                    m_btnPause.Visible = m_btnPause.Enabled = false;
                    //pageButtons.btnPause.Visibility = false.ToVisibility();

                    m_btnPlay.Visible = m_btnPlay.Enabled = false;
                    //pageButtons.btnPlay.Visibility = false.ToVisibility();
                    //pageButtons.labelTime.Opacity = 1;
                }

                //TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate);
                
            }







        }

        private int GetPercentageComplete(TimeSpan elapsed, TimeSpan total)
        {
            return (int)((elapsed.TotalMilliseconds * 100 / total.TotalMilliseconds));
        }


        public void StartWork()
        {
            if (m_bIsOverTime && m_mode == TimerMode.MODE_WORK && 
                TomatoTimerWPF.Properties.Settings.Default.EnableGCalEvent)
                OpenGoogleCalender(m_TimeDateStart, DateTime.Now);

            SetWindowFlash(false);
            m_TimeDateStart = DateTime.Now;
            m_TimeDatePauseStart = DateTime.Now;
            m_TimeSpanPause = TimeSpan.FromMinutes(0);
            m_TimeSpan = TimeSpan.FromMinutes(0);
            m_mode = TimerMode.MODE_WORK;
            m_bIsPause = false;
            UpdateUI();
           
        }

        public void StartRelax()
        {
            if (m_bIsOverTime && m_mode == TimerMode.MODE_WORK &&
                TomatoTimerWPF.Properties.Settings.Default.EnableGCalEvent)
                OpenGoogleCalender(m_TimeDateStart, DateTime.Now);

            SetWindowFlash(false);
            m_TimeDateStart = DateTime.Now;
            m_TimeDatePauseStart = DateTime.Now;
            m_TimeSpanPause = TimeSpan.FromMinutes(0);
            m_TimeSpan = TimeSpan.FromMinutes(0);
            m_mode = TimerMode.MODE_RELAX;
            Page_Buttons pageButtons = this.Content as Page_Buttons;
            if (pageButtons != null)
            {
                if (pageButtons.GetIsLongRest())
                    m_mode = TimerMode.MODE_RELAX_LONG;
            }
            m_bIsPause = false;
            UpdateUI();
        }

        public void Pause()
        {
            if (m_bIsPause) return;
            if (m_mode == TimerMode.MODE_RELAX_LONG || m_mode == TimerMode.MODE_RELAX)
                return;
            m_pauseSound.Play();
            m_bIsPause = true;
            m_TimeSpan = DateTime.Now - m_TimeDateStart;
            m_TimeDatePauseStart = DateTime.Now;
            UpdateUI();
        }

        public void Resume()
        {
            TimeSpan pauseSpan;
            if (!m_bIsPause) return;

            m_resumeSound.Play();
            m_bIsPause = false;
            pauseSpan = DateTime.Now - m_TimeDatePauseStart;
            m_TimeDateStart += pauseSpan;
            m_TimeSpanPause += pauseSpan;

            UpdateUI();
        }

        public void Reset()
        {
            if (m_bIsOverTime && m_mode == TimerMode.MODE_WORK &&
                TomatoTimerWPF.Properties.Settings.Default.EnableGCalEvent)
                OpenGoogleCalender(m_TimeDateStart, DateTime.Now);

            SetWindowFlash(false);
            m_bIsPause = false;
            m_TimeDateStart = DateTime.Now;
            m_TimeDatePauseStart = DateTime.Now;
            m_TimeSpanPause = TimeSpan.FromMinutes(0);
            m_TimeSpan = TimeSpan.FromMinutes(0);
            UpdateUI();


        }

        private void SetWindowFlash(bool enable)
        {
            FLASHWINFO fw = new FLASHWINFO();

            fw.cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO)));
            fw.hwnd = m_hwnd;// new WindowInteropHelper(this).Handle;
            fw.dwFlags = enable ? 2 : 0;
            fw.uCount = enable ? UInt32.MaxValue : 0;

            FlashWindowEx(ref fw);
        }

        public void OpenGoogleCalender(DateTime startTime, DateTime endTime)
        {

            String strTimeStart, strTimeEnd, gcalUrl;
            DateTime dateStartUTC, dateEndUTC;

            dateStartUTC = startTime.ToUniversalTime();
            dateEndUTC = endTime.ToUniversalTime();
            //dateEnd = DateTime.UtcNow;

            strTimeStart = String.Format("{0:yyyyMMdd}", dateStartUTC) + "T" + String.Format("{0:HHmmss}", dateStartUTC) + "Z";
            strTimeEnd = String.Format("{0:yyyyMMdd}", dateEndUTC) + "T" + String.Format("{0:HHmmss}", dateEndUTC) + "Z";

            gcalUrl = "http://www.google.com/calendar/event?action=TEMPLATE";
            if (TomatoTimerWPF.Properties.Settings.Default.GoogleCal_text.Length != 0)
            {
                String textUrlEncode = System.Web.HttpUtility.UrlEncode(TomatoTimerWPF.Properties.Settings.Default.GoogleCal_text);
                //MessageBox.Show("[" +TomatoTimerWPF.Properties.Settings.Default.GoogleCal_text +"]->[" + textUrlEncode +"]");
                gcalUrl += "&text=" + textUrlEncode;
            }
            if (TomatoTimerWPF.Properties.Settings.Default.GoogleCal_src.Length != 0)
            {
                if (TomatoTimerWPF.Properties.Settings.Default.GoogleCal_src.IndexOf("@")!=-1)
                {
                    gcalUrl += "&src=" + System.Web.HttpUtility.UrlEncode(TomatoTimerWPF.Properties.Settings.Default.GoogleCal_src);
                }
                else
                    gcalUrl += "&src=" + TomatoTimerWPF.Properties.Settings.Default.GoogleCal_src;
            }
                
            gcalUrl += "&dates=" + strTimeStart + "/" + strTimeEnd;
            //MessageBox.Show(gcalUrl);

            System.Diagnostics.Process.Start(gcalUrl);
        }


        public void SwitchToButtons()
        {
            m_taSlideOut.To = new Thickness(0, this.Height, 0, 0);
            m_taSlideIn.From = new Thickness(0, -this.Height, 0, 0);

            pages.Push(m_pageButtons);
            m_sbAniOut.Begin(this.Content as UserControl);
        }

        Stack<UserControl> pages = new Stack<UserControl>();

        public void SwitchToSettings()
        {
            m_taSlideOut.To = new Thickness(0, -this.Height, 0, 0);
            m_taSlideIn.From = new Thickness(0, this.Height, 0, 0);

            pages.Push(m_pageSettings);
            m_sbAniOut.Begin(this.Content as UserControl);

        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            if (pages.Count != 0)
            {
                UserControl page = pages.Pop();

                Page_Buttons pageButtons = page as Page_Buttons;
                if (pageButtons != null)
                    UpdateUI();
                this.Content = page;

                m_sbAniIn.Begin(page);
            }
        }
    }
}
