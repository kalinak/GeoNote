using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GeoNote.Resources;

using Microsoft.Phone.Maps.Services;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Controls;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;
using Infrastructure;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Maps.Toolkit;
using GeoNote.Model;
using GeoNote.Model.Entities;
using System.Collections.ObjectModel;


namespace GeoNote
{
    public partial class MainPage : PhoneApplicationPage
    {
        readonly static GeoCoordinate initialCenter = new GeoCoordinate(47.6096, -122.3341);
        private ProgressIndicator progressIndicator = new ProgressIndicator();

        private ObservableCollection<Note> allNotes;
        private MapItemsControl mapItemsControl;

        private bool isAddingCustomLocation;
        private int maxDisplayedNoteId;
            
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            
            // initialize the current location layer
            MapLayer mapLayer = myMap.Layers[1];
            MapOverlay mapOverlay = mapLayer.First();
            currentLocationOverlay = mapOverlay;

            // initialize the system tray
            SystemTray.SetIsVisible(this, false);
            SystemTray.SetOpacity(this, 0);

            progressIndicator.IsVisible = true;
            progressIndicator.IsIndeterminate = true;

            SystemTray.SetProgressIndicator(this, progressIndicator);

            CenterMapOnLocation();
        }

        private void mAddBtn_Click(object sender, System.EventArgs e)
        {
            Tuple<GeoCoordinate, bool> pageParams = new Tuple<GeoCoordinate, bool>(App.CurrentLocation, isAddingCustomLocation);

            NavigationService.Navigate("/AddNotePage.xaml", pageParams);            
        }

        private void mSetDestination_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/DirectionsPage.xaml", UriKind.Relative));
        }

        private void mSearch_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/NotesListPage.xaml", UriKind.Relative));
        }

        private void mViewAllNotes_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/NotesListPage.xaml", UriKind.Relative));
        }

        private void mLocation_Click(object sender, System.EventArgs e)
        {
            CenterMapOnLocation();
        }

        async void CenterMapOnLocation()
        {
            App.CurrentLocation = await GetLocation();
            myMap.Center = App.CurrentLocation;

            if (myMap.Center != null)
            {
                isAddingCustomLocation = false;
                currentLocationOverlay.GeoCoordinate = myMap.Center;
                myMap.ZoomLevel = 13;
            }            
        }

        async Task<GeoCoordinate> GetLocation()
        {
            DisplayIndicator("Locating...");
            
            var geolocator = new Geolocator();
            Geoposition geoposition;

            try
            {
                geoposition = await geolocator.GetGeopositionAsync();
            }
            catch (UnauthorizedAccessException)
            {
                HideIndicator();
                return initialCenter;
            }

            Geocoordinate geocoordinate = geoposition.Coordinate;
            HideIndicator();
            return geocoordinate.ToGeoCoordinate();            
        }

        private void DisplayIndicator(string message)
        {
            SystemTray.SetIsVisible(this, true);
            myMap.Margin = new Thickness(0, 35, 0, 0);
            progressIndicator.Text = message;
        }

        private void HideIndicator()
        {
            SystemTray.SetIsVisible(this, false);
            myMap.Margin = new Thickness(0);
        }

        private void myMap_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            isAddingCustomLocation = true;

            Point p = e.GetPosition(myMap);
            GeoCoordinate tappedGeoCoordinate = myMap.ConvertViewportPointToGeoCoordinate(p);

            App.CurrentLocation = tappedGeoCoordinate;
            currentLocationOverlay.GeoCoordinate = tappedGeoCoordinate;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Note: that's more efficient code but it needs some plumbing to detect changed entities

            if (allNotes == null)
            {
                allNotes = new ObservableCollection<Note>(GeoNoteDataService.GetAllNotes());
                maxDisplayedNoteId = GeoNoteDataService.GetMaxNoteId();
            }
            else
            {
                // more robust, but more inefficient code
                //allNotes.Clear();
                //(GeoNoteDataService.GetAllNotes()).ToList<Note>().ForEach((note) => allNotes.Add(note));

                // put the code below for higher performance, but it has issues when deleting and updatign
                int currentCountInDb = GeoNoteDataService.GetNotesCount();
                var newNotesFromDb = GeoNoteDataService.GetNotesWithIdBiggerThan(maxDisplayedNoteId);

                if (newNotesFromDb.Count() == 0 && currentCountInDb < allNotes.Count) // we have deleted
                {
                    allNotes.Clear();
                    (GeoNoteDataService.GetAllNotes()).ToList<Note>().ForEach((note) => allNotes.Add(note));
                }
                else
                {
                    maxDisplayedNoteId = GeoNoteDataService.GetMaxNoteId();
                    newNotesFromDb.ToList<Note>().ForEach((note) => allNotes.Add(note));
                }
            }            

            if (mapItemsControl == null)
            {
                ObservableCollection<DependencyObject> children = MapExtensions.GetChildren(myMap);
                mapItemsControl = children.FirstOrDefault(x => x.GetType() == typeof(MapItemsControl)) as MapItemsControl;
            }

            if (mapItemsControl.ItemsSource == null)
            {
                mapItemsControl.ItemsSource = allNotes;
            }                        
        }

        private void Pushpin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Pushpin clickedPin = sender as Pushpin;
            if (clickedPin == null)
                return;

            Note clickedNote = clickedPin.DataContext as Note;
            if (clickedNote == null)
                return;

            NavigationService.Navigate("/NoteDetailsPage.xaml", clickedNote);

            e.Handled = true;
        }
    }
}
