using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for CSVImportDialog.xaml
    /// </summary>
    public partial class CSVImportDialog : SurfaceWindow, CSVImport.CSVMsgReceiver
    {

        private string csv_file_path;

        public CSVImportDialog()
        {
            InitializeComponent();
            csv_file_path = "";
            addCSVButton.IsEnabled = false;
        }

        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "CSV Files(*.CSV)|*.CSV|All files(*.*)|*.*";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                csv_file_path = ofd.FileName;
                fileNameLbl.Content = csv_file_path;
                addCSVButton.IsEnabled = true;
            }
        }

        private void addCSVButton_Click(object sender, RoutedEventArgs e)
        {
            CSVImport.CSVImporter.setOutputWindow(this);
            CSVImport.CSVImporter.DoBatchImport(csv_file_path);
        }

        // Displays a message to the output console.
        // This method is called by the CSV Importer to display the messages that it would
        // have otherwise written to standard out.
        public void outputMessage(String msg)
        {
            csvOutputLbl.AppendText(msg);
        }
    }
}
