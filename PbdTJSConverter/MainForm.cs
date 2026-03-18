using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pbd.Commom;

namespace PbdTJSConverter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            //游戏列表
            {
                ComboBox cb = this.cbTitles;
                cb.BeginUpdate();
                cb.Items.Clear();
                cb.DisplayMember = nameof(PbdCustomParams.Title);
                foreach (PbdCustomParams param in DataManager.Titles)
                {
                    cb.Items.Add(param);
                }
                cb.EndUpdate();
            }
        }

        //下拉框选择
        private void CbTitles_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;

            string s = string.Empty;
            if (cb.SelectedIndex >= 0)
            {
                s = ((PbdCustomParams)cb.SelectedItem).GetParamsString();
            }
            this.tbParams.Text = s;
        }

        //转换按钮点击
        private async void BtnConvert_OnClick(object sender, EventArgs e)
        {
            ComboBox cb = this.cbTitles;
            Button btn = (Button)sender;
            TextBox log = this.tbLog;

            if (cb.SelectedIndex < 0)
            {
                MessageBox.Show("请选择游戏", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using FolderBrowserDialog fbd = new()
            {
                AutoUpgradeEnabled = true,
                Description = "选择立绘资源文件夹",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false,
            };
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                PbdCustomParams pbd = (PbdCustomParams)cb.SelectedItem;
                string inputDir = fbd.SelectedPath;
                IProgress<string> logCB = new Progress<string>((string s) =>
                {
                    log.AppendText($"{DateTime.Now:HH-mm-ss} | {s}\r\n");
                });

                log.Clear();
                cb.Enabled = false;
                btn.Enabled = false;

                await Task.Run(() =>
                {
                    PbdTJSUtils.Convert(inputDir, pbd, logCB);
                });

                cb.Enabled = true;
                btn.Enabled = true;

                MessageBox.Show("转换完毕", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}