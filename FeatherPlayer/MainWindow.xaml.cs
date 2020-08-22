﻿using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using System.Windows.Threading;

namespace FeatherPlayer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Geometry pausedata, continuedata;//initialize the icons
        MusicPlayer player;
        public MainWindow()
        {
            string continuedatastr = "M10,16.5V7.5L16,12M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
            string pausestr = "M15,16H13V8H15M11,16H9V8H11M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
            continuedata = Geometry.Parse(continuedatastr);
            pausedata = Geometry.Parse(pausestr);

            player = new MusicPlayer();
            player.PlaybackStopped += Player_PlaybackStopped;
              
            InitializeComponent();
            //playGrid.Visibility = Visibility.Hidden;
        }

        public enum playStatus
        {
            Playing,
            Paused,
            Unloaded
        }

        private void wndMain_Loaded(object sender, RoutedEventArgs e)
        {
            Blur.EnableBlur(this);
            //AudioInfo.wav.WavInfo wi = AudioInfo.wav.GetWavInfo("test.wav");
            //MessageBox.Show("Test");
        }

        private void btnExit_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation daToRed = new DoubleAnimation() {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            btnExitBackground.BeginAnimation(OpacityProperty, daToRed);
        }

        private void btnExit_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation daToTrans = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            btnExitBackground.BeginAnimation(OpacityProperty, daToTrans);
        }

        private void btnExit_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            btnExitBackground.Background = Brushes.OrangeRed;
            player.Stop();
            Application.Current.Shutdown();
        }      

        private void PlayStop_MouseEnter(object sender, MouseEventArgs e)
        {
            btnOpacity.FloatElement(btnPlayStop, 0.9, 200);
            btnMove.ScaleEasingAnimationShow(btnPlayStop, 1, 0.9, 500);
        }

        private void PlayStop_MouseLeave(object sender, MouseEventArgs e)
        {
            btnOpacity.FloatElement(btnPlayStop, 1, 200);
            btnMove.ScaleEasingAnimationShow(btnPlayStop, 0.9, 1, 500);
        }

        private void Back_MouseEnter(object sender, MouseEventArgs e)
        {
            btnOpacity.FloatElement(Back, 0.9, 200);
            btnMove.ScaleEasingAnimationShow(Back, 1, 0.9, 500);
        }

        private void Back_MouseLeave(object sender, MouseEventArgs e)
        {
            btnOpacity.FloatElement(Back, 1, 200);
            btnMove.ScaleEasingAnimationShow(Back, 0.9, 1, 500);
        }

        private void Next_MouseEnter(object sender, MouseEventArgs e)
        {
            btnOpacity.FloatElement(Next, 0.9, 200);
            btnMove.ScaleEasingAnimationShow(Next, 1, 0.9, 500);
        }

        private void Next_MouseLeave(object sender, MouseEventArgs e)
        {
            btnOpacity.FloatElement(Next, 1, 200);
            btnMove.ScaleEasingAnimationShow(Next, 0.9, 1, 500);
        }

        private void SongPic_MouseEnter(object sender, MouseEventArgs e)
        {
            btnOpacity.FloatElement(SongPic, 0.9, 200);
            btnMove.ScaleEasingAnimationShow(SongPic, 1, 0.9, 500);
        }

        private void SongPic_MouseLeave(object sender, MouseEventArgs e)
        {
            btnOpacity.FloatElement(SongPic, 1, 200);
            btnMove.ScaleEasingAnimationShow(SongPic, 0.9, 1, 500);
        }
        DispatcherTimer timer = null;
        int Songtime;//歌曲当前长度 timer要用
        private void PlayStop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string fileName;          
            //int stream;
            switch (player.PlaybackState)
            {
                case PlaybackState.Stopped:
                    //nothing loaded

                    OpenFileDialog opfflac = new OpenFileDialog()
                    {
                        Title = "No music loaded. Please select a valid audio file.",
                        Filter = CodecFactory.SupportedFilesFilterEn //"FreeLosslessAudioCodec|*.flac|MPEG-3|*.mp3|Wave|*.wav"
                    };
                    
                    if (opfflac.ShowDialog() == true)
                    {
                        string strFileName = opfflac.FileName;
                        string dirName = Path.GetDirectoryName(strFileName);
                        string SongName = Path.GetFileName(strFileName);//获得歌曲名称
                        FileInfo fInfo = new FileInfo(strFileName);                     
                        var mmdeviceEnumerator = new MMDeviceEnumerator();
                        MMDevice device = mmdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);                       
                        fileName = opfflac.FileName;                  
                        player.Open(fileName, device);
                        sliSong.Maximum = player.Length.Milliseconds;
                        Songtime = player.Position.Milliseconds;
                        player.Play();
                        player.Volume = 10;
                        PlayStop.Data = pausedata;
                        timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromMilliseconds(1000);
                        timer.Tick += new EventHandler(timer_tick);
                        timer.Start();
                    }
                    break;
                case PlaybackState.Playing: //点击暂停时
                    PlayStop.Data = continuedata;
                    player.Pause();
                    break;
                case PlaybackState.Paused: //点击继续时
                    PlayStop.Data = pausedata;
                    player.Play();
                    break;
            }
        }
        private void timer_tick(object sender, EventArgs e) //timer同步进度条
        {
            sliSong.Value = Songtime;
        }
        /// <summary>
        /// move window
        /// </summary>
        /// <param name="sender"></param>
        /// 
        /// <param name="e"></param>
        private void gridTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch { }
        }

        private void frmPages_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Player_PlaybackStopped(object sender, PlaybackStoppedEventArgs e)
        {
            PlayStop.Data = continuedata;
        }
    }
}
