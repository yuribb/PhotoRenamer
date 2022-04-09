using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoRenamerNet
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private readonly DateTime MinDate = new DateTime(2006, 1, 1, 1, 1, 1, DateTimeKind.Local);

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                if (Directory.Exists(TextBoxPath.Text))
                {
                    RenameAllAsync(TextBoxPath.Text).RunSynchronously();
                }
                else
                {
                    MessageBox.Show($@"Directory {TextBoxPath.Text}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
            
        }

        private async Task RenameFilesAsync(string path)
        {
            try
            {
                var subdirectories = Directory.GetDirectories(path);

                foreach (var subdirectory in subdirectories)
                {
                    await RenameFilesAsync(subdirectory);
                }

                LabelPath.Invoke((MethodInvoker)delegate
                {
                    LabelPath.Text = path;
                });

                var filePaths = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                ProgressBar.Invoke((MethodInvoker)delegate
                {
                    ProgressBar.Maximum = filePaths.Length;
                });

                for (var i = 1; i <= filePaths.Length; i++)
                {
                    var value = i;
                    var filePath = filePaths[value - 1];
                    LabelCurrent.Invoke((MethodInvoker)delegate
                    {
                        LabelCurrent.Text = new FileInfo(filePath).Name;
                    });
                    
                    ProgressBar.Invoke((MethodInvoker)delegate
                    {
                        ProgressBar.Value = value;
                        ProgressBar.Text = $@"{value}/{filePaths.Length}";
                    });
                    RenameFile(filePath);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task RenameAllAsync(string rootPath)
        {
            try
            {
                ButtonStart.Invoke((MethodInvoker)delegate
                {
                    ButtonStart.Enabled = false;
                });
                LabelCurrent.Invoke((MethodInvoker)delegate
                {
                    LabelCurrent.Text = string.Empty;
                });
                LabelPath.Invoke((MethodInvoker)delegate
                {
                    LabelPath.Text = rootPath;
                });
                ProgressBar.Invoke((MethodInvoker)delegate
                {
                    ProgressBar.Visible = true;
                });

                await RenameFilesAsync(rootPath);
            }
            finally
            {
                ButtonStart.Invoke((MethodInvoker)delegate
                {
                    ButtonStart.Enabled = true;
                });
                LabelCurrent.Invoke((MethodInvoker)delegate
                {
                    LabelCurrent.Text = @"Ready";
                });
                LabelPath.Invoke((MethodInvoker)delegate
                {
                    LabelPath.Text = string.Empty;
                });
                ProgressBar.Invoke((MethodInvoker)delegate
                {
                    ProgressBar.Text = string.Empty;
                    ProgressBar.Visible = false;
                });
            }
        }

        private void ButtonPath_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.ShowNewFolderButton = false;
                if (Directory.Exists(TextBoxPath.Text))
                {
                    folderBrowserDialog.SelectedPath = TextBoxPath.Text;
                }

                var dialogResult = folderBrowserDialog.ShowDialog();
                if (dialogResult != DialogResult.OK)
                {
                    return;
                }
                TextBoxPath.Text = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.Path = TextBoxPath.Text;
                Properties.Settings.Default.Save();
            }
        }

        private void RenameFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                var fi = new FileInfo(filePath);
                var filo = fi.Name.ToLower();
                DateTime? dateTime;
                if (filo.StartsWith("201") || filo.StartsWith("200") || filo.StartsWith("202"))
                {
                    if (filo.Contains("_") && filo.Length <= 19)
                    {
                        var year = int.Parse(fi.Name.Substring(0, 4));
                        var month = int.Parse(fi.Name.Substring(4, 2));
                        var day = int.Parse(fi.Name.Substring(6, 2));
                        var hour = int.Parse(fi.Name.Substring(9, 2));
                        var minute = int.Parse(fi.Name.Substring(11, 2));
                        var second = int.Parse(fi.Name.Substring(13, 2));
                        dateTime = new DateTime(year, month, day, hour, minute, second);
                    }
                    else
                        return;
                }
                else if ((filo.StartsWith("img_20") || filo.StartsWith("vid_20")) && filo.Length > 17)
                {
                    var year = int.Parse(fi.Name.Substring(4, 4));
                    var month = int.Parse(fi.Name.Substring(8, 2));
                    var day = int.Parse(fi.Name.Substring(10, 2));
                    var hour = int.Parse(fi.Name.Substring(13, 2));
                    var minute = int.Parse(fi.Name.Substring(15, 2));
                    var second = int.Parse(fi.Name.Substring(17, 2));
                    dateTime = new DateTime(year, month, day, hour, minute, second);
                }
                else if (filo.StartsWith("wp_20") && filo.Length > 20)
                {
                    var year = int.Parse(fi.Name.Substring(3, 4));
                    var month = int.Parse(fi.Name.Substring(7, 2));
                    var day = int.Parse(fi.Name.Substring(9, 2));
                    var hour = int.Parse(fi.Name.Substring(12, 2));
                    var minute = int.Parse(fi.Name.Substring(15, 2));
                    var second = int.Parse(fi.Name.Substring(18, 2));
                    dateTime = new DateTime(year, month, day, hour, minute, second);
                }
                else
                {
                    dateTime = GetMetadataTime(fi);
                }

                if (!dateTime.HasValue)
                {
                    return;
                }
                var newPath = GiveNewPath(fi, dateTime.Value);
                File.Move(filePath, newPath);
                ChangeFileDate(newPath, dateTime.Value);
            }
            catch(Exception ex)
            {
                LabelCurrent.Invoke((MethodInvoker)delegate
                {
                    LabelCurrent.Text = ex.Message;
                });
            }
        }

        private DateTime? GetMetadataTime(FileSystemInfo fi)
        {
            var dateTime = GetExifDate(fi);
            if (dateTime.HasValue)
            {
                return dateTime;
            }

            var mf = new MediaInfoDotNet.MediaFile(fi.FullName);

            if (mf.Image.Any())
            {
                dateTime = mf.Image.First().EncodedDate;
            }

            else if (mf.Video.Any())
            {
                dateTime = mf.Video.First().EncodedDate;
            }
            if (dateTime.HasValue && dateTime.Value > MinDate)
            {
                return dateTime;
            }
            return null;
        }

        private DateTime? GetExifDate(FileSystemInfo fi)
        {
            IReadOnlyList<MetadataExtractor.Directory> metadata;
            try
            {
                metadata = MetadataExtractor.ImageMetadataReader.ReadMetadata(fi.FullName);
            }
            catch
            {
                return null;
            }

            IList<DateTime> dates = new List<DateTime>();

            foreach (var meta in metadata)
            {
                foreach (var tag in meta.Tags)
                {
                    if (tag.Description == null || !tag.Name.ToLower().Contains("date") || tag.Description.Length < 19 || tag.Description.Contains("+")) continue;
                    if (DateTime.TryParse(tag.Description, out var dateTime))
                    {
                        dates.Add(dateTime);
                    }
                    else if (tag.Description.Contains(":"))
                    {
                        try
                        {
                            var year = int.Parse(tag.Description.Substring(0, 4));
                            var month = int.Parse(tag.Description.Substring(5, 2));
                            var day = int.Parse(tag.Description.Substring(8, 2));
                            var hour = int.Parse(tag.Description.Substring(11, 2));
                            var minute = int.Parse(tag.Description.Substring(14, 2));
                            var second = int.Parse(tag.Description.Substring(17, 2));
                            dateTime = new DateTime(year, month, day, hour, minute, second);
                            dates.Add(dateTime);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }

            if (!dates.Any())
            {
                return null;
            }
            dates = dates.Where(d => d > MinDate).ToList();
            if (!dates.Any())
            {
                return null;
            }

            var dateTimeVal = dates.Min();
            if (dateTimeVal <= MinDate)
            {
                return null;
            }
            return dateTimeVal;
        }

        private string GiveNewPath(FileSystemInfo fi, DateTime date)
        {
            var newName = $"{date.Year}-{date.Month:D2}-{date.Day:D2} {date.Hour:D2}-{date.Minute:D2}-{date.Second:D2}{fi.Extension.ToLower()}";

            LabelCurrent.Invoke((MethodInvoker)delegate
            {
                LabelCurrent.Text = LabelCurrent.Text += $@" → {newName}";
            });

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

        private void MainForm_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();
            TextBoxPath.Text = Properties.Settings.Default.Path;
        }
    }
}