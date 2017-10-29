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

using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Resources;
using System.Windows.Media;
using System.Threading;
using GeoNote.Model;
using GeoNote.Model.Entities;
using System.Device.Location;
using Infrastructure;
using Infrastructure.IsolatedStorage;
using Infrastructure.Audio;

namespace GeoNote
{
    public partial class AddNotePage : PhoneApplicationPage
    {
        private CameraCaptureTask cameraCaptureTask;        

        private bool noteSaved;        
        
        private string photoFileName;

        private GeoCoordinate noteLocation;

        private bool isAddingCustomLocation;
        private bool hasRecording;
                       
        private ImageBrush startRecordingImageBrush;
        private ImageBrush stopRecordingImageBrush;

        public AddNotePage()
        {
            InitializeComponent();

            Tuple<GeoCoordinate, bool> pageParams = (Tuple<GeoCoordinate, bool>)NavigationService.GetLastNavigationData();

            noteLocation = pageParams.Item1;
            isAddingCustomLocation = pageParams.Item2;
        }

        void DoneWithPhoto(object sender, PhotoResult args)
        {
            if (args.TaskResult == TaskResult.OK)
            {
               // noteBox.Margin = new Thickness(120, 29, 0, 0);
                Grid.SetRow(noteBox, 1);
                Grid.SetRow(saveNoteBtn, 2);

                BitmapImage img = new BitmapImage();
                img.SetSource(args.ChosenPhoto);
                imageBox.Source = img;

                photoFileName = Path.GetFileName(args.OriginalFileName);
                SaveImage(args.ChosenPhoto, photoFileName);
            }
        }

        public static void SaveImage(Stream imageStream, string fileName)
        {
            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorage.FileExists(fileName))
                    isolatedStorage.DeleteFile(fileName);

                var fileStream = isolatedStorage.CreateFile(fileName);
                var bitmap = new BitmapImage();
                bitmap.SetSource(imageStream);

                var wb = new WriteableBitmap(bitmap);
                wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                fileStream.Close();
            }
        }

        private void addImage_Click(object sender, System.EventArgs e)
        {
            if (cameraCaptureTask == null)
            {
                cameraCaptureTask = new CameraCaptureTask();
                cameraCaptureTask.Completed += DoneWithPhoto;
            }

            try
            {
                cameraCaptureTask.Show();
            }

            catch (System.InvalidOperationException)
            {

            }
        }

        private void addAudio_Click(object sender, System.EventArgs e)
        {            

            startRecordingImageBrush = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"Assets\microphone.png", UriKind.Relative))
            };

            stopRecordingImageBrush = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"Assets\stop.png", UriKind.Relative))
            };

            Dispatcher.BeginInvoke(() =>
            {
                toggleRecordingButton.Visibility = Visibility.Visible;
            });
        }

        #region Recording Audio  
        void ToggleRecording()
        {
            if (AudioHelpers.IsRecording)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (toggleRecordingButton.Background != startRecordingImageBrush)
                        toggleRecordingButton.Background = startRecordingImageBrush;

                    playButton.Visibility = Visibility.Visible;
                    imgRecording.Visibility = Visibility.Collapsed;
                });

                AudioHelpers.StopRecording();
                hasRecording = true;

            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (toggleRecordingButton.Background != stopRecordingImageBrush)
                        toggleRecordingButton.Background = stopRecordingImageBrush;

                    playButton.Visibility = Visibility.Collapsed;
                    imgRecording.Visibility = Visibility.Visible;
                });

                AudioHelpers.StartRecording();
            }
        }        
        #endregion

        #region TextBox service routines

        private void NewTextString(object sender, RoutedEventArgs e)
        {
            noteBox.SelectAll();
        }

        private void Box_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {


            }
        }

        #endregion

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            AudioHelpers.PlayCurrentRecording();
        }

        private void toggleRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleRecording();
        }

        private void saveNote_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (noteSaved)
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                return;
            }

            try
            {                
                GeoNoteDataService.SaveNote(noteLocation.Latitude, noteLocation.Longitude, noteBox.Text, photoFileName, hasRecording ? AudioHelpers.RecordingFileName : null);                
            }
            catch (Exception ex)
            {
                statusTextBlock.Text = "Save failed, try again! Error: " + ex.Message;
                return;
            }

            statusTextBlock.Text = "Note Saved!";            
            saveNoteBtn.Content = "Back to Map";
            noteBox.IsEnabled = false;
            toggleRecordingButton.Visibility = Visibility.Collapsed;
            playButton.Visibility = Visibility.Collapsed;

            ApplicationBarMenuItem addImageItem = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            ApplicationBarMenuItem addAudioItem = ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
            addImageItem.IsEnabled = false;
            addAudioItem.IsEnabled = false;

            noteSaved = true;
        }
    }
}