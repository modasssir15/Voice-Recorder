using App38.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace App38
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class RecordingsPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly DispatcherTimer timer;
        private DispatcherTimer gridTimer;


        public static RecordingsPage Current { get; set; }

        public RecordingsPage.PlayerState State { get; set; }



        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public RecordingsPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            RecordingsPage.Current = this;
            MediaElement mediaElement1 = this.playback;
            mediaElement1.MediaEnded += this.PlaybackEnded;

            RecordingSource recordingSource =  ((IDictionary<object, object>)Application.Current.Resources)[(object)"recordingsSource"] as RecordingSource;
            if (recordingSource != null)
                this.playListBox.DataContext = (object)recordingSource.Recordings;
            this.State = RecordingsPage.PlayerState.Stopped;
            DispatcherTimer dispatcherTimer1 = new DispatcherTimer();
            dispatcherTimer1.Interval = new System.TimeSpan(500L);
            this.timer = dispatcherTimer1;
            DispatcherTimer dispatcherTimer2 = this.timer;
            dispatcherTimer2.Tick += this.timer_Tick;
            MediaElement mediaElement2 = this.playback;
            mediaElement2.MediaEnded += this.playback_MediaEnded;
            Point point = new Point();
            Rect bounds = Window.Current.Bounds;
            point.Width = bounds.Width;
            point.Height = bounds.Height;
            MainPage current = MainPage.Current;
            RecordingsPage recordingsPage = this;
            recordingsPage.Loaded += this.RecordingsPage_Loaded;
            this.ParentedPopup.Closed += this.PopupClosed;
            this.playListBox.DoubleTapped += this.ChangeSong;
            this.playListBox.SelectionChanged += this.PlayListBox_SelectionChanged_1;
            this.btnSkipBack.Click += this.SkipBackClick;
            this.btnPlay.Click += this.OnPlayClick;
            this.btnStop.Click += this.StopRecording;
            this.backButton.Click += this.GoBack;




        }
        void GoBack(object sender, RoutedEventArgs e)
        {
            if (this.Frame == null || !this.Frame.CanGoBack)
                return;
            this.Frame.GoBack();

        }

        private void StopRecording(object sender, RoutedEventArgs e)
        {
            try
            {
                this.timer.Stop();
                this.playback.Stop();
                this.txtDuration.Text = "00:00:00";
                this.State = RecordingsPage.PlayerState.Stopped;
                this.btnPlay.Style = (Style)((IDictionary<object, object>)Application.Current.Resources)[(object)"PlayAppBarButtonStyle"];
            }
            catch(Exception ex)
            {
                new MessageDialog("I am afraid , I can't let you do that " + ex.Message).ShowAsync();

            }
        }
        private void SkipBackClick(object sender,RoutedEventArgs e)
        {
            try
            {
                Recording recording = this.playListBox.SelectedItem as Recording;
                RecordingSource recordingSource = ((IDictionary<object, object>)Application.Current.Resources)[(object)"recordingsSource"] as RecordingSource;
                if (recording == null || recordingSource == null)
                    return;

                int int32 = Convert.ToInt32(recording.Position.TrimEnd('.'));
                if (int32 > 1)
                    this.playListBox.SelectedIndex = int32 - 2;
                if (this.State == RecordingsPage.PlayerState.Playing)
                    this.PlayRecording(this.playListBox.SelectedItem as Recording);
                else
                    this.playback.Stop();

            }
            catch(Exception ex)
            {
                new MessageDialog("I am afraid I can't let you do that " + ex.Message).ShowAsync();
            }
        }


        private void RecordingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dispatcherTimer1 = new DispatcherTimer();
            dispatcherTimer1.Interval = new System.TimeSpan(0, 0, 1);
            this.gridTimer = dispatcherTimer1;
            DispatcherTimer dispatcherTimer2 = this.gridTimer;
            dispatcherTimer2.Tick += this.gridTimer_Tick;
            this.gridTimer.Start();

        }
        private void gridTimer_Tick(object sender, object e)
        {
            gridTimer.Stop();
        }

        private void sender_TargetApplicationChosen(DataTransferManager sender, TargetApplicationChosenEventArgs args)
        {
            Point point = ((IDictionary<object, object>)Application.Current.Resources)[(object)"recordingSent"] as Point;
            if (point == null)
                return;

            point.Height = 200.0;
            ((IDictionary<object, object>)Application.Current.Resources)[(object)"recordingSent"] = (object)point;
        }

        private void showPopUp()
        {
            if (!this.ParentedPopup.IsOpen)
            {
                this.ParentedPopup.IsLightDismissEnabled = true;
                this.ParentedPopup.IsOpen = true;
                this.ParentedPopup.Visibility = Visibility.Visible;
            }
            else
            {
                this.ParentedPopup.IsOpen = false;
                this.showPopUp();

            }
        }

        private void playback_MediaEnded(object sender,RoutedEventArgs e)
        {
            try{

                this.timer.Stop();
                this.txtDuration.Text = "00:00:00";


            }catch(Exception ex)
            {
                new MessageDialog("I am afraid , I can't let you do that " + ex.Message).ShowAsync();
            }
        }
        private void timer_Tick(object sender, object e)
        {
            try
            {
                System.TimeSpan position = this.playback.Position;
                string str1 = position.Hours.ToString();
                string str2 = position.Minutes.ToString();
                string str3 = position.Seconds.ToString();
                if (position.Hours < 10)
                    str1 = str1.Insert(0, "0");
                if (position.Minutes < 10)
                    str2 = str2.Insert(0, "0");
                if (position.Seconds < 10)
                    str3 = str3.Insert(0, "0");
                this.txtDuration.Text = str1 + ":" + str2 + ":" + str3;

            }
            catch(Exception ex)
            {
                new MessageDialog("I'm afraid I can't let you do that " + ex.Message).ShowAsync();
            }
        }

        private void PlaybackEnded(object sender,RoutedEventArgs e)
        {
            this.btnPlay.Style = (Style)((IDictionary<object, object>)Application.Current.Resources)[(object)"PlayAppBarButtonStyle"];
            this.State = RecordingsPage.PlayerState.Stopped;

        }

        private void PlayListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.State != RecordingsPage.PlayerState.Stopped)
                    return;
                Recording recording = this.playListBox.SelectedItem as Recording;
                if (recording == null)
                    return;
                this.txtRecordingTitle.Text = recording.Title;

            }
            catch(Exception ex)
            {
                new MessageDialog("I am afraid I can't let you do that! " + ex.Message).ShowAsync();
            }
        }

        public  void LoadState(object navigationParameter , Dictionary<string,object> pageState)
        {

            RecordingSource recordingSource = ((IDictionary<object, object>)Application.Current.Resources)[(object)"recordingsSource"] as RecordingSource;
            if (recordingSource == null)
                return;
            this.playListBox.DataContext = (object)recordingSource.Recordings;


        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            try{
                if(this.State == RecordingsPage.PlayerState.Playing)
                {
                    this.btnPlay.Style = (Style)((IDictionary<object, object>)Application.Current.Resources)[(object)"PlayAppBarButtonStyle"];
                    this.playback.Pause();
                    this.State = RecordingsPage.PlayerState.Paused;
                    this.timer.Stop();


                }
                else
                {
                    if (this.State == RecordingsPage.PlayerState.Stopped)
                        this.PlayRecording(this.playListBox.SelectedItem as Recording);
                    if (this.State != RecordingsPage.PlayerState.Paused)
                        return;
                    this.playback.Play();
                    this.timer.Start();
                    this.State = RecordingsPage.PlayerState.Playing;
                    this.btnPlay.Style = (Style)((IDictionary<object, object>)Application.Current.Resources)[(object)"PauseAppBarButtonStyle"];


                }

            }
            catch(Exception ex)
            {
                new MessageDialog("I am afraid I can't let you do that " + ex.Message).ShowAsync();

            }
        }

        private async void PlayRecording(Recording recording)
        {
            try
            {
                if(recording != null)
                {
                    try
                    {
                        this.playback.SetSource(recording.Stream, recording.FileType);
                        this.playback.Play();
                    }
                    catch (Exception ex)
                    {
                        new MessageDialog(" plaback " + ex.Message).ShowAsync();
                    }
            if (timer.IsEnabled)
                        this.timer.Stop();
                    this.timer.Start();
                    this.txtRecordingTitle.Text = recording.Title;
                    
                    this.State = RecordingsPage.PlayerState.Playing;


                }
                else
                {
                    IUICommand command = await new MessageDialog("You must select a recording or create one").ShowAsync();

                }
            }
            catch(Exception ex)
            {
                new MessageDialog("I am afraid i can't let you do that " + ex.Message).ShowAsync();
            }
        }
        private void ChangeSong(object sender,RoutedEventArgs e)
        {
            string title = ( ((IDictionary<object, object>)Application.Current.Resources)[(object)"recordingsSource"] as RecordingSource).Recordings[0].Title;
            this.PlayRecording(this.playListBox.SelectedItem as Recording);

        }
        public static Rect GetElementRect(FrameworkElement element)
        {
            try
            {
                return new Rect(element.TransformToVisual((UIElement)null).TransformPoint(new Windows.Foundation.Point()), new Size(element.ActualWidth, element.ActualHeight));

            }
            catch(Exception ex)
            {
                new MessageDialog("I'm afraid I can't let you do that!" + ex.Message).ShowAsync();
                return new Rect();
            }
        }
        private void PopupClosed(object sender, object e)
        {
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

       
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        public enum PlayerState
        {
            Playing,
            Paused,
            Stopped,
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
