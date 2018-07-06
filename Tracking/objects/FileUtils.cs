using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Tracking.objects
{
    public class FileUtils
    {

        bool flag = false;
        public string[] oldFileCollection;
        public string[] newFileCollection;
        public string newDirectory;
        public string oldDirectory;


        public void setOldFileDir()
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
                }
            }
        }

        public string setNewFileDir()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = (@"C:\newImages");
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    newDirectory = dialog.SelectedPath;
                    return newDirectory;
                }
                return "None Selected";
            }
        }

        private void selectAction(string action)
        {

            switch (action)
            {
                case "Move and Rename":
                    moveAndRename();
                    break;

                case "Preview Image":

                    break;

                case "Preview Movie":

                    break;

                case "Copy":

                    break;

                case "Delete":

                    break;

            }
        }

        public void previewImage(DataGrid datagrid)
        {

            var v = GetDataGridRows(datagrid);
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

        public string[] moveAndRename()
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

            return newFileCollection;
        }

        public void writeFiles()
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
