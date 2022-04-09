using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoRenamer.Core
{
    public class RenameProcessor
    {
        public delegate void RenaemHandler(string fileName);
        public event RenaemHandler Notify;

        public async Task RenameAllFiles(string path)
        {

        }

        private string GiveNewPath(FileSystemInfo fi, DateTime date)
        {
            var newName = $"{date.Year}-{date.Month:D2}-{date.Day:D2} {date.Hour:D2}-{date.Minute:D2}-{date.Second:D2}{fi.Extension.ToLower()}";

            Notify(newName);

            var newPath = fi.FullName.Replace(fi.Name, newName);
            ChangeFileDate(fi.FullName, date);
            if (!File.Exists(newPath))
            {
                return newPath;
            }
            var tempPath = newPath;
            var i = 0;
            while (File.Exists(tempPath))
            {
                i++;
                tempPath = $"{newPath.Replace(fi.Extension.ToLower(), string.Empty)}_{i:D4}{fi.Extension.ToLower()}";
            }
            newPath = tempPath;
            return newPath;
        }

        private static void ChangeFileDate(string filePath, DateTime date)
        {
            if (!File.Exists(filePath))
            {
                return;
            }
            File.SetCreationTime(filePath, date);
            File.SetLastWriteTime(filePath, date);
            File.SetLastAccessTime(filePath, date);
        }
    }
}
