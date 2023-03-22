using Microsoft.Office.Interop.Word;
using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Linq;


namespace PdfConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private object oMissing = System.Reflection.Missing.Value;
        private Object oFalse = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSelectDocFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                txtDocFolder.Text = dialog.SelectedPath;
            }
        }

        private void btnSelectPdfFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                txtPdfFolder.Text = dialog.SelectedPath;
            }
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            string docFolder = txtDocFolder.Text;
            string pdfFolder = txtPdfFolder.Text;
            string volumeInput = txtVolume.Text;
            int rs = 0;
            using (LoadingControl loading = new LoadingControl((x) => rs = Convert(docFolder, pdfFolder, volumeInput)))
            {
                loading.Owner = this;
                loading.ShowDialog();
            }

            switch (rs)
            {
                case -1:
                    MessageBox.Show("Quá trình chuyển đổi hoàn tất. Vui lòng vào thư mục pdf để nhận kết quả.");
                    break;

                case 1:
                    MessageBox.Show("Chưa chọn thư mục chứa tài nguyên.");
                    break;

                case 2:
                    MessageBox.Show("Thư mục chứa docs không tồn tại.");
                    break;

                case 3:
                    MessageBox.Show("Thư mục nhận kết quả không tồn tại.");
                    break;

                case 4:
                    MessageBox.Show("Số lượng file không hợp lệ.");
                    break;

                case 5:
                    MessageBox.Show("Số lượng file quá lớn.");
                    break;

                default:
                    MessageBox.Show("Lỗi chyển đổi.");
                    break;
            }
        }

        private int Convert(string docPath, string pdfPath, string volume = "3")
        {
            docPath = docPath.Trim();
            pdfPath = pdfPath.Trim();
            volume = volume?.Trim();
            if (String.IsNullOrEmpty(docPath) || String.IsNullOrEmpty(pdfPath))
            {
                return 1;
            }

            if (!Directory.Exists(docPath))
            {
                return 2;
            }

            if (!Directory.Exists(pdfPath))
            {
                return 3;
            }

            int batchVolume = 3;
            if (!string.IsNullOrEmpty(volume))
            {
                bool validateVolume = int.TryParse(volume, out batchVolume);
                if (!validateVolume)
                {
                    return 4;
                }
            }

            if(batchVolume > 10)
            {
                return 5;
            }

            var files = Directory.GetFiles(docPath).Where(x => Path.GetExtension(x) == ".docx" || Path.GetExtension(x) == ".doc");
            int skip = 0;
            string resultDir = pdfPath;

            while (files.Skip(skip).Count() > 0)
            {
                var batch = files.Skip(skip).Take(batchVolume);
                skip += batchVolume;

                List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                foreach (var file in batch)
                {
                    var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        System.Diagnostics.Debug.WriteLine(file);
                        //if (Path.GetExtension(file) != ".docx" && Path.GetExtension(file) != ".doc")
                        //{
                        //    System.Diagnostics.Debug.WriteLine("Sai định dạng");
                        //    if (Interlocked.Decrement(ref toProcess) == 0)
                        //        resetEvent.Set();
                        //    return;
                        //}

                        if (Path.GetFileName(file).StartsWith("~$"))
                        {
                            System.Diagnostics.Debug.WriteLine("File temp");
                            return;
                        }

                        Object filename = (Object)file;
                        Microsoft.Office.Interop.Word.Application wordApplication = new Microsoft.Office.Interop.Word.Application();
                        try
                        {
                            Document doc = wordApplication.Documents.Open(ref filename, ref oMissing,
                                ref oFalse, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                ref oMissing, ref oMissing, ref oMissing, ref oMissing);

                            //doc.Activate();

                            var pathfilename = Path.Combine(resultDir, Path.GetFileNameWithoutExtension(file) + ".pdf");
                            Object filename2 = (Object)pathfilename;

                            doc.SaveAs(ref filename2, WdSaveFormat.wdFormatPDF,
                                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                ref oMissing, ref oMissing, ref oMissing, ref oMissing);

                            // close word doc and word app.
                            object saveChanges = WdSaveOptions.wdDoNotSaveChanges;

                            ((_Document)doc).Close(ref saveChanges, ref oMissing, ref oMissing);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error: {0}", new object[] { ex.Message });
                            //errorFlag = true;
                        }
                        finally
                        {
                            ((_Application)wordApplication).Quit(ref oMissing, ref oMissing, ref oMissing);
                        }
                    });
                    tasks.Add(task);
                }

                System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

            }
            return -1;
        }
    }
}
