using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
    using Windows.Devices.Sensors;
    // The Blank Page item template is documented at https://go.microsoft.com/fwlink/p/?linkid=234238

    namespace App1
    {
        /// <summary>
        /// An empty page that can be used on its own or navigated to within a Frame.
        /// </summary>
        public sealed partial class MainPage : Page
        {
            // Sensor and dispatcher variables
            private SimpleOrientationSensor _simpleorientation;

            // This event handler writes the current sensor reading to
            // a text block on the app' s main page.

            private async void OrientationChanged(object sender, SimpleOrientationSensorOrientationChangedEventArgs e)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SimpleOrientation orientation = e.Orientation;
                    switch (orientation)
                    {
                        case SimpleOrientation.NotRotated:
                            txtOrientation.Text = "Not Rotated";
                            break;
                        case SimpleOrientation.Rotated90DegreesCounterclockwise:
                            txtOrientation.Text = "Rotated 90 Degrees Counterclockwise";
                            break;
                        case SimpleOrientation.Rotated180DegreesCounterclockwise:
                            txtOrientation.Text = "Rotated 180 Degrees Counterclockwise";
                            break;
                        case SimpleOrientation.Rotated270DegreesCounterclockwise:
                            txtOrientation.Text = "Rotated 270 Degrees Counterclockwise";
                            break;
                        case SimpleOrientation.Faceup:
                            txtOrientation.Text = "Faceup";
                            break;
                        case SimpleOrientation.Facedown:
                            txtOrientation.Text = "Facedown";
                            break;
                        default:
                            txtOrientation.Text = "Unknown orientation";
                            break;
                    }
                });
            }

            public MainPage()
            {
                this.InitializeComponent();
                _simpleorientation = SimpleOrientationSensor.GetDefault();

                // Assign an event handler for the sensor orientation-changed event
                if (_simpleorientation != null)
                {
                    _simpleorientation.OrientationChanged += new TypedEventHandler<SimpleOrientationSensor, SimpleOrientationSensorOrientationChangedEventArgs>(OrientationChanged);
                }
            }
        }
    }
