using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace AllPlayMediaPlayer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        AllPlay.Service s;
        private bool isSingleWindowDevice;
        public MainPage()
        {
            this.InitializeComponent();
            //mediaElement.TransportControls = controls;
            s = new AllPlay.Service(mediaElement);
            s.Start();

            isSingleWindowDevice = 
                Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT" ||
                Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile" ||
                Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.XBox";

            var systemMediaControls = SystemMediaTransportControls.GetForCurrentView();
            //mediaElement.TransportControls = systemMediaControls;
            systemMediaControls.ButtonPressed += SystemControls_ButtonPressed;
            mediaElement.CurrentStateChanged += MediaElement_CurrentStateChanged;
        }

        private void MediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            UpdateScreensaverSettings();
        }

        private DisplayRequest appDisplayRequest;

        private void UpdateScreensaverSettings()
        {
            bool enableScreenSaver = false;
            if (mediaElement.IsFullWindow)
            {
                enableScreenSaver = true;
            }
            else if (isSingleWindowDevice)
            {
                enableScreenSaver = true;
            }
            if (mediaElement.IsAudioOnly == false) //Disable system screensaver if playing video
            {
                if (mediaElement.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
                {
                    if (isSingleWindowDevice) mediaElement.IsFullWindow = true; //on single window devices, go full screen for video
                    if (appDisplayRequest == null)
                    {
                        // This call creates an instance of the DisplayRequest object. 
                        appDisplayRequest = new DisplayRequest();
                        appDisplayRequest.RequestActive();
                        enableScreenSaver = false;
                    }
                }
                else // CurrentState is Buffering, Closed, Opening, Paused, or Stopped. 
                {
                    if (appDisplayRequest != null)
                    {
                        // Deactivate the display request and set the var to null.
                        appDisplayRequest.RequestRelease();
                        appDisplayRequest = null;
                    }
                }
            }
            else
            {
                if (isSingleWindowDevice) mediaElement.IsFullWindow = false;
            }

            Screensaver.IsScreensaverEnabled = enableScreenSaver;
            Screensaver.ScreensaverUri = Playlist.CurrentItem?.ThumbnailUrl;
        }

        public AllPlay.Playlist Playlist
        {
            get
            {
                return s.Playlist;
            }
        }

        private void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
           switch(args.Button)
            {
                case SystemMediaTransportControlsButton.Next:
                    s.Playlist.MoveNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    s.Playlist.MovePrevious();
                    break;
                default: break;
            }
        }
    }
}
