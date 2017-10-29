using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Input;
using GeoNote.Resources;

using Microsoft.Phone.Maps.Services;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Tasks;

namespace GeoNote
{
    public partial class DirectionsPage : PhoneApplicationPage
    {

        List<GeoCoordinate> MyCoordinates = new List<GeoCoordinate>();

        public DirectionsPage()
        {
            InitializeComponent();
            MyCoordinates.Add(App.CurrentLocation);

        }


        private void directionsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            MapsDirectionsTask mapsDirectionsTask = new MapsDirectionsTask();

            // If you set the geocoordinate parameter to null, the label parameter is used as a search term.
            LabeledMapLocation destination = new LabeledMapLocation(textBoxDir.Text, null);

            mapsDirectionsTask.End = destination;

            // If mapsDirectionsTask.Start is not set, the user's current location is used as the start point.
          //  NavigationService.Navigate("/MainPage.xaml",     

           mapsDirectionsTask.Show();

        }

        
        private void Box_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }


    }
}