using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections;

namespace Tracking.windows
{
    /// <summary>
    /// Interaction logic for FileActions.xaml
    /// </summary>
    public partial class FileActions : Window
    {
        bool flag = false;
        public string[] oldFileCollection;
        public string[] newFileCollection;
        public string newDirectory;
        public string oldDirectory;
        public FileActions()
        {
            InitializeComponent();
            DataContext = new System.Windows.DataObject();
            progressBar.Visibility = Visibility.Hidden;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {

                    //FileInfo[] v = new DirectoryInfo(dialog.SelectedPath).GetFiles();
                    //string[] fileNames = new string[v.Length];
                    //for (int i = 0; i < v.Length; i++)
                    //{
                    //    fileNames[i] = v[i].Name;

                    //}
                    //oldFileCollection = fileNames;
                    //dataGrid.ItemsSource = fileNames;
                    oldDirectory = dialog.SelectedPath;
                    label1.Content = oldDirectory;
                }
            }
        }

        private void runAction(string s)
        {
            switch (s)
            {
                case "Move and Rename":
                    moveAndRename();
                    break;

                case "Preview Image":
                    previewImage();
                    break;

                case "Preview Movie":

                    break;

                case "Copy":

                    break;

                case "Delete":

                    break;

            }
        }
        private  void button2_Click(object sender, RoutedEventArgs e)
        {
            string c = comboBox.SelectedValue.ToString();



              


        }

        public void previewImage()
        {

            var v = GetDataGridRows(dataGrid);
            string s = "test";
        }
        public IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }

        public void moveAndRename()
        {

            FileInfo[] v = new DirectoryInfo(oldDirectory).GetFiles();

            string oldFileName = v[0].Name;
            string[] newFileNames = new string[v.Length];
            string[] oldFileNames = new string[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                try
                {
                    oldFileNames[i] = v[i].FullName;
                    string newFileName = Helper.GetUntilOrEmpty(v[i].Name);
                    string filePath = v[i].Directory.FullName;
                    double d = Convert.ToDouble(newFileName);
                    string fmt = "000000";
                    string NewFileName = newDirectory + "\\" + d.ToString(fmt) + v[i].Extension;
                    newFileNames[i] = NewFileName;
                }
                catch (Exception ex)
                {
                    ErrorLogging(ex);
                }
            }
            newFileCollection = newFileNames;
            oldFileCollection = oldFileNames;

            progressBar.Maximum = newFileCollection.Length - 1;
            progressBar.Visibility = Visibility.Visible;
            Thread backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    for (int i = 0; i < newFileCollection.Length - 1; i++)
                    {
                        try
                        {
                            System.IO.File.Move(oldFileCollection[i], newFileCollection[i]);
                        }
                        catch
                        {
                            File.Delete(newFileCollection[i]);
                            System.IO.File.Move(oldFileCollection[i], newFileCollection[i]);
                        }
                        progressBar.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal
                        , new DispatcherOperationCallback(delegate
                        {
                            progressBar.Value = progressBar.Value + 1;
                            label6.Content = "Processing " + i.ToString() + " of " + (newFileCollection.Length - 1).ToString();
                            return null;
                        }), null);
                    }
                }
            ));
            backgroundThread.Start();
            dataGrid.ItemsSource = newFileCollection;
        }

        public void writeFiles()
        {
            for (int i = 0; i < newFileCollection.Length -1; i++ )
            {
                try
                    {
                    System.IO.File.Move(oldFileCollection[i], newFileCollection[i]);
                }
                    catch
                    {
                        File.Delete(newFileCollection[i]);
                        System.IO.File.Move(oldFileCollection[i], newFileCollection[i]);
                    }
            }
            flag = true;
     
        }

        public static void ErrorLogging(Exception ex)
        {
            string strPath = @"C:\log\log.txt";
            if (!File.Exists(strPath))
            {
                File.Create(strPath).Dispose();
            }
            using (StreamWriter sw = File.AppendText(strPath))
            {
                sw.WriteLine("=============Error Logging ===========");
                sw.WriteLine("===========Start============= " + DateTime.Now);
                sw.WriteLine("Error Message: " + ex.Message);
                sw.WriteLine("Stack Trace: " + ex.StackTrace);
                sw.WriteLine("===========End============= " + DateTime.Now);

            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = (@"C:\newImages");
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    newDirectory = dialog.SelectedPath;
                    label5.Content = newDirectory;
                }
            }
        }
    }
    static class Helper
    {
        public static string GetUntilOrEmpty(this string text, string stopAt = "_")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }
    }
}
