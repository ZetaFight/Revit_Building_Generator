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

namespace MyPlugin
{
    /// <summary>
    /// LevelCmdWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LevelCmdWindow : Window
    {

        #region 字段
        private string _floorHeight;
        #endregion

        #region 属性
        public string FloorHeight
        {
            get { return _floorHeight; }
            set
            {
                _floorHeight = value;
            }
        }
        #endregion

        #region 单例模式
        private LevelCmdWindow()
        {
            InitializeComponent();
            txtBoxLevel.Text = "4500,3600,3600,3600,3600,3600,3600,3600";
        }

        private static LevelCmdWindow _instance;

        public static LevelCmdWindow Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new LevelCmdWindow();
                }
                return _instance;
            }
        }
        #endregion

        #region 功能函数
        private bool CheckInput(string text)
        {
            string[] textTrimed;
            textTrimed = text.Split(',');
            foreach (string num in textTrimed)
            {
                try
                {
                    Convert.ToInt32(num);
                    if (num == "0")
                    {
                        MessageBox.Show("层高不能为0");
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 控件函数
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (txtBoxLevel.Text != "")
            {
                if (CheckInput(txtBoxLevel.Text))
                {
                    FloorHeight = txtBoxLevel.Text;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("输入格式不正确。");
                    return;
                }
            }
            else
            {
                MessageBox.Show("未输入各层层高。");
                return;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            FloorHeight = null;
            this.DialogResult = false;
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _instance = null;
        }
        #endregion
    }
}
