using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AllPlayMediaPlayer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        AllPlay.Service s;
        public MainPage()
        {
            this.InitializeComponent();
            //mediaElement.TransportControls = controls;
            s = new AllPlay.Service(mediaElement);
            s.Start();

            var systemMediaControls = SystemMediaTransportControls.GetForCurrentView();
            //mediaElement.TransportControls = systemMediaControls;
            systemMediaControls.ButtonPressed += SystemControls_ButtonPressed;
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
