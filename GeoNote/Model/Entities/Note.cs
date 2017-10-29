using GeoNote.Infrastructure;
using Infrastructure.IsolatedStorage;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GeoNote.Model.Entities
{
    [Table]
    public class Note : NotifyPropertyChangeBase
    {
        int id;

        [Column(IsPrimaryKey = true, DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                Assign(ref id, value);
            }
        }

        string text;
        [Column(DbType = "NVarChar(1000)")]
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                Assign(ref text, value);
            }
        }

        DateTime? timeCreated;
        [Column(DbType = "DATETIME")]
        public DateTime? TimeCreated
        {
            get
            {
                return timeCreated;
            }
            set
            {
                Assign(ref timeCreated, value);
            }
        }

        string pictureFilePath;
        [Column(DbType = "NVarChar(1000)")]
        public string PictureFilePath
        {
            get
            {
                return pictureFilePath;
            }
            set
            {
                Assign(ref pictureFilePath, value);
            }
        }

        string audioFilePath;
        [Column(DbType = "NVarChar(1000)")]
        public string AudioFilePath
        {
            get
            {
                return audioFilePath;
            }
            set
            {
                Assign(ref audioFilePath, value);
            }
        }
        
        double latitude;
        [Column(DbType = "real")]
        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                Assign(ref latitude, value);
            }
        }

        double longitude;
        [Column(DbType = "real")]
        public double Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                Assign(ref longitude, value);
            }
        }

        public GeoCoordinate Coordinate
        {
            get
            {
                return new GeoCoordinate(latitude, longitude);
            }
        }

        public BitmapImage NoteImage
        {
            get
            {
                if (pictureFilePath == null)
                    return null;

                BitmapImage img = new BitmapImage();                
                img.DecodePixelHeight = 50;

                MemoryStream stream = IsolatedStorageHelpers.ReadFileMemoryStream(pictureFilePath);
                if (stream == null)
                    return null;

                img.SetSource(IsolatedStorageHelpers.ReadFileMemoryStream(pictureFilePath));

                return img;
            }
        }

        public BitmapImage NoteImageBig
        {
            get
            {
                if (pictureFilePath == null)
                    return null;

                BitmapImage img = new BitmapImage();
                img.DecodePixelHeight = 600;

                MemoryStream stream = IsolatedStorageHelpers.ReadFileMemoryStream(pictureFilePath);
                if (stream == null)
                    return null;

                img.SetSource(IsolatedStorageHelpers.ReadFileMemoryStream(pictureFilePath));

                return img;
            }
        }

        public Visibility HasRecording
        {
            get
            {
                if (String.IsNullOrEmpty(audioFilePath))
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public bool HasImage
        {
            get
            {
                return (!String.IsNullOrEmpty(pictureFilePath));
            }
        }


        private double? distanceToCurrent;
        public double DistanceToCurrent
        {
            get
            {
                if (distanceToCurrent == null && currentCoordinate != null)
                {
                    distanceToCurrent = currentCoordinate.GetDistanceTo(new GeoCoordinate(latitude, longitude));
                }

                if (distanceToCurrent.HasValue)
                    return Math.Round(distanceToCurrent.Value / 1000, 2);

                return 0;
            }
        }

        private GeoCoordinate currentCoordinate;
        public GeoCoordinate CurrentLocation
        {
            get
            {
                return currentCoordinate;
            }

            set
            {
                currentCoordinate = value;
                distanceToCurrent = null;
            }
        }

        public Visibility HasDistanceSet
        {
            get
            {
                if (distanceToCurrent.HasValue)
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
        }
    }
}
