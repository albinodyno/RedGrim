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
using Windows.UI.Core;
using Windows.Media.Capture;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Graphics.Display;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236
//Camera stream tutorial: https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/simple-camera-preview-access

namespace RedGrim.App.Controls
{
    public sealed partial class CameraControl : UserControl
    {
        DisplayRequest displayRequest = new DisplayRequest();
        MediaCapture mediaCapture;
        bool isPreviewing;

        public CameraControl()
        {
            this.InitializeComponent();
        }

        private async Task StartVidStream()
        {
            try
            {
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                //ShowMessageToUser("The app was denied access to the camera");
                return;
            }

            try
            {
                cetCamera1.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                isPreviewing = true;
            }
            catch (System.IO.FileLoadException)
            {
                //mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
            }
        }

        //private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        //{
        //    if (args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
        //    {
        //        ShowMessageToUser("The camera preview can't be displayed because another app has exclusive access");
        //    }
        //    else if (args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !isPreviewing)
        //    {
        //        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        //        {
        //            await StartPreviewAsync();
        //        });
        //    }
        //}

        private async Task CleanupCameraAsync()
        {
            if (mediaCapture != null)
            {
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    cetCamera1.Source = null;
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                    }

                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }

        }
    }
}
