using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoNote.Model.Utility
{
    public class DatabaseUtility
    {
        string databaseFileName;
        readonly LocalDatabaseMode localDatabaseMode;

        public DatabaseUtility(string databaseFileName,
            LocalDatabaseMode localDatabaseMode = LocalDatabaseMode.ReadWrite)
        {
            this.localDatabaseMode = localDatabaseMode;
            this.databaseFileName = ArgumentValidator.AssertNotNullOrWhiteSpace(
                                                databaseFileName, "databaseFileName");
        }

        public void InitializeDatabase(DataContext dataContext, bool wipe = false)
        {
            ArgumentValidator.AssertNotNull(dataContext, "dataContext");

            if (localDatabaseMode != LocalDatabaseMode.ReadWrite
                && localDatabaseMode != LocalDatabaseMode.Exclusive)
            {
                return;
                //throw new InvalidOperationException(
                //"Database is not writable. Its mode is " + localDatabaseMode);
            }

            if (wipe && dataContext.DatabaseExists())
            {
                dataContext.DeleteDatabase();
            }

            if (!dataContext.DatabaseExists())
            {
                dataContext.CreateDatabase();
            }
        }

        public void DeleteDatabase()
        {
            using (IsolatedStorageFile isolatedStorageFile
                        = IsolatedStorageFile.GetUserStoreForApplication())
            {
                DeleteDatabase(isolatedStorageFile);
            }
        }

        void DeleteDatabase(IsolatedStorageFile isolatedStorageFile)
        {
            string name = GetFileNameWithoutPrefix(databaseFileName);

            if (isolatedStorageFile.FileExists(name))
            {
                isolatedStorageFile.DeleteFile(name);
            }
        }

        static string GetFileNameWithoutPrefix(string name)
        {
            if (name.StartsWith("isostore:", StringComparison.OrdinalIgnoreCase))
            {
                return name.Substring(9);
            }
            return name;
        }

        public bool DatabaseExists()
        {
            string name = GetFileNameWithoutPrefix(databaseFileName);

            using (IsolatedStorageFile isolatedStorageFile
                        = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isolatedStorageFile.FileExists(name);
            }
        }

        public string DataSource
        {
            get
            {
                return databaseFileName;
            }
        }

        public string DatabaseFileName
        {
            get
            {
                return databaseFileName;
            }
            set
            {
                databaseFileName = value;
            }
        }

        public string DatabasePassword { get; set; }

        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DatabasePassword))
                {
                    return string.Format("Data Source='{0}';Mode={1}",
                        DataSource, localDatabaseMode.ToConnectionStringValue());
                }
                return string.Format("Data Source='{0}';Password='{1}';Mode={2}",
                                        DataSource,
                                        DatabasePassword,
                                        localDatabaseMode.ToConnectionStringValue());
            }
        }
    }

    public enum LocalDatabaseMode
    {
        ReadWrite,
        ReadOnly,
        Exclusive,
        SharedRead
    }

    public static class LocalDatabaseModeExtensions
    {
        public static string ToConnectionStringValue(this LocalDatabaseMode mode)
        {
            switch (mode)
            {
                case LocalDatabaseMode.ReadWrite:
                    return "Read Write";
                case LocalDatabaseMode.ReadOnly:
                    return "Read Only";
                case LocalDatabaseMode.Exclusive:
                    return "Exclusive";
                case LocalDatabaseMode.SharedRead:
                    return "Shared Read";
                default:
                    throw new ArgumentException("Unknown mode: " + mode);
            }
        }
    }
}
