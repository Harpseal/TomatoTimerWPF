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

using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Configuration;

using TomatoTimerWPF.Pages;

namespace TomatoTimerWPF
{
    using Res = Properties.Resources;
    //using Setting = Properties.Settings;
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

        private const Int32 GWL_STYLE = -16;
        private const Int32 WS_MAXIMIZEBOX = 0x00010000;
        private const Int32 WS_MINIMIZEBOX = 0x00020000;

        [DllImport("User32.dll", EntryPoint = "GetWindowLong")]
        private extern static Int32 GetWindowLongPtr(IntPtr hWnd, Int32 nIndex);

        [DllImport("User32.dll", EntryPoint = "SetWindowLong")]
        private extern static Int32 SetWindowLongPtr(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public Int32 dwFlags;
            public UInt32 uCount;
            public Int32 dwTimeout;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        enum TimerMode{
            MODE_WORK,
            MODE_RELAX,
            MODE_RELAX_LONG
        };

        private System.Windows.Forms.Timer m_timer;
        private bool m_bIsPause = false;
        private DateTime m_TimeDateStart = DateTime.Now;
        private DateTime m_TimeDatePauseStart;
        private TimeSpan m_TimeSpan;
        private TimeSpan m_TimeSpanPause;
        private TimerMode m_mode = TimerMode.MODE_WORK;
        private int m_OverlayIconLastMin;
        private bool m_bIsSupportTaskbarManager = false;
        private bool m_bIsOverTime = true;

        public IntPtr m_hwnd;

        private Page_Buttons m_pageButtons;
        private Page_Settings m_pageSettings;
        private Page_SoundSettings m_pageSoundSettings;

        Storyboard m_sbAniOut;
        Storyboard m_sbAniIn;

        ThicknessAnimation m_taSlideOut;
        ThicknessAnimation m_taSlideIn;

        private bool m_isNormalClosingEvent;

        public MainWindow()
        {

            InitializeComponent(); 

            //string path = System.Reflection.Assembly.GetExecutingAssembly().Location + ".config";
            //if (!System.IO.File.Exists(path))
            //{
            //    //TomatoTimerWPF.TimerSettings.Default.Save();
            //    Console.WriteLine("Save setting [" + path + "]");
            //}
            //else
            //    Console.WriteLine("Exists![" + path + "]");

            m_OverlayIconLastMin = -99999;
            m_isNormalClosingEvent = false;

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
                Rect bounds = Rect.Parse(TomatoTimerWPF.TimerSettings.Default.WindowRestoreBounds);
                bounds = CheckBounds(bounds);
                this.Top = bounds.Top;
                this.Left = bounds.Left;
                this.Width = bounds.Width;
                this.Height = bounds.Height;
            }
            catch (Exception)
            {
                //MessageBox.Show(e.ToString());
                //MessageBox.Show("[" + TomatoTimerWPF.TimerSettings.Default.WindowRestoreBounds + "]");
            }
        }

        static public Rect CheckBounds(Rect bounds)
        {
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

            //if (iInsideLT != -1 || iInsideRB != -1)
            {
                int recheckScreen = -1;

                if (iInsideLT == -1 && iInsideRB == -1)
                    recheckScreen = 0;
                else if (iInsideLT == -1)
                    recheckScreen = iInsideRB;
                else if (iInsideRB == -1)
                    recheckScreen = iInsideLT;

                if (recheckScreen != -1)
                {
                    System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.AllScreens[recheckScreen];

                    if (bounds.X < screen.Bounds.Left)
                        bounds.X = screen.Bounds.Left;
                    if (bounds.Y < screen.Bounds.Top)
                        bounds.Y = screen.Bounds.Top;
                    if (bounds.Width > screen.Bounds.Width)
                        bounds.Width = screen.Bounds.Width;
                    if (bounds.Height > screen.Bounds.Height)
                        bounds.Height = screen.Bounds.Height;

                    if (bounds.Right > screen.Bounds.Right)
                        bounds.X -= bounds.Right - screen.Bounds.Right;
                    if (bounds.Bottom > screen.Bounds.Bottom)
                        bounds.Y -= bounds.Bottom - screen.Bounds.Bottom;

                }
            }
            return bounds;
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            m_hwnd = new WindowInteropHelper(this).Handle;

            Int32 windowStyle = GetWindowLongPtr(m_hwnd, GWL_STYLE);
            SetWindowLongPtr(m_hwnd, GWL_STYLE, windowStyle & ~WS_MAXIMIZEBOX);

            m_bIsSupportTaskbarManager = true;
            this.TaskbarItemInfo.ThumbnailClipMargin = new Thickness(0, 0, 0, 0);

            try
            {
                DateTime restoreStart = DateTime.Parse(TomatoTimerWPF.TimerSettings.Default.TimerRestoreDateTime);
                m_TimeDateStart = restoreStart;
                m_TimeDatePauseStart = restoreStart;
                m_mode = (TimerMode)TomatoTimerWPF.TimerSettings.Default.TimerRestoreMode;
            }
            catch(Exception)
            {
                m_TimeDateStart = DateTime.Now;
                m_TimeDatePauseStart = DateTime.Now;
            }

            m_pageButtons = new Page_Buttons(this);
            m_pageSettings = new Page_Settings(this);
            m_pageSoundSettings = new Page_SoundSettings(this);

            btnAlwaysOnTop.IsChecked = TomatoTimerWPF.TimerSettings.Default.AlwaysOnTop;
            if (btnAlwaysOnTop.IsChecked == true)
                this.ToggleAlwaysOnTop();

            spWindowControlStackPanel.Margin = new Thickness(0, -this.Height, 0, 0);
            m_sbAniOut.Begin(spWindowControlStackPanel);

            this.ucContent.Children.Add(m_pageButtons);
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

        public void SavePropertiesAndClose(bool isSaveTimerState)
        {
            if (isSaveTimerState)
            {
                TomatoTimerWPF.TimerSettings.Default.TimerRestoreDateTime = m_TimeDateStart.ToString();
                TomatoTimerWPF.TimerSettings.Default.TimerRestoreMode = (int)m_mode;
            }
            else
            {
                TomatoTimerWPF.TimerSettings.Default.TimerRestoreDateTime = "";
                TomatoTimerWPF.TimerSettings.Default.TimerRestoreMode = (int)TimerMode.MODE_WORK;
            }
            TomatoTimerWPF.TimerSettings.Default.WindowRestoreBounds = this.RestoreBounds.ToString();
            TomatoTimerWPF.TimerSettings.Default.Save();

            m_isNormalClosingEvent = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!m_isNormalClosingEvent)
            {
                TomatoTimerWPF.TimerSettings.Default.TimerRestoreDateTime = m_TimeDateStart.ToString();
                TomatoTimerWPF.TimerSettings.Default.TimerRestoreMode = (int)m_mode;
                TomatoTimerWPF.TimerSettings.Default.WindowRestoreBounds = this.RestoreBounds.ToString();
                TomatoTimerWPF.TimerSettings.Default.Save();
            }
        }

        public void UpdateUI()
        {


            TimeSpan pauseSpan = m_TimeSpanPause;
            if (m_bIsPause)
                pauseSpan += DateTime.Now - m_TimeDatePauseStart;

            TimeSpan timerSpan, modeSpan;
            if (m_mode == TimerMode.MODE_WORK)
                modeSpan = TimeSpan.FromMinutes(TomatoTimerWPF.TimerSettings.Default.Work_Time) + 800.Milliseconds();
            else if (m_mode == TimerMode.MODE_RELAX)
                modeSpan = TimeSpan.FromMinutes(TomatoTimerWPF.TimerSettings.Default.Relax_Time) + 800.Milliseconds();
            else if (m_mode == TimerMode.MODE_RELAX_LONG)
                modeSpan = TimeSpan.FromMinutes(TomatoTimerWPF.TimerSettings.Default.Relax_Time_Long) + 800.Milliseconds();
            else
                modeSpan = 1.Seconds();

            if (m_bIsPause)
                timerSpan = modeSpan - m_TimeSpan;
            else
                timerSpan = modeSpan - (DateTime.Now - m_TimeDateStart);

            if (!m_bIsOverTime && timerSpan.IsNegativeOrZero())
            {
                SetWindowFlash(true);

                if (m_mode == TimerMode.MODE_WORK)
                    m_pageSoundSettings.playSound(Page_SoundSettings.SoundType.WorkDone);
                else
                    m_pageSoundSettings.playSound(Page_SoundSettings.SoundType.RestTimeOut);
                
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


                TextBlock timeText = new TextBlock();
                if (m_mode == TimerMode.MODE_WORK)
                {
                    time = "Work ";
                    //timeText.Inlines.Add(new Bold(new Run("W")));
                    timeText.Inlines.Add("Work  ");
                }
                else if (m_mode == TimerMode.MODE_RELAX || m_mode == TimerMode.MODE_RELAX_LONG)
                {
                    time = "Rest ";
                    //timeText.Inlines.Add(new Bold(new Run("R")));
                    timeText.Inlines.Add("Rest  ");
                }
                else
                    time = "";


                if (timerSpan.Hours != 0 || timerSpan.Days != 0)
                {
                    if ((timerSpan.Seconds & 0x1) == 0)
                        time += "{0}:{1:00} {2:00}".ToFormat(Math.Abs(timerSpan.Hours + timerSpan.Days * 24), Math.Abs(timerSpan.Minutes), Math.Abs(timerSpan.Seconds));
                    else
                        time += "{0}:{1:00}:{2:00}".ToFormat(Math.Abs(timerSpan.Hours + timerSpan.Days * 24), Math.Abs(timerSpan.Minutes), Math.Abs(timerSpan.Seconds));

                    timeText.Inlines.Add("{0}:{1:00}".ToFormat(Math.Abs(timerSpan.Hours + timerSpan.Days * 24), Math.Abs(timerSpan.Minutes)));
                    //Run minText = new Run("{0:00}".ToFormat(Math.Abs(timerSpan.Minutes)));
                    //minText.FontFamily = new System.Windows.Media.FontFamily("/TomatoTimerWPF;component/Resource/#Roboto");
                    //minText.FontWeight = FontWeights.Black;
                    //timeText.Inlines.Add(new Bold(minText));
                    timeText.Inlines.Add((timerSpan.Seconds & 0x1) == 0 ? ":" : " ");
                    timeText.Inlines.Add("{0:00}".ToFormat(Math.Abs(timerSpan.Seconds)));   
                }
                else
                {
                    if ((timerSpan.Seconds & 0x1)==0)
                        time += "{0} {1:00}".ToFormat(Math.Abs(timerSpan.Minutes), Math.Abs(timerSpan.Seconds));
                    else
                        time += "{0}:{1:00}".ToFormat(Math.Abs(timerSpan.Minutes), Math.Abs(timerSpan.Seconds));

                    Run minText= new Run("{0}".ToFormat(Math.Abs(timerSpan.Minutes)));
                    minText.FontFamily = new System.Windows.Media.FontFamily("/TomatoTimerWPF;component/Resource/#Roboto");
                    minText.FontWeight = FontWeights.Black;
                    timeText.Inlines.Add(new Bold(minText));
                    timeText.Inlines.Add((timerSpan.Seconds & 0x1)==0?":":" ");
                    timeText.Inlines.Add("{0:00}".ToFormat(Math.Abs(timerSpan.Seconds)));   
                }

                if (pageButtons.labelTimeWhite.Visibility == Visibility.Visible)
                {
                    pageButtons.labelTimeWhite.Content = time;
                    pageButtons.labelTime.Content = time;
                }
                else
                {
                    pageButtons.labelTime.Content = timeText;
                }

                

                if (pageButtons.GetIsLongMouseDown())
                {
                    if (pageButtons.btnRelax.IsPressed)
                        pageButtons.labelTime_small.Content = "Long rest";
                    else if (pageButtons.btnReset.IsPressed && m_mode == TimerMode.MODE_WORK)
                        pageButtons.labelTime_small.Content = "Skip GCal";
                    else
                        pageButtons.labelTime_small.Content = time;
                }
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

                        IntPtr hBitmap = bmp.GetHbitmap();
                        this.TaskbarItemInfo.Overlay = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                                       hBitmap,
                                                       IntPtr.Zero,
                                                       System.Windows.Int32Rect.Empty,
                                                       BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));

                        DeleteObject(hBitmap);
                    }
                    m_OverlayIconLastMin = (tMin == 0 ? timerSpan.Seconds : tMin) + (timerSpan.IsNegativeOrZero() ? 1 : 0) + (m_bIsPause ? 3 : 1);
                }


                if (m_bIsPause)
                {
                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                    this.TaskbarItemInfo.ProgressValue = 1;
                }
                else if (m_bIsOverTime)
                {
                    this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                    this.TaskbarItemInfo.ProgressValue = 1;
                }
                else
                {
                    if (progressValue > 80)
                        this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                    else
                        this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    this.TaskbarItemInfo.ProgressValue = progressValue / 100;
                }


                if (m_mode == TimerMode.MODE_WORK)
                {

                    ThumbButtonGoToWork.Visibility = Visibility.Hidden;
                    ThumbButtonTakeABreak.Visibility = Visibility.Visible;
                    ThumbButtonPause.Visibility = !m_bIsPause ? Visibility.Visible : Visibility.Hidden;
                    ThumbButtonPlay.Visibility = m_bIsPause ? Visibility.Visible : Visibility.Hidden;

                }
                else
                {
                    ThumbButtonGoToWork.Visibility = Visibility.Visible;
                    ThumbButtonTakeABreak.Visibility = Visibility.Hidden;
                    ThumbButtonPause.Visibility = Visibility.Hidden;
                    ThumbButtonPlay.Visibility = Visibility.Hidden;
                }
            }
        }

        private int GetPercentageComplete(TimeSpan elapsed, TimeSpan total)
        {
            return (int)((elapsed.TotalMilliseconds * 100 / total.TotalMilliseconds));
        }


        public void StartWork()
        {
            if (m_bIsOverTime && m_mode == TimerMode.MODE_WORK &&
                TomatoTimerWPF.TimerSettings.Default.GoogleCal_EnableEvent)
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
                TomatoTimerWPF.TimerSettings.Default.GoogleCal_EnableEvent)
                OpenGoogleCalender(m_TimeDateStart, DateTime.Now);

            SetWindowFlash(false);
            m_TimeDateStart = DateTime.Now;
            m_TimeDatePauseStart = DateTime.Now;
            m_TimeSpanPause = TimeSpan.FromMinutes(0);
            m_TimeSpan = TimeSpan.FromMinutes(0);
            m_mode = TimerMode.MODE_RELAX;

            if (m_pageButtons != null)
            {
                if (m_pageButtons.GetIsLongMouseDown())
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

            m_pageSoundSettings.playSound(Page_SoundSettings.SoundType.Pause);
            m_bIsPause = true;
            m_TimeSpan = DateTime.Now - m_TimeDateStart;
            m_TimeDatePauseStart = DateTime.Now;
            UpdateUI();
        }

        public void Resume()
        {
            TimeSpan pauseSpan;
            if (!m_bIsPause) return;

            m_pageSoundSettings.playSound(Page_SoundSettings.SoundType.Resume);
            m_bIsPause = false;
            pauseSpan = DateTime.Now - m_TimeDatePauseStart;
            m_TimeDateStart += pauseSpan;
            m_TimeSpanPause += pauseSpan;

            UpdateUI();
        }

        public void Reset()
        {
            if (m_bIsOverTime && m_mode == TimerMode.MODE_WORK && !m_pageButtons.GetIsLongMouseDown() &&
                TomatoTimerWPF.TimerSettings.Default.GoogleCal_EnableEvent)
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
            if (TomatoTimerWPF.TimerSettings.Default.GoogleCal_text.Length != 0)
            {
                String textUrlEncode = System.Web.HttpUtility.UrlEncode(TomatoTimerWPF.TimerSettings.Default.GoogleCal_text);
                //MessageBox.Show("[" +TomatoTimerWPF.TimerSettings.Default.GoogleCal_text +"]->[" + textUrlEncode +"]");
                gcalUrl += "&text=" + textUrlEncode;
            }
            if (TomatoTimerWPF.TimerSettings.Default.GoogleCal_src.Length != 0)
            {
                if (TomatoTimerWPF.TimerSettings.Default.GoogleCal_src.IndexOf("@")!=-1)
                {
                    gcalUrl += "&src=" + System.Web.HttpUtility.UrlEncode(TomatoTimerWPF.TimerSettings.Default.GoogleCal_src);
                }
                else
                    gcalUrl += "&src=" + TomatoTimerWPF.TimerSettings.Default.GoogleCal_src;
            }
                
            gcalUrl += "&dates=" + strTimeStart + "/" + strTimeEnd;
            //MessageBox.Show(gcalUrl);

            System.Diagnostics.Process.Start(gcalUrl);
        }


        public void SwitchToButtons()
        {
            if (ucContent.Children.Count == 0)
                return;
            m_taSlideOut.To = new Thickness(0, this.Height, 0, 0);
            m_taSlideIn.From = new Thickness(0, -this.Height, 0, 0);

            pages.Push(m_pageButtons);

            FrameworkElement ucCurrent = ucContent.Children[0] as FrameworkElement;
            m_sbAniOut.Begin(ucCurrent);

            
        }

        Stack<UserControl> pages = new Stack<UserControl>();

        public void SwitchToSettings()
        {
            if (ucContent.Children.Count == 0)
                return;

            m_taSlideOut.To = new Thickness(0, -this.Height, 0, 0);
            m_taSlideIn.From = new Thickness(0, this.Height, 0, 0);
            
            pages.Push(m_pageSettings);

            FrameworkElement ucCurrent = ucContent.Children[0] as FrameworkElement;
            m_sbAniOut.Begin(ucCurrent);

        }

        public void SwitchFromSettingToSound()
        {
            if (ucContent.Children.Count == 0)
                return;

            m_taSlideOut.To = new Thickness(-this.Width, 0, 0, 0);
            m_taSlideIn.From = new Thickness(this.Width, 0, 0, 0);

            pages.Push(m_pageSoundSettings);
            FrameworkElement ucCurrent = ucContent.Children[0] as FrameworkElement;
            m_sbAniOut.Begin(ucCurrent);

        }


        public void SwitchFromSoundToSetting()
        {
            if (ucContent.Children.Count == 0)
                return;

            m_taSlideOut.To = new Thickness(this.Width, 0, 0, 0);
            m_taSlideIn.From = new Thickness(-this.Width, 0, 0, 0);

            pages.Push(m_pageSettings);
            UserControl ucCurrent = ucContent.Children[0] as UserControl;
            m_sbAniOut.Begin(ucCurrent);

        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            if (pages.Count != 0)
            {
                UserControl page = pages.Pop();

                Page_Buttons pageButtons = page as Page_Buttons;
                if (pageButtons != null)
                {
                    UpdateUI();
                    btnAlwaysOnTop.Visibility = Visibility.Visible;
                }
                else
                    btnAlwaysOnTop.Visibility = Visibility.Collapsed;
                ucContent.Children.Clear();
                ucContent.Children.Add(page);

                m_sbAniIn.Begin(page);
            }
        }


        private void btnAlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleAlwaysOnTop();
            TomatoTimerWPF.TimerSettings.Default.AlwaysOnTop = this.Topmost;
        }

        private void ToggleAlwaysOnTop()
        {
            this.Topmost = btnAlwaysOnTop.IsChecked.HasValue && btnAlwaysOnTop.IsChecked.Value;
        }


        private void menuClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("menuClose_MouseLeftButtonUp");
            this.SavePropertiesAndClose(true);
        }

        private void menuCloseDontSave_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("menuCloseDontSave_Click");
            this.SavePropertiesAndClose(false);
        }

        //private void menuCloseDontSave_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    MessageBox.Show("menuCloseDontSave_MouseLeftButtonUp");
        //    m_window.SavePropertiesAndClose(false);
        //}

        //private void menuCloseDontSave_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    //menuClose.ReleaseMouseCapture();
        //    //MessageBox.Show("menuCloseDontSave_MouseEnter");
        //}

        private void menuCloseSave_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("menuCloseSave_Click");
            this.SavePropertiesAndClose(true);
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            m_taSlideIn.From = new Thickness(0, 0, 0, 0);
            m_sbAniIn.Begin(spWindowControlStackPanel);

        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            m_taSlideOut.To = new Thickness(0, 0, 0, 0);
            m_sbAniOut.Begin(spWindowControlStackPanel);
        }

        private void ThumbButtonPlay_Click(object sender, EventArgs e)
        {
            Resume();           
        }

        private void ThumbButtonPause_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void ThumbButtonReset_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void ThumbButtonGoToWork_Click(object sender, EventArgs e)
        {
            StartWork();
        }

        private void ThumbButtonTakeABreak_Click(object sender, EventArgs e)
        {
            StartRelax();
        }
    }
}
