using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DiscUtils;
using DiscUtils.Iso9660;
using System.IO;


namespace QuickISO
{
    using FilePath = System.IO.Path;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ObservableCollection<string> filelist;

        public MainWindow()
        {
            InitializeComponent();

            filelist = new ObservableCollection<string>();
            ContentList.DataContext = filelist;
            bootFile.DataContext = filelist;
            
            

        }

        private void fileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.InitialDirectory = "c:\\dan";
            ofd.Multiselect = true;
            ofd.Title = "Find Files for your Disc Image";
            ofd.DereferenceLinks = true;

            if (ofd.ShowDialog() ?? false)
            {
                foreach (string file in ofd.FileNames)
                {
                    AddFile(file);
                }
            }


        }

        void AddFile(string file)
        {
            if (!filelist.Contains(file))
            {
                filelist.Add(file);
            }
        }

        private void isoButton_Click(object sender, RoutedEventArgs e)
        {
            
            
            
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".iso";
            sfd.InitialDirectory = "c:\\dan\\ISO";
            sfd.OverwritePrompt = true;
            sfd.Title = "Choose A home for your new Disc Image";
           

            if (sfd.ShowDialog() ?? false)
            { 
                //craete an iso dumb ass                    
              
                    CDBuilder image = new CDBuilder();
                    image.UseJoliet = true;
                    image.VolumeIdentifier = FilePath.GetFileNameWithoutExtension(sfd.FileName);


                    //check for bootable 
                    if (bootable.IsChecked ?? false)
                    {
                        if (bootFile.SelectedValue != null)
                        {
                            image.SetBootImage(File.OpenRead((string)bootFile.SelectedValue), BootDeviceEmulation.NoEmulation, 0);
                        }
                    }

                    foreach (string file in filelist)
                    {                 
                        
                        image.AddFile(file, file);
                    }


                    System.Threading.Tasks.Task.Factory.StartNew((Action)(()=>
                    {
                        using (Stream fs = File.Open(sfd.FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                        {
                            image.Build(fs);                            
                        }
                    
                    }));                                   
            }

        }


        public void AddFiles( string dir)
        {
            
            //add files
            string[] files = Directory.EnumerateFiles(dir).ToArray();
            
            foreach(string file in files)
            {
                 AddFile(file);
            }
            

            //search directories
            string[] dirs = Directory.EnumerateDirectories(dir).ToArray();

            foreach (string direc in dirs)
            {
                AddFiles(direc);
            }

        }

        private void directoryButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string dir = fbd.SelectedPath;
                
                foreach(string file in Directory.EnumerateFiles(dir))
                {
                    AddFile(file);
                }

                foreach (string direc in Directory.EnumerateDirectories(dir))
                {
                    AddFiles(direc);
                }


            }           
        }
    }



}
