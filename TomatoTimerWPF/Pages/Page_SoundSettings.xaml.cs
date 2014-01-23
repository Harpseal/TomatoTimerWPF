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

using WMPLib;

namespace TomatoTimerWPF.Pages
{
    using Res = Properties.Resources;
    /// <summary>
    /// Interaction logic for Page_SoundSettings.xaml
    /// </summary>
    public partial class Page_SoundSettings : UserControl
    {
        private MainWindow m_window;
        private WindowsMediaPlayer m_wmplayer = null;
        //private System.Media.SoundPlayer[] m_ResourcePlayer;

        private Button[] m_aBtnPlay;
        private Button[] m_aBtnStop;
        private Button[] m_aBtnOpenFile;
        private Button[] m_aBtnMute;

        public enum SoundType : int 
        {
            Resume = 0,
            Pause = 1,
            WorkDone = 2,
            RestTimeOut = 3
        }

        public enum SoundState : int
        {
            Resource = 0,
            Mute = 1,
            File = 2
        }

        private SoundState[] m_aSoundState;


        public Page_SoundSettings(MainWindow window)
        {
            InitializeComponent();
            m_window = window;

            m_aBtnPlay = new Button[4] { btnSoundResumePlay, btnSoundPausePlay, btnSoundWorkPlay, btnSoundRestPlay };
            m_aBtnStop = new Button[4] { btnSoundResumeStop, btnSoundPauseStop, btnSoundWorkStop, btnSoundRestStop };
            m_aBtnOpenFile = new Button[4] { btnSoundResumeOpenFile, btnSoundPauseOpenFile, btnSoundWorkOpenFile, btnSoundRestOpenFile };
            m_aBtnMute = new Button[4] { btnSoundResumeMute, btnSoundPauseMute, btnSoundWorkMute, btnSoundRestMute };

            m_aSoundState = new SoundState[4];
            //m_ResourcePlayer = new System.Media.SoundPlayer[4];

            for (int i = 0; i < 4; i++)
            {
                //m_ResourcePlayer[i] = null;
                m_aSoundState[i] = getSoundState(getSoundPathProperties((SoundType)i));
                SyncStateToPlayer((SoundType)i, m_aSoundState[i]);
                SyncStateToUI((SoundType)i, m_aSoundState[i]);
            }
        }

        private SoundState getSoundState(string path)
        {
            if (path.Length == 0)
                return SoundState.Mute;
            else
            {
                if (path == SoundState.Mute.ToString())
                    return SoundState.Mute;
                if (path == SoundState.Resource.ToString())
                    return SoundState.Mute;

                if (System.IO.File.Exists(path))
                {
                    return SoundState.File;
                }
                else
                    return SoundState.Mute;
            }
        }

        private void SyncStateToUI(SoundType type, SoundState state)
        {
            switch (state)
            {
                case SoundState.Resource:
                case SoundState.File:
                    m_aBtnPlay[(int)type].Visibility = Visibility.Visible;
                    m_aBtnStop[(int)type].Visibility = Visibility.Collapsed;
                    m_aBtnOpenFile[(int)type].Visibility = Visibility.Visible;
                    m_aBtnMute[(int)type].Visibility = Visibility.Visible;
                    break;
                case SoundState.Mute:
                    m_aBtnPlay[(int)type].Visibility = Visibility.Collapsed;
                    m_aBtnStop[(int)type].Visibility = Visibility.Collapsed;
                    m_aBtnOpenFile[(int)type].Visibility = Visibility.Visible;
                    m_aBtnMute[(int)type].Visibility = Visibility.Collapsed;
                    break;
            }

        }

        private void SyncStateToPlayer(SoundType type, SoundState state)
        {
            switch (state)
            {
                case SoundState.Resource:
                    //if (m_ResourcePlayer[(int)type]==null)
                    //{
                    //    switch (type)
                    //    {
                    //        case SoundType.Pause:
                    //            ;//m_ResourcePlayer[(int)type] = new System.Media.SoundPlayer(Res.Bellatrix_Pause);
                    //            break;
                    //        case SoundType.Resume:
                    //            ;//m_ResourcePlayer[(int)type] = new System.Media.SoundPlayer(Res.Pollux_Resume);
                    //            break;
                    //        case SoundType.WorkDone:
                    //            ;//m_ResourcePlayer[(int)type] = new System.Media.SoundPlayer(Res.CanisMajor_WorkDone);
                    //            break;
                    //        case SoundType.RestTimeOut:
                    //            ;//m_ResourcePlayer[(int)type] = new System.Media.SoundPlayer(Res.Fermium_RestTimeOut);
                    //            break;
                    //    }
                    //}
                    break;

                case SoundState.File:
                    if (m_wmplayer == null)
                    {
                        m_wmplayer = new WMPLib.WindowsMediaPlayer();
                        m_wmplayer.PlayStateChange += 
                            new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
                    }
                    break;

            }

        }

        private string getSoundPathProperties(SoundType type)
        {
            switch (type)
            {
                case SoundType.Resume:
                    return TomatoTimerWPF.Properties.Settings.Default.SoundPathResume;
                case SoundType.Pause:
                    return TomatoTimerWPF.Properties.Settings.Default.SoundPathPause;
                case SoundType.WorkDone:
                    return TomatoTimerWPF.Properties.Settings.Default.SoundPathTimeOutWork;
                case SoundType.RestTimeOut:
                    return TomatoTimerWPF.Properties.Settings.Default.SoundPathTimeOutRest;
                default:
                    return "";
            }
        }

        private void setSoundPathProperties(SoundType type,string stateString)
        {
            switch (type)
            {
                case SoundType.Resume:
                    TomatoTimerWPF.Properties.Settings.Default.SoundPathResume = stateString;
                    break;
                case SoundType.Pause:
                    TomatoTimerWPF.Properties.Settings.Default.SoundPathPause = stateString;
                    break;
                case SoundType.WorkDone:
                    TomatoTimerWPF.Properties.Settings.Default.SoundPathTimeOutWork = stateString;
                    break;;
                case SoundType.RestTimeOut:
                    TomatoTimerWPF.Properties.Settings.Default.SoundPathTimeOutRest = stateString;
                    break;
            }
        }

        private void Player_PlayStateChange(int NewState)
        {
            if ((WMPLib.WMPPlayState)NewState == WMPLib.WMPPlayState.wmppsStopped ||
                (WMPLib.WMPPlayState)NewState == WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                for (int i = 0; i < 4; i++)
                {
                    SyncStateToUI((SoundType)i, m_aSoundState[i]);
                }
            }
            else if ((WMPLib.WMPPlayState)NewState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                btnSoundPausePlay.Visibility = Visibility.Collapsed;
                btnSoundResumePlay.Visibility = Visibility.Collapsed;
                btnSoundWorkPlay.Visibility = Visibility.Collapsed;
                btnSoundRestPlay.Visibility = Visibility.Collapsed;

                btnSoundPauseStop.Visibility = Visibility.Visible;
                btnSoundResumeStop.Visibility = Visibility.Visible;
                btnSoundWorkStop.Visibility = Visibility.Visible;
                btnSoundRestStop.Visibility = Visibility.Visible;
            }
        }

        public void playSound(SoundType type)
        {
            if ((int)type >= 4 || (int)type < 0) return;

            SyncStateToPlayer(type, m_aSoundState[(int)type]);
            switch (m_aSoundState[(int)type])
            { 
                case SoundState.File:
                    m_wmplayer.URL = getSoundPathProperties(type);
                    m_wmplayer.controls.play();
                    break;
                case SoundState.Resource:
                    //m_ResourcePlayer[(int)type].Play();
                    break;
            }

        }

        private void btnSound_Click(object sender, RoutedEventArgs e)
        {
            Button btnSelf = (sender as Button); 

            btnSelf.ContextMenu.IsEnabled = true;
            btnSelf.ContextMenu.PlacementTarget = btnSelf;
            btnSelf.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
            btnSelf.ContextMenu.VerticalOffset = (btnSelf.ActualHeight);
            //btnSelf.ContextMenu.HorizontalOffset = (btnSelf.ContextMenu.ActualWidth / 2);
            btnSelf.ContextMenu.IsOpen = true;

        }

        private void btnGotoSetting_Click(object sender, RoutedEventArgs e)
        {
            if (m_wmplayer != null && m_wmplayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
                m_wmplayer.controls.stop();
            m_window.SwitchFromSoundToSetting();
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

        private void btnSoundPlay_Click(object sender, RoutedEventArgs e)
        {
            Button btnSelf = sender as Button;
            SoundType type;
            for (int i=0;i<4;i++)
            {
                if (btnSelf == m_aBtnPlay[i])
                {
                    type = (SoundType)i;
                    playSound(type);
                    return;
                }
            }
        }

        private void btnSoundStop_Click(object sender, RoutedEventArgs e)
        {
            if (m_wmplayer == null) return;
            if (m_wmplayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
                m_wmplayer.controls.stop();
                
        }

        private void btnSoundOpenFile_Click(object sender, RoutedEventArgs e)
        {
            Button btnSelf = sender as Button;
            SoundType type;
            SoundState state;

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();

            //dialog.Filter = "Sound files (*.wma;*.mp3;*.wav;*.ogg)|*.wma;*.mp3;*.wav;*.ogg";
            dialog.Filter = "Sound files (*.wma;*.mp3;*.wav)|*.wma;*.mp3;*.wav";

            //設定起始目錄為程式目錄
            dialog.InitialDirectory = System.Windows.Forms.Application.StartupPath;

            //設定dialog的Title
            dialog.Title = "Select a sound file";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (btnSelf == m_aBtnOpenFile[i])
                    {
                        type = (SoundType)i;
                        m_aSoundState[i] = SoundState.File;
                        SyncStateToUI(type, SoundState.File);
                        setSoundPathProperties(type, dialog.FileName);
                        playSound(type);
                        return;
                    }
                }
            }
            //else 
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        if (btnSelf == m_aBtnOpenFile[i])
            //        {
            //            type = (SoundType)i;
            //            m_aSoundState[i] = SoundState.Mute;
            //            SyncStateToUI(type, SoundState.Resource);
            //            setSoundPathProperties(type, SoundState.Resource.ToString());
            //            playSound(type);
            //            return;
            //        }
            //    }
            //}
        }

        private void btnSoundMute_Click(object sender, RoutedEventArgs e)
        {
            Button btnSelf = sender as Button;
            SoundType type;
            for (int i = 0; i < 4; i++)
            {
                if (btnSelf == m_aBtnMute[i])
                {
                    type = (SoundType)i;
                    m_aSoundState[i] = SoundState.Mute;
                    SyncStateToUI(type, SoundState.Mute);
                    setSoundPathProperties(type, SoundState.Mute.ToString());
                    return;
                }
            }
        }
        
    }
}
