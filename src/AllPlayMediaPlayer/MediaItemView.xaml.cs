using net.allplay.MediaPlayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace AllPlayMediaPlayer
{
    public sealed partial class MediaItemView : UserControl
    {
        public MediaItemView()
        {
            this.InitializeComponent();
        }
        
        public MediaItem MediaItem
        {
            get { return (MediaItem)GetValue(MediaItemProperty); }
            set { SetValue(MediaItemProperty, value); }
        }

        public static readonly DependencyProperty MediaItemProperty =
            DependencyProperty.Register(nameof(MediaItem), typeof(MediaItem), typeof(MediaItemView), new PropertyMetadata(null, OnMediaItemPropertyChanged));

        private static void OnMediaItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = ((MediaItemView)d);
            //view.LayoutRoot.DataContext = e.NewValue;
            view.Visibility = view.MediaItem != null ? Visibility.Visible : Visibility.Collapsed; ;
            view.AlbumArt.Visibility = !string.IsNullOrEmpty(view.MediaItem?.ThumbnailUrl) && view.ShowAlbumArt ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool ShowAlbumArt
        {
            get { return (bool)GetValue(ShowAlbumArtProperty); }
            set { SetValue(ShowAlbumArtProperty, value); }
        }

        public static readonly DependencyProperty ShowAlbumArtProperty =
            DependencyProperty.Register("ShowAlbumArt", typeof(bool), typeof(MediaItem), new PropertyMetadata(true, OnShowAlbumArtPropertyChanged));

        private static void OnShowAlbumArtPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaItemView)d).AlbumArt.Visibility = ((MediaItemView)d).ShowAlbumArt ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
