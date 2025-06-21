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
    /// GridCmdWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GridCmdWindow : Window
    {
        #region 字段
        private string _verticalGridSpan;
        private string _horizontalGridSpan;
        #endregion

        #region 属性
        public string VerticalGridSpan
        {
            get { return _verticalGridSpan; }
            set
            {
                _verticalGridSpan = value;
            }
        }

        public string HorizontalGridSpan
        {
            get { return _horizontalGridSpan; }
            set
            {
                _horizontalGridSpan = value;
            }
        }
        #endregion

        #region 单例模式
        private GridCmdWindow()
        {
            InitializeComponent();
            txtBoxVerticalGridSpan.Text = "8400,8400,8400,8400,8400";
            txtBoxHorizontalGridSpan.Text = "7200,7000,7200";
        }

        private static GridCmdWindow _instance;

        public static GridCmdWindow Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new GridCmdWindow();
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
                        MessageBox.Show("轴线间距不能为0");
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
            if (txtBoxVerticalGridSpan.Text != "" && txtBoxHorizontalGridSpan.Text != "")
            {

                if (CheckInput(txtBoxVerticalGridSpan.Text) && CheckInput(txtBoxHorizontalGridSpan.Text))
                {
                    VerticalGridSpan = txtBoxVerticalGridSpan.Text;
                    HorizontalGridSpan = txtBoxHorizontalGridSpan.Text;
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
                MessageBox.Show("未输入轴线间距。");
                return;
            }
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            VerticalGridSpan = null;
            HorizontalGridSpan = null;
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
