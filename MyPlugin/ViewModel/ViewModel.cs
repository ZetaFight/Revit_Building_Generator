using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyPlugin
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region 初始化外部事件
        ExternalEvent createGridEvent = null;
        ExternalEvent createLevelEvent = null;
        ExternalEvent createFrameEvent = null;
        ExternalEvent createFloorWallEvent = null;
        ExternalEvent showModelEvent = null;
        ExternalEvent createRooftopWallEvent = null;
        ExternalEvent editWallEvent = null;
        #endregion

        #region 控件字段
        private string _creatGridBtnContent;
        private string _creatLevelBtnContent;
        private bool _isBtnCreateFrameEnabled;
        private bool _isBtnCreateFloorWallEnabled;
        private UIElement _previewControl;
        #endregion

        #region 控件属性
        public string CreateGridBtnContent
        {
            get => _creatGridBtnContent;
            set
            {
                if (_creatGridBtnContent != value)
                {
                    _creatGridBtnContent = value;
                    OnPropertyChanged("CreateGridBtnContent");
                }
            }
        }

        public string CreateLevelBtnContent
        {
            get => _creatLevelBtnContent;
            set
            {
                if (_creatLevelBtnContent != value)
                {
                    _creatLevelBtnContent = value;
                    OnPropertyChanged("CreateLevelBtnContent");
                }
            }
        }

        public bool IsBtnCreateFrameEnabled
        {
            get => _isBtnCreateFrameEnabled;
            set
            {
                _isBtnCreateFrameEnabled = value;
                OnPropertyChanged("IsBtnCreateFrameEnabled");
            }
        }

        public bool IsBtnCreateFloorWallEnabled
        {
            get => _isBtnCreateFloorWallEnabled;
            set
            {
                _isBtnCreateFloorWallEnabled = value;
                OnPropertyChanged("IsBtnCreateFloorWallEnabled");
            }
        }

        public UIElement PreviewControl
        {
            get => _previewControl;
            set
            {
                _previewControl = value;
                OnPropertyChanged(nameof(PreviewControl));
            }
        }
        #endregion

        public ViewModel()
        {
            #region 注册外部事件
            CreateGrid createGridCommand = new CreateGrid();
            createGridEvent = ExternalEvent.Create(createGridCommand);

            CreateLevel createLevelCommand = new CreateLevel();
            createLevelEvent = ExternalEvent.Create(createLevelCommand);

            CreateFrame createFrameCommand = new CreateFrame();
            createFrameEvent = ExternalEvent.Create(createFrameCommand);

            CreateFloorWall createFloorWallCommand = new CreateFloorWall();
            createFloorWallEvent = ExternalEvent.Create(createFloorWallCommand);

            ShowModel showModelCommand = new ShowModel(this);
            showModelEvent = ExternalEvent.Create(showModelCommand);

            CreateRooftopWall createRooftopWallCommand = new CreateRooftopWall();
            createRooftopWallEvent = ExternalEvent.Create(createRooftopWallCommand);

            EditWall editWallCommand = new EditWall();
            editWallEvent = ExternalEvent.Create(editWallCommand);
            #endregion

            #region 初始化控件属性
            _creatGridBtnContent = "创建轴网";
            _creatLevelBtnContent = "创建标高";
            _isBtnCreateFrameEnabled = false;
            _isBtnCreateFloorWallEnabled = false;
            #endregion

            #region 注册控件命令
            btnMoreCommand = new RelayCommand(MoreCommand);
            btnCreateGridCommand = new RelayCommand(CreateGridCommand);
            btnCreateLevelCommand = new RelayCommand(CreateLevelCommand);
            btnCreateFrameCommand = new RelayCommand(CreateFrameCommand);
            btnCreateFloorWallCommand = new RelayCommand(CreateFloorWallCommand);
            btnShowModelCommand = new RelayCommand(ShowModelCommand);
            btnCreateRooftopWallCommand = new RelayCommand(CreateRooftopWallCommand);
            btnEditWallCommand = new RelayCommand(EditWallCommand);
            #endregion
        }

        #region 按钮命令属性
        public ICommand btnCreateGridCommand { get; }
        public ICommand btnCreateLevelCommand { get; }
        public ICommand btnCreateFrameCommand { get; }
        public ICommand btnCreateFloorWallCommand { get; }
        public ICommand btnShowModelCommand { get; }
        public ICommand btnCreateRooftopWallCommand { get; }
        public ICommand btnEditWallCommand { get; }
        public ICommand btnMoreCommand { get; }       
        #endregion

        #region 按钮命令
        private void MoreCommand(object parameter)
        {
            MessageBox.Show("敬请期待!");
        }

        private void CreateGridCommand(object parameter)
        {
            GridCmdWindow gridCmdWindow = GridCmdWindow.Instance;
            bool? dialogResult = gridCmdWindow.ShowDialog();

            if (dialogResult == true)
            {
                CreateGrid.TextV = gridCmdWindow.VerticalGridSpan;
                CreateGrid.TextH = gridCmdWindow.HorizontalGridSpan;

                createGridEvent.Raise();

                CreateGridBtnContent = "重新创建轴网";
                IsBtnCreateFrameEnabled = true;
            }

        }

        private void CreateLevelCommand(object parameter)
        {
            LevelCmdWindow levelCmdWindow = LevelCmdWindow.Instance;
            bool? dialogResult = levelCmdWindow.ShowDialog();
 
            if (dialogResult == true)
            {
                CreateLevel.TextF = levelCmdWindow.FloorHeight;

                createLevelEvent.Raise();

                CreateLevelBtnContent = "重新创建标高";
            }
            
        }

        private void CreateFrameCommand(object parameter)
        {
            createFrameEvent.Raise();
            IsBtnCreateFloorWallEnabled = true;
        }
        
        private void CreateFloorWallCommand(object parameter)
        {
            createFloorWallEvent.Raise();
        }

        private void ShowModelCommand(object parameter)
        {
            showModelEvent.Raise();         
        }

        private void CreateRooftopWallCommand(object parameter)
        {
            createRooftopWallEvent.Raise();
        }

        private void EditWallCommand(object parameter)
        {
            editWallEvent.Raise();
        }
        #endregion

        public void UpdatePreviewControl(UIElement control)
        {
            PreviewControl = control;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
