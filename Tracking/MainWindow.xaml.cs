using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using Tracking.objects;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text;
using Tracking.windows;
using System.Configuration;
using System.Threading;
using System.Windows.Threading;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Initialize "global" variables
        public string file;
        public string[] files;
        public string[] timeChannelFiles;
        private IDN Neutrophil;
        private List<IDN> nList = new List<IDN>();
        private List<TimeChannelFile> tcList = new List<TimeChannelFile>();
        private int tcIndex = 0;
        private int lineSegmentIndex = 1;
        List<settingItem> items = new List<settingItem>();
        public double[] lineCutIndex;
        public double[] lineCutIndexHorizontal;
        FileUtils fileUtils = new FileUtils();

        public MainWindow()
        {
            InitializeComponent();
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo("SourcePath");
            colorPicker.SelectedColor = System.Windows.Media.Color.FromRgb(255, 255, 255);
            colorPicker2.SelectedColor = System.Windows.Media.Color.FromRgb(255, 255, 255);
            doubleUpDown_Matlab.Value = 18.0;
            setComboBox();
            setAppConfigs();
            doubleUpDown.Value = 1.0;
            setLabels();
            string[] lineCutMasks = new string[] { "Sepsis Chip" };
            comboBox3.ItemsSource = lineCutMasks;
            comboBox3.SelectedIndex = 0;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            progressBar.Visibility = Visibility.Hidden;


        }

        private void setLabels()
        {
            label22.Content = items.Where(x => x.Setting == "Source Location").Select(x => x.Value).SingleOrDefault();
            label39.Content = items.Where(x => x.Setting == "Source Location").Select(x => x.Value).SingleOrDefault();
            label37.Content = items.Where(y => y.Setting == "File Rename Rule").Select(y => y.Value).SingleOrDefault();
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            setTimeChannelFile();
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = (@"C:\work\temp\images\simple2");
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    rec2.Visibility = Visibility.Hidden;
                    label8.Visibility = Visibility.Hidden;
                    //label10.Visibility = Visibility.Hidden; // hide "movie files" label
                    files = Directory.GetFiles(dialog.SelectedPath);
                    setFiles(dialog.SelectedPath, dialog);
                    LoadNewImage(files.ElementAt(1));
                }
            }
        }
        // Gives drop down for line thickness for paint utilities
        public void setComboBox()
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            list.Add("1");
            list.Add("2");
            list.Add("3");
            this.comboBox.ItemsSource = list;
            this.comboBox.SelectedIndex = 0;

            ObservableCollection<string> Rulelist = new ObservableCollection<string>();
            Rulelist.Clear();
            Rulelist.Add("_");
            Rulelist.Add(")");
            this.comboBox2.ItemsSource = Rulelist;
            this.comboBox2.SelectedIndex = 0;
        }
        // used to sort and rename string [] with file names //
        public void setFiles(string folderPath, FolderBrowserDialog dialog)
        {
            var ext = new List<string> { ".jpg", ".gif", ".png", ".tif" };
            var filesVar = Directory.GetFiles(dialog.SelectedPath, "*.*", SearchOption.AllDirectories)
                 .Where(s => ext.Contains(System.IO.Path.GetExtension(s)));

            string[] files = filesVar.ToArray();
            string[] temp = new string[files.Length];

            int parseBefore = folderPath.Length + 2;

            for (int i = 0; i < files.Length; i++)
            {
                int parseAfter = files[i].Length - parseBefore - 4;
                temp[i] = files[i].Substring(parseBefore, (parseAfter));
            }
            Array.Sort(temp);
            files = temp;

            if (files.Length > 0)
            {
                slider.Minimum = 0;
                slider.Maximum = files.Length - 1;
            }
        }
        private void LoadNewImage(string path)
        {
            label5.Content = path;
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path);
            image.EndInit();
            image1.Source = image;
        }

        private void image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(this.Cnv);
            p.X = Math.Round(p.X);
            p.Y = Math.Round(p.Y);
            IDN neutrophil = new IDN();
            neutrophil.scaleX = (image.Source.Width / image.Width);
            neutrophil.scaleY = (image.Source.Height / image.Height);
            neutrophil.timeChannel = tcList.ElementAt(tcIndex).fileName;
            neutrophil.name = "LineSegment" + lineSegmentIndex.ToString();
            neutrophil.timeChannel = image.Source.ToString().Substring(image.Source.ToString().LastIndexOf('/') + 1);
            neutrophil.timeChannel = neutrophil.timeChannel.Substring(0, neutrophil.timeChannel.Length - 4);
            Neutrophil = neutrophil;
            nList.Add(Neutrophil);
            Neutrophil.addPointToList(p);
            lineSegmentIndex++;
            var v = Neutrophil;
            listBox1.Items.Add(v);

        }

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(this.Cnv);
            p.X = Math.Round(p.X);
            p.Y = Math.Round(p.Y);
            Neutrophil.addPointToList(p);
            paintLines(Neutrophil);
            updateTotals();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsNullOrEmpty(files) == true)
            {
                return;
            }
            string fileToShow = files[(int)slider.Value];
            LoadNewImage(fileToShow);
        }
        private static bool IsNullOrEmpty(Array array)
        {
            return (array == null || array.Length == 0);
        }
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            this.Cnv.Children.Clear();


        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            setTimeChannelImage(1);
        }

        private void setTimeChannelImage(int pos)
        {
            if (tcIndex == 0 && pos == -1)
                return;
            tcIndex += pos;
            ImageSource imageSource = new BitmapImage(new Uri(tcList.ElementAt(tcIndex).fullFileName));
            image.Source = imageSource;
            Cnv.Children.Clear();
            comboBox1.SelectedIndex = comboBox1.SelectedIndex + pos;
        }
        private void setTimeChannelFile()
        {
            tcList.Clear();
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = (@"C:\bin\TimeChannels");
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    rec1.Visibility = Visibility.Hidden;
                    label6.Visibility = Visibility.Hidden;
                    label10.Visibility = Visibility.Hidden; // hide "movie files" label

                    foreach (var v in Directory.GetFiles(dialog.SelectedPath))
                    {
                        TimeChannelFile tc = new TimeChannelFile();
                        tc.fullFileName = v.ToString();
                        tc.setFileName();
                        tcList.Add(tc);
                        comboBox1.Items.Add(tc.fileName);
                    }
                    ImageSource imageSource = new BitmapImage(new Uri(tcList.ElementAt(0).fullFileName));
                    slider.Width = image.Width;
                    image.Source = imageSource;

                    comboBox1.SelectedIndex = 0;
                }
            }
        }

        private void setTreeViewItems()
        {

            treeView.Items.Clear();
            TreeViewItem treeItem = null;

            treeItem = new TreeViewItem();
            treeItem.Header = "Details";

            treeItem.Items.Add(new TreeViewItem() { Header = Neutrophil.timeChannel });
            treeItem.Items.Add(new TreeViewItem() { Header = "ΔX" });
            treeItem.Items.Add(new TreeViewItem() { Header = "ΔY" });

            treeView.Items.Add(treeItem);
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setTreeViewItems();
        }

        private void paintLines(IDN n)
        {
            SolidColorBrush brush = new SolidColorBrush(colorPicker.SelectedColor.Value);

            int thickness;
            Int32.TryParse(comboBox.SelectionBoxItem.ToString(), out thickness);

            for (int i = 1; i < n.pList.Count; i++)
            {
                Line line = new Line();
                line.Stroke = brush;
                line.StrokeThickness = thickness;
                if (n.pList.Count == 1)
                    break;

                line.X1 = n.pList.ElementAt(i - 1).X;
                line.X2 = n.pList.ElementAt(i).X;
                line.Y1 = n.pList.ElementAt(i - 1).Y;
                line.Y2 = n.pList.ElementAt(i).Y;
                Cnv.Children.Add(line);
            }
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            var v = listBox1.SelectedItem;
            paintLines((IDN)v);
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < nList.Count; i++)
            {
                if (nList.ElementAt(i) == listBox1.SelectedItem)
                {
                    listBox1.Items.Remove(nList.ElementAt(i));
                    nList.RemoveAt(i);
                    treeView.Items.Clear();
                }
            }
            updateTotals();
        }
        private void writeXML()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = (@"C:\work\temp\results");
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    string filename = dialog.SelectedPath + "\\output.xml";

                    double scaleX = (image.Source.Width / image.Width);
                    double scaleY = (image.Source.Height / image.Height);

                    XmlTextWriter writer = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
                    writer.WriteStartDocument(true);
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 2;
                    writer.WriteStartElement("Table");
                    writer.WriteStartElement("Scale");
                    writer.WriteStartElement("ScaleX");
                    writer.WriteString(scaleX.ToString());
                    writer.WriteEndElement();
                    writer.WriteStartElement("ScaleY");
                    writer.WriteString(scaleY.ToString());
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteStartElement("LineSegments");
                    foreach (var v in nList)
                    {
                        createNode(writer, v, scaleX, scaleY);
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Close();
                }
            }
        }
        private void createNode(XmlTextWriter writer, IDN idn, double scaleX, double scaleY)
        {

            writer.WriteStartElement("LineSegment");
            foreach (var v in idn.pList)
            {
                writer.WriteStartElement("X");
                writer.WriteString(Math.Round((v.X * scaleX), 2).ToString());
                writer.WriteEndElement();

            }
            foreach (var v in idn.pList)
            {
                writer.WriteStartElement("Y");
                writer.WriteString(Math.Round((v.Y * scaleY), 2).ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            //writeXML();
            serealizeObj();
        }

        private void setTimeChannelComboBox(string fileName)
        {

        }

        private void button10_Click(object sender, RoutedEventArgs e)
        {
            serealizeObj();

        }

        private string serealizeObj()
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(List<IDN>));
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, nList);
                    xml = sww.ToString();
                }
            }
            return xml;
        }

        private void button11_Click(object sender, RoutedEventArgs e)
        {
            setTimeChannelImage(-1);
        }

        private void button12_Click(object sender, RoutedEventArgs e)
        {
            MLApp.MLApp matlab = new MLApp.MLApp();
            object result = null;

            double deltaX = (double)doubleUpDown_Matlab.Value;
            matlab.Execute(@"cd C:\work\temp\images\simple\simple2");
            matlab.Feval("verticleLineCutFeed", 0, out result, "*.jpg", deltaX);

            img_Cut_rectangle.Visibility = Visibility.Hidden;
            image2.Source = new BitmapImage(new Uri(@"C:\work\temp\images\simple\simple2\cuts.jpg", UriKind.RelativeOrAbsolute));


        }

        private void button13_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = "";
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = (@"C:\work\temp\results");
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {

                    //System.IO.Directory.CreateDirectory(@"C:\work\temp\images\simple\simple\simple3");

                    //StringBuilder builder = new StringBuilder();

                    //DirectoryInfo d = new DirectoryInfo(dialog.SelectedPath);
                    //FileInfo[] infos = d.GetFiles();

                    //foreach (FileInfo f in infos)
                    //{
                    //    builder.Append(@"C:\work\temp\images\simple\simple\simple3\").Append(GetUntilOrEmpty(f.Name)).Append(".jpg");
                    //    File.Move(f.FullName, builder.ToString());
                    //    builder.Clear();
                    //}

                    folderPath = dialog.SelectedPath;
                    MLApp.MLApp matlab = new MLApp.MLApp();
                    object matlabResult = null;

                    string[] fileEntries = Directory.GetFiles(@"..\..\MATLAB");

                    matlab.Execute(@"cd C:\Tracking\Images");

                    matlab.Feval("fileRename", 0, out matlabResult, folderPath, "simple", "_");

                }

            }

        }

        private void updateTotals()
        {
            double totalDistance = 0;
            foreach (var n in nList)
            {
                for (int i = 1; i < n.pList.Count; i++)
                {
                    totalDistance += Math.Abs(n.pList.ElementAt(i - 1).Y - n.pList.ElementAt(i).Y);
                }
            }
            label16.Content = totalDistance.ToString();
            label15.Content = nList.Count.ToString();
        }

        private void runMATLAB(string s, string[] args)
        {
            MLApp.MLApp matlab = new MLApp.MLApp();
            object matlabResult = null;

            matlab.Execute(@"cd C:\work\temp\images\simple");
            matlab.Feval(s, 0, out matlabResult, args[0], args[1], args[2]);

        }


        private void btnDoAsynchronousCalculation_Click(object sender, RoutedEventArgs e)
        {
            pbCalculationProgress.Value = 0;


            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(10000);
        }

        private void btnDoSynchronousCalculation_Click(object sender, RoutedEventArgs e)
        {
            int max = 10000;


            int result = 0;
            for (int i = 0; i < max; i++)
            {
                if (i % 42 == 0)
                {
                    result++;
                }
                System.Threading.Thread.Sleep(1);
                pbCalculationProgress.Value = Convert.ToInt32(((double)i / max) * 100);
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int max = (int)e.Argument;
            int result = 0;
            for (int i = 0; i < max; i++)
            {
                int progressPercentage = Convert.ToInt32(((double)i / max) * 100);
                if (i % 42 == 0)
                {
                    result++;
                    (sender as BackgroundWorker).ReportProgress(progressPercentage, i);
                }
                else
                    (sender as BackgroundWorker).ReportProgress(progressPercentage);
                System.Threading.Thread.Sleep(1);

            }
            e.Result = result;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbCalculationProgress.Value = e.ProgressPercentage;

        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        public string GetUntilOrEmpty(string text, string stopAt = "_")
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

        private void btn_RotateImg_Click(object sender, RoutedEventArgs e)
        {
            MLApp.MLApp matlab = new MLApp.MLApp();
            object matlabResult = null;


            matlab.Execute(@"cd C:\work\temp\images\simple\simple2");
            matlab.Feval("rotateImg", 0, out matlabResult, doubleUpDown1.Value);
        }

        private void button10_Click_1(object sender, RoutedEventArgs e)
        {
            MLApp.MLApp matlab = new MLApp.MLApp();
            object matlabResult = null;


            matlab.Execute(@"cd C:\work\temp\images\simple\simple2");
            matlab.Feval("LineCutGUI", 0, out matlabResult);
        }

        private void button10_Click_2(object sender, RoutedEventArgs e)
        {
            Loading loadingWin = new Loading();
            loadingWin.Show();

        }
        private void button14_Click(object sender, RoutedEventArgs e)
        {
            setAppConfigs();
        }
        private void setAppConfigs()
        {

            settingItem pixelToMicron = new settingItem("Pixel to Micron", System.Configuration.ConfigurationManager.AppSettings["pixelToMicron"]);
            items.Add(pixelToMicron);
            settingItem saveLocation = new settingItem("Save Location", System.Configuration.ConfigurationManager.AppSettings["saveLocation"]);
            items.Add(saveLocation);
            settingItem sourceLocation = new settingItem("Source Location", System.Configuration.ConfigurationManager.AppSettings["sourceLocation"]);
            items.Add(sourceLocation);
            settingItem fileRenameRule = new settingItem("File Rename Rule", System.Configuration.ConfigurationManager.AppSettings["fileRenameRule"]);
            items.Add(fileRenameRule);
            dataGrid.ItemsSource = items;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "Setting")
            {
                // e.Cancel = true;   // For not to include 
                e.Column.IsReadOnly = true; // Makes the column as read only
            }
        }
        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listBox1.ItemsSource = null;
            listBox1.Items.Clear();
            foreach (var v in nList)
            {
                if (v.timeChannel == System.IO.Path.GetFileNameWithoutExtension(image.Source.ToString()) &&
                    v.timeChannel == comboBox1.SelectedValue.ToString())
                    listBox1.Items.Add(v);
            }
        }

        private void button15_Click(object sender, RoutedEventArgs e)
        {

            RotateTransform rotateTransform = new RotateTransform(45);


            Rectangle_LineCut.Visibility = Visibility.Hidden;
            label30.Visibility = Visibility.Hidden;
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(@"C:\work\temp\images\simple\000006.jpg");
            image.EndInit();
            image3.Source = image;

            double[] LineCuts = new double[] { 284, 284, 160, 500 };
            double[] LineCutHorizontal = new double[] { 284, 1008, 160, 160 };
            lineCutIndex = LineCuts;
            lineCutIndexHorizontal = LineCutHorizontal;

            paintLineCuts();
            
        }

        private void paintLineCuts()
        {

            List<Line> lineCuts = new List<Line>();
            double X1 = lineCutIndex[0];
            double X2 = lineCutIndex[1];
            double Y1 = lineCutIndex[2];
            double Y2 = lineCutIndex[3];

            CnvLineCut.Children.Clear();

            for (int i = 0; i < 38; i++)
            {
                SolidColorBrush brush = new SolidColorBrush(colorPicker2.SelectedColor.Value);

                int thickness = 1;

                Line line = new Line();
                line.Stroke = brush;
                line.StrokeThickness = thickness;

                line.X1 = X1;
                line.X2 = X2;
                line.Y1 = Y1;
                line.Y2 = Y2;

                if (i == 18)
                {
                    X1 += 39;
                    X2 += 39;
                }
                CnvLineCut.Children.Add(line);

                X1 += 18.5;
                X2 += 18.5 ;
            }

            double HX1 = lineCutIndexHorizontal[0];
            double HX2 = lineCutIndexHorizontal[1];
            double HY1 = lineCutIndexHorizontal[2];
            double HY2 = lineCutIndexHorizontal[3];

            for (int i= 0; i < 3; i++)
            {

                SolidColorBrush brush = new SolidColorBrush(colorPicker2.SelectedColor.Value);

                int thickness = 1;

                Line line = new Line();
                line.Stroke = brush;
                line.StrokeThickness = thickness;

                line.X1 = HX1;
                line.X2 = HX2;
                line.Y1 = HY1;
                line.Y2 = HY2;

                CnvLineCut.Children.Add(line);

                HY1 += 37;
                HY2 += 37;

            }


        }

        private void button16_Click(object sender, RoutedEventArgs e)
        {
            lineCutIndex[2] += slider1.Value;
            lineCutIndex[3] += slider1.Value;
            lineCutIndexHorizontal[2] += slider1.Value;
            lineCutIndexHorizontal[3] += slider1.Value;

            paintLineCuts();
        }

        private void button17_Click(object sender, RoutedEventArgs e)
        {
            lineCutIndex[0] += slider1.Value;
            lineCutIndex[1] += slider1.Value;
            lineCutIndexHorizontal[0] += slider1.Value;
            lineCutIndexHorizontal[1] += slider1.Value;

            paintLineCuts();
        }

        private void button18_Click(object sender, RoutedEventArgs e)
        {
            lineCutIndex[0] -= slider1.Value;
            lineCutIndex[1] -= slider1.Value;
            lineCutIndexHorizontal[0] -= slider1.Value;
            lineCutIndexHorizontal[1] -= slider1.Value;

            paintLineCuts();
        }

        private void button19_Click(object sender, RoutedEventArgs e)
        {
            lineCutIndex[2] -= slider1.Value;
            lineCutIndex[3] -= slider1.Value;
            lineCutIndexHorizontal[2] -= slider1.Value;
            lineCutIndexHorizontal[3] -= slider1.Value;

            paintLineCuts();
        }

        private void button20_Click(object sender, RoutedEventArgs e)
        {
            FileActions fileActionWindow = new FileActions();
            fileActionWindow.Show();
        }

        private void button21_Click(object sender, RoutedEventArgs e)
        {
            // logic to export line cut data to MATLAB
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (label19== null)
                return;

            label19.Content = slider1.Value.ToString();
        }

        private void button22_Click(object sender, RoutedEventArgs e)
        {
            LineCuts lineCutWindow = new LineCuts();
            lineCutWindow.Show();
        }

        private void button23_Click(object sender, RoutedEventArgs e)
        {
            MenuWindow menuWindow = new MenuWindow();
            menuWindow.Show();
        }

        private void button24_Click(object sender, RoutedEventArgs e)
        {

            fileUtils.setOldFileDir();
            label39.Content = fileUtils.oldDirectory;


        }

        private void button25_Click(object sender, RoutedEventArgs e)
        {
            label41.Content = fileUtils.setNewFileDir();
        }

        private void button24_Click_1(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = fileUtils.moveAndRename();
            label40.Content = fileUtils.newFileCollection.Length - 1;
            progressBar.Maximum = fileUtils.newFileCollection.Length - 1;
            progressBar.Visibility = Visibility.Visible;
            Thread backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    for (int i = 0; i < fileUtils.newFileCollection.Length - 1; i++)
                    {
                        try
                        {
                            System.IO.File.Move(fileUtils.oldFileCollection[i], fileUtils.newFileCollection[i]);
                        }
                        catch
                        {
                            File.Delete(fileUtils.newFileCollection[i]);
                            System.IO.File.Move(fileUtils.oldFileCollection[i], fileUtils.newFileCollection[i]);
                        }
                        progressBar.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal
                        , new DispatcherOperationCallback(delegate
                        {
                            progressBar.Value = progressBar.Value + 1;
                            label43.Content = "Processing " + i.ToString() + " of " + (fileUtils.newFileCollection.Length - 1).ToString();
                            return null;
                        }), null);
                    }
                }
            ));
            dataGrid1.ItemsSource = fileUtils.newFileCollection;
            backgroundThread.Start();
          }

        private void button27_Click(object sender, RoutedEventArgs e)
        {

            FileInfo[] v = new DirectoryInfo(@"C:\newImages").GetFiles();

            string oldFileName = v[0].Name;
            string[] newFileNames = new string[v.Length];
            string[] oldFileNames = new string[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                BitmapSource bSource = new BitmapImage(new Uri(v[i].FullName));
                BitmapImage bImage = new BitmapImage(new Uri(v[i].FullName));

                System.Drawing.Color[][] b = GetBitMapColorMatrix(v[i].FullName);


                int imageWidth = b.GetLength(0);
                int imageHeight = 720;
                byte[] column = new byte[imageHeight];
                byte[] columns = new byte[imageWidth];
                byte[,] kymograph = new byte[imageWidth, imageHeight];



                List<byte[]> newImageList = new List<byte[]>();

                for (int q = 0; q < b.GetLength(0); q++)
                {
                    byte[] columnValues = new byte[imageHeight];
                    for (int z = 0; z < 720; z++)
                    {
                        columnValues[z] = (b[q][z].B);
                    }


                    newImageList.Add(columnValues);
                    //columns[i, i] = columns[i, columnValues];            
                    columns.SetValue(newImageList.ElementAt(0), 0);
                    columns.SetValue(newImageList.ElementAt(1), 1);
                }

            }





            //var binFormatter = new BinaryFormatter();
            //var mStream = new MemoryStream();
            //binFormatter.Serialize(mStream, newImageList.ElementAt(0));

            //This gives you the byte array.
            //mStream.ToArray()         
            string s = "test";
        }

        public System.Drawing.Color[][] GetBitMapColorMatrix(string bitmapFilePath)
        {
            System.Drawing.Bitmap b1 = new Bitmap(bitmapFilePath);

            int hight = b1.Height;
            int width = b1.Width;

            System.Drawing.Color[][] colorMatrix = new System.Drawing.Color[width][];
            for (int i = 0; i < width; i++)
            {
                colorMatrix[i] = new System.Drawing.Color[hight];
                for (int j = 0; j < hight; j++)
                {
                    colorMatrix[i][j] = b1.GetPixel(i, j);
                }
            }
            return colorMatrix;
        }
        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
        public System.Drawing.Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            return returnImage;
        }

    }
}

