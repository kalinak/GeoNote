using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IsolatedStorage
{
    public class IsolatedStorageHelpers
    {
        private static readonly object fileLock = new object();

        public static void WriteFile(MemoryStream memoryStream, string fileName)
        {
            lock (fileLock)
            {
                using (var isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                {                    
                    using (IsolatedStorageFileStream fileStream
                        = isolatedStorageFile.OpenFile(fileName, FileMode.Create))
                    {
                        memoryStream.WriteTo(fileStream);
                    }
                }
            }
        }

        public static byte[] ReadFile(string fileName)
        {
            byte[] bytes = null;

            lock (fileLock)
            {
                using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!String.IsNullOrEmpty(fileName) && userStore.FileExists(fileName))
                    {
                        using (IsolatedStorageFileStream fileStream
                                    = userStore.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            bytes = new byte[fileStream.Length];
                            fileStream.Read(bytes, 0, bytes.Length);
                        }
                    }
                }
            }

            return bytes;
        }

        public static MemoryStream ReadFileMemoryStream(string fileName)
        {
            byte[] bytes = ReadFile(fileName);
            if (bytes == null)
                return null;

            return new MemoryStream(bytes);
        }
    }
}
