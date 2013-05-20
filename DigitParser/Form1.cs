using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitParser
{
    public partial class Form1 : Form
    {
        private readonly BackgroundWorker _worker;
        private FileDialog _dialog;
        private bool _cancel = false;
        public Form1()
        {
            InitializeComponent();
            _worker = new BackgroundWorker {WorkerReportsProgress = true};
            _worker.DoWork += WorkerOnDoWork;
            _worker.ProgressChanged += WorkerOnProgressChanged;
            _worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            progressBar1.Value = 0;
        }

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            progressBar1.Value = progressChangedEventArgs.ProgressPercentage;
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var fi = new FileInfo(textBox1.Text);
            var size = fi.Length;
            var orig = size;
            var distance = (int)(size/100.0f);
            var current = 0;
            var val = "";
            using(var fs = File.OpenText(textBox1.Text))
            {
                using(var ws = File.OpenWrite(textBox2.Text))
                {
                    while (!fs.EndOfStream)
                    {
                        current++;
                        size--;
                        if (current > distance)
                        {
                            _worker.ReportProgress(100 - (int) ((double)size/orig*100));
                            current = 0;
                        }
                        var old = val;
                        var f = (char) fs.Read();
                        int s;
                        if (!int.TryParse(f.ToString(CultureInfo.InvariantCulture), out s)) continue;

                        val += f;
                        if (int.Parse(val) <= 255 && val.Length <= 3) continue;
                        var b = Byte.Parse(old);
                        if (checkBox1.Checked)
                        {
                            if (b == 32 || (b >= 48 && b <= 57) || (b >= 97 && b <= 122) || (b >= 65 && b <= 90))
                                ws.WriteByte(b);
                        }
                        else
                            ws.WriteByte(b);

                        val = f.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        private void Button3Click(object sender, EventArgs e)
        {
            _cancel = false;
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
                Error("Input or output missing");

            if(!File.Exists(textBox1.Text))
                Error("Input file is invalid or missing");

            FileStream fs = null;
            try
            {
                fs = File.Create(textBox2.Text);
            }
            catch (Exception)
            {
                Error("Output file could not be created");
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }

            if (_cancel)
                return;

            _worker.RunWorkerAsync();

        }

        private void Error(string message)
        {
            _cancel = true;
            MessageBox.Show(message);
        }

        private void Button1Click(object sender, EventArgs e)
        {
            _dialog = new OpenFileDialog();
            if(_dialog.ShowDialog() == DialogResult.OK)
                textBox1.Text = _dialog.FileName;
        }

        private void Button2Click(object sender, EventArgs e)
        {
            _dialog = new SaveFileDialog();
            if (_dialog.ShowDialog() == DialogResult.OK)
                textBox2.Text = _dialog.FileName;
        }
    }
}
