using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace App38
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MediaCapture _capture;
        private readonly DispatcherTimer _timer;
        private readonly Storyboard myStoryboard;
        private DispatcherTimer gridTimer;
        private bool initialized;
        private Recording newRecording;
        private StorageFile recordStorageFile;
        private bool recording;
        private System.DateTime time;

        public bool RegisteredForShare { get; set; }
        public static MainPage Current { get; set; }
         
        public MainPage()
        {
            this.InitializeComponent();
            this.initialized = false;
            this._capture = new MediaCapture();
            this.time = new System.DateTime();
            MainPage mainpage = this;
            mainpage.Loaded += this.MainPage_Loaded;
            this.btnRecord.Click += onStartRecord;
            this.btnLibrary.Click += onLibraryClick;
            this.backButton.Click += GoBack;
            DispatcherTimer dispatcherTimer1 = new DispatcherTimer();
            dispatcherTimer1.Interval = new TimeSpan(0, 0, 1);
            this._timer = dispatcherTimer1;
            DispatcherTimer dispatcherTimer2 = this._timer;
            dispatcherTimer2.Tick += this.TickEvent;
            DoubleAnimation doubleAnimation1 = new DoubleAnimation();
            doubleAnimation1.From = new Double?(100);
            doubleAnimation1.To = new Double?(0.0);
            doubleAnimation1.EnableDependentAnimation = true;
            doubleAnimation1.Duration = new Duration(System.TimeSpan.FromSeconds(6.0));
            DoubleAnimation doubleAnimation2 = doubleAnimation1;

            this.myStoryboard = new Storyboard();
            Storyboard.SetTargetName((Timeline)this.myStoryboard, "txtRecordingName");
            Storyboard.SetTargetProperty((Timeline)doubleAnimation2, "Opacity");
            ((ICollection<Timeline>)this.myStoryboard.Children).Add((Timeline)doubleAnimation2);
            MediaCapture mediaCapture = this._capture;
            mediaCapture.RecordLimitationExceeded += this._capture_RecordLimitationExceeded;
            SettingsPane forCurrentview = SettingsPane.GetForCurrentView();
            forCurrentview.CommandsRequested += this.onCommandsRequested;
            this.btnLibrary.Click += this.onLibraryClick;
            this.btnRecord.Click += this.onStartRecord;






    }
        private void GoBack(object sender, RoutedEventArgs e)
        {
            if (this.Frame == null || !this.Frame.CanGoBack)
                return;
            this.Frame.GoBack();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dispatcherTimer1 = new DispatcherTimer();
            dispatcherTimer1.Interval = new System.TimeSpan(0, 0, 1);
            this.gridTimer = dispatcherTimer1;
            DispatcherTimer dispatcherTimer2 = this.gridTimer;
            dispatcherTimer2.Tick += this.gridTimer_Tick;
            this.gridTimer.Start();
        }
        private void gridTimer_Tick(object sender,object e)
        {
            gridTimer.Stop();

        }

        private void TickEvent(object sender,object e)
        {
            this.time = this.time.AddSeconds(1.0);
            int hour = this.time.Hour;
            int minute = this.time.Minute;
            int seconds = this.time.Second;
            this.txtHours.Text = hour.ToString();
            if (this.txtHours.Text.Length <= 1)
                this.txtHours.Text = this.txtHours.Text.Insert(0, "0");

            this.txtMinutes.Text = minute.ToString();
            if (this.txtMinutes.Text.Length <= 1)
                this.txtMinutes.Text = this.txtMinutes.Text.Insert(0, "0");

            this.txtSeconds.Text = seconds.ToString();
            if (this.txtSeconds.Text.Length > 1)
                return;
            this.txtSeconds.Text = this.txtSeconds.Text.Insert(0, "0");

        }

        private async void _capture_RecordLimitationExceeded(MediaCapture sender)
        {
            IUICommand uiCommand = await new MessageDialog("You exceeded your record limitation!").ShowAsync();
        }

        private async void onStartRecord(object sender,RoutedEventArgs e)
        {
            try
            {

                if (!this.recording)
                {
                    if (!this.initialized)
                        await this.initializeMic();
                    string filetime = "Audio Recording " + (object)System.DateTime.Now.Hour + "-" + (object)System.DateTime.Now.Minute + "-" + System.DateTime.Now.Second;
                    string filename = filetime + ".mp3";
                    this.newRecording = new Recording();
                    bool exceptionCaught = false;
                    bool fileExists = false;
                    StorageFolder storageFolder = (StorageFolder)null;
                    foreach(StorageFolder storageFolder1 in (IEnumerable<StorageFolder>) await KnownFolders.MusicLibrary.GetFoldersAsync())
                    {
                        if(string.Compare(storageFolder1.Name, "Audio Recordings",StringComparison.Ordinal) == 0)
                        {
                            storageFolder = storageFolder1;
                            break;
                        }
                    }

                    if (storageFolder == null)
                        storageFolder = await KnownFolders.MusicLibrary.CreateFolderAsync("Audio Recordings");

                    try
                    {
                        ((MainPage)this).recordStorageFile = await storageFolder.CreateFileAsync(filename);

                    }catch(Exception ex)
                    {
                        exceptionCaught = true;
                        this.recordStorageFile = (StorageFile)null;
                        if(ex.Message.Contains("file already exists"))
                        {
                            filename = filetime + new Guid().ToString() + ".mp3";
                            fileExists = true;

                        }
                        


                    }
                    if (exceptionCaught && fileExists)
                        this.recordStorageFile = await KnownFolders.MusicLibrary.CreateFileAsync(filename);
                    if (this.recordStorageFile == null)
                        return;

                    this.newRecording.Title = filename.Substring(0, filename.Length - 4);
                    MediaEncodingProfile recordProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto);
                    this.recording = true;
                    this.btnRecord.Foreground = (Brush)new SolidColorBrush(Colors.Red);
                    this._timer.Start();
                    this.btnRecord.SetValue(AutomationProperties.NameProperty, (object)"Stop Recording");
                    await this._capture.StartRecordToStorageFileAsync(recordProfile, (IStorageFile)this.recordStorageFile);



                }
                else
                {
                    this.StopRecording();
                }

            }catch(Exception ex)
            {
                this.recording = false;
                this._timer.Stop();
                if (this.recordStorageFile != null)
                    this.recordStorageFile.DeleteAsync();
                this.btnRecord.Foreground = (Brush)new SolidColorBrush(Colors.Green);
                this.btnRecord.SetValue(AutomationProperties.NameProperty, "Start Recording");
                this.pageTitle.Text =   ex.Message;


            }
        }
        private async void StopRecording()
        {
            this._timer.Stop();
            this.newRecording.Length = this.txtHours.Text + ":" + this.txtMinutes.Text + ":" + this.txtSeconds.Text;
            this.txtRecordingName.Text = "The file " + newRecording.Title + " was saved in your music library.\r\n Right click or swipe from the top to play your recordings.";
            int seconds = Convert.ToInt32(this.txtSeconds.Text);
            this.storyboard.Begin();
            if(seconds <= 1)
            {
                this.recording = false;
                IUICommand uiCommand = await new MessageDialog("Recording Too Small").ShowAsync();
                await this.recordStorageFile.DeleteAsync();
                this.btnRecord.Foreground = (Brush)new SolidColorBrush(Colors.Green);
                this.btnRecord.SetValue(AutomationProperties.NameProperty, "Start Recording");

            }
            else
            {
                try
                {
                    await this._capture.StopRecordAsync();
                }
                catch(Exception ex)
                {
                    this.pageTitle.Text += ex.StackTrace + "  " + ex.Message;

                }
                this.time = new System.DateTime();
                this.btnRecord.Foreground = (Brush)new SolidColorBrush(Colors.Green);
                this.btnRecord.SetValue(AutomationProperties.NameProperty, "Start Recording");
                
                IRandomAccessStream stream = await this.recordStorageFile.OpenAsync(FileAccessMode.Read);
                this.newRecording.Stream = stream;
                this.newRecording.FileType = this.recordStorageFile.FileType;

                RecordingSource recordingSource = ((IDictionary<object, object>)Application.Current.Resources)[(object)"recordingsSource"] as RecordingSource;
                this.txtRecordingName.Focus(FocusState.Programmatic);
                if(recordingSource != null)
                {
                    this.newRecording.Position = (recordingSource.Recordings.Count + 1).ToString();
                    recordingSource.Recordings.Add(newRecording);

                }
                else
                {
                    recordingSource = new RecordingSource();
                    this.newRecording.Position = "1";
                    recordingSource.Recordings.Add(this.newRecording);

                }

                this.recording = false;

            }

        }

        private async Task initializeMic()
        {
            MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings()
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio
            };
            await this._capture.InitializeAsync();
            this.recording = false;
            this.initialized = true;

        


        }

        private void onLibraryClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Frame.Navigate(typeof(RecordingsPage));

            }
            catch
            {

            }

        }
        private void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            
        }

    }
        

        }

    

