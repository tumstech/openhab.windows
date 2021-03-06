﻿using System;
using MJPEGDecoderWinRTLib;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace OpenHAB.Windows.Controls
{
    /// <summary>
    /// Widget control that represents an OpenHAB Image
    /// </summary>
    public sealed partial class ImageWidget : WidgetBase
    {
        private BitmapImage _cameraBitmapImage;
        private MJPEGDecoder _mjpegDecoder;

        /// <summary>
        /// Gets or sets the camera bitmapimage
        /// </summary>
        public BitmapImage CameraBitmapImage
        {
            get
            {
                return _cameraBitmapImage;
            }

            set
            {
                if (_cameraBitmapImage == value)
                {
                    return;
                }

                _cameraBitmapImage = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageWidget"/> class.
        /// </summary>
        public ImageWidget()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _mjpegDecoder = new MJPEGDecoder();
            CameraBitmapImage = new BitmapImage();

            // Register listener methods
            _mjpegDecoder.FrameReady += MjpegDecoder_FrameReady;
            _mjpegDecoder.Error += MjpegDecoder_Error;

            // Construct Http Uri
            string requestUri = "http://jarvis:8888";

            // Tell MJPEGDecoder to connect to the IP camera, parse the mjpeg stream, and
            // report the received image frames.
            await _mjpegDecoder.ParseStreamAsync(requestUri, string.Empty, string.Empty);
        }

        private async void MjpegDecoder_FrameReady(object sender, FrameReadyEventArgs e)
        {
            // Copy the received FrameBuffer to an InMemoryRandomAccessStream.
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(e.FrameBuffer);
                    await writer.StoreAsync();
                }

                // Update source of CameraBitmap with the memory stream
                CameraBitmapImage.SetSource(ms);
            }
        }

        private void MjpegDecoder_Error(object sender, ErrorEventArgs e)
        {
            // ErrorMsg = e.Message;
        }

        private async void ImageWidget_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            CameraViewDialog.MinWidth = CameraBitmapImage.PixelWidth;
            await CameraViewDialog.ShowAsync();
        }
    }
}
