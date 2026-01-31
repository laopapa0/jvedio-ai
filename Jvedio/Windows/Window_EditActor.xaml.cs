using Jvedio.Core.AI;
using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Entity;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Framework.ORM.Utils;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Jvedio.MapperManager;
using static SuperUtils.Media.ImageHelper;
using static SuperUtils.WPF.VisualTools.WindowHelper;

namespace Jvedio
{
    /// <summary>
    /// Window_EditActor.xaml 的交互逻辑
    /// </summary>
    public partial class Window_EditActor : BaseWindow
    {
        #region "事件"
        public static Action<long> onActorInfoChanged;

        #endregion


        #region "属性"
        public long ActorID { get; set; }

        public ActorInfo CurrentActorInfo { get; set; }

        private string _lastRawResponse = "";

        #endregion

        private Window_EditActor()
        {
            InitializeComponent();
        }


        public Window_EditActor(long actorID, bool newActor = false) : this()
        {
            this.DataContext = this;

            if (newActor) {
                CurrentActorInfo = new ActorInfo();
            } else {
                if (actorID <= 0)
                    return;
                this.ActorID = actorID;
                Init();
            }

        }

        public void Init()
        {
            if (this.ActorID <= 0)
                return;
            CurrentActorInfo = ActorInfo.GetById(ActorID);
            SetGender();
        }

        private void SetGender()
        {
            if (CurrentActorInfo == null)
                return;
            if (CurrentActorInfo.Gender == Gender.Boy)
                boy.IsChecked = true;
            else
                girl.IsChecked = true;
        }

        private void SaveActor(object sender, RoutedEventArgs e)
        {
            string actorName = CurrentActorInfo.ActorName.ToProperFileName();
            if (string.IsNullOrEmpty(actorName)) {
                MessageNotify.Error(LangManager.GetValueByKey("ActorCanNotBeNull"));
                return;
            }

            CurrentActorInfo.ActorName = actorName;

            if (ActorID > 0) {
                int update = actorMapper.UpdateById(CurrentActorInfo);
                if (update > 0) {
                    ActorInfo actorInfo = ActorInfo.GetById(ActorID);
                    CurrentActorInfo.SmallImage = null;
                    CurrentActorInfo.SmallImage = actorInfo.SmallImage;

                    MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("Message_Success"));
                    onActorInfoChanged(ActorID);
                }
            } else {
                // 新增
                // 检查是否存在
                SelectWrapper<ActorInfo> wrapper = new SelectWrapper<ActorInfo>();
                wrapper.Eq("ActorName", CurrentActorInfo.ActorName.ToProperSql());
                ActorInfo actorInfo = actorMapper.SelectOne(wrapper);
                bool insert = true;
                if (actorInfo != null && !string.IsNullOrEmpty(actorInfo.ActorName)) {
                    insert = (bool)new MsgBox($"{LangManager.GetValueByKey("LibraryAlreadyHas")} {actorInfo.ActorName} {LangManager.GetValueByKey("SameActorToAdd")}").ShowDialog();
                }

                if (insert) {
                    actorMapper.Insert(CurrentActorInfo);
                    if (CurrentActorInfo.ActorID > 0) {
                        this.DialogResult = true;
                    } else
                        MessageNotify.Error(LangManager.GetValueByKey("Error"));
                }
            }
        }

        private void SetActorImage(object sender, RoutedEventArgs e)
        {
            if (CurrentActorInfo == null || string.IsNullOrEmpty(CurrentActorInfo.ActorName)) {
                MsgBox.Show(LangManager.GetValueByKey("ActorCanNotBeNull"));
                return;
            }
            string imageFileName = string.Empty;
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Title = SuperControls.Style.LangManager.GetValueByKey("ChooseFile");
            fileDialog.Filter = Window_Settings.SupportPictureFormat;
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string filename = fileDialog.FileName;
            if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
                imageFileName = filename;


            bool copied = false;
            string targetFileName = CurrentActorInfo.GetImagePath(searchExt: false);
            if (File.Exists(targetFileName)) {
                if (new MsgBox(LangManager.GetValueByKey("ActorImageExistsAndUseID")).ShowDialog() == true) {
                    string dir = System.IO.Path.GetDirectoryName(targetFileName);
                    string ext = System.IO.Path.GetExtension(targetFileName);
                    targetFileName = System.IO.Path.Combine(dir, $"{CurrentActorInfo.ActorID}_{CurrentActorInfo.ActorName}{ext}");
                    FileHelper.TryCopyFile(imageFileName, targetFileName, true);
                    copied = true;
                }
            } else {
                FileHelper.TryCopyFile(imageFileName, targetFileName);
                copied = true;
            }

            if (copied) {
                // 设置图片
                SetCurrentImage(targetFileName);
            }
        }

        private void ActorImage_Drop(object sender, DragEventArgs e)
        {
            PathType pathType = (PathType)ConfigManager.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData) {
                MsgBox.Show(LangManager.GetValueByKey("ActorImageNotSupported"));
                return;
            }


            if (CurrentActorInfo == null || string.IsNullOrEmpty(CurrentActorInfo.ActorName)) {
                MsgBox.Show(LangManager.GetValueByKey("ActorCanNotBeNull"));
                return;
            }

            string basePicPath = ConfigManager.Settings.PicPaths[pathType.ToString()].ToString();
            string saveDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(basePicPath, "Actresses"));
            string name = CurrentActorInfo.ActorName.ToProperFileName();
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dragdropFiles != null && dragdropFiles.Length > 0) {
                System.Collections.Generic.List<string> list = dragdropFiles.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(System.IO.Path.GetExtension(arg).ToLower())).ToList();
                if (list.Count > 0) {
                    string originPath = list[0];
                    if (FileHelper.IsFile(originPath)) {
                        // 设置演员头像
                        string targetFileName = CurrentActorInfo.GetImagePath(searchExt: false);
                        bool copy = true;
                        if (File.Exists(targetFileName) && new MsgBox(LangManager.GetValueByKey("ExistsToOverwrite")).ShowDialog() != true) {
                            copy = false;
                        }
                        if (copy) {
                            FileHelper.TryCopyFile(originPath, targetFileName, true);
                            SetCurrentImage(targetFileName);
                        }

                    }
                }
            }
        }

        private void SetCurrentImage(string targetFileName)
        {
            CurrentActorInfo.SmallImage = null;
            CurrentActorInfo.SmallImage = BitmapImageFromFile(targetFileName);
            onActorInfoChanged?.Invoke(CurrentActorInfo.ActorID);
        }

        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true; // 必须加
        }


        private void SetGender(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton button &&
                button.Parent is StackPanel panel &&
                panel.Children.OfType<RadioButton>().ToList() is List<RadioButton> buttonList) {
                int idx = buttonList.IndexOf(button);
                if (idx == 0) {
                    CurrentActorInfo.Gender = Gender.Boy;
                } else {
                    CurrentActorInfo.Gender = Gender.Girl;
                }
            }
        }

        private void onTextBoxFocus(object sender, RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.GotFocus(sender);
        }

        private void onTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.LostFocus(sender);
        }

        /// <summary>
        /// 使用 AI 补全演员信息
        /// </summary>
        private async void CompleteActorInfoWithAI(object sender, RoutedEventArgs e)
        {
            if (CurrentActorInfo == null || string.IsNullOrWhiteSpace(CurrentActorInfo.ActorName))
            {
                MessageNotify.Error("请先输入演员名字");
                return;
            }

            // 检查是否已配置千问 API
            if (!DashScopeClient.IsConfigured())
            {
                MessageNotify.Error("未配置千问 API，请在 .env 文件中配置 DASHSCOPE_API_KEY");
                return;
            }

            try
            {
                // 禁用按钮，防止重复点击
                btnAICompletion.IsEnabled = false;
                btnAICompletion.Content = "正在获取...";

                // 调用 AI 补全服务
                ActorCompletionResult result = await ActorInfoCompletionService.CompleteByNameAsync(CurrentActorInfo);

                if (result.Success && result.CompletedInfo != null)
                {
                    // 更新演员信息
                    var info = result.CompletedInfo;

                    // 生日
                    if (!string.IsNullOrEmpty(info.Birthday))
                    {
                        CurrentActorInfo.Birthday = info.Birthday;
                    }

                    // 年龄
                    if (info.Age > 0)
                    {
                        CurrentActorInfo.Age = info.Age;
                    }

                    // 血型
                    if (!string.IsNullOrEmpty(info.BloodType))
                    {
                        CurrentActorInfo.BloodType = info.BloodType;
                    }

                    // 身高
                    if (info.Height > 0)
                    {
                        CurrentActorInfo.Height = info.Height;
                    }

                    // 体重
                    if (info.Weight > 0)
                    {
                        CurrentActorInfo.Weight = info.Weight;
                    }

                    // 罩杯
                    if (info.Cup != 'Z')
                    {
                        CurrentActorInfo.Cup = info.Cup;
                    }

                    // 胸围
                    if (info.Chest > 0)
                    {
                        CurrentActorInfo.Chest = info.Chest;
                    }

                    // 腰围
                    if (info.Waist > 0)
                    {
                        CurrentActorInfo.Waist = info.Waist;
                    }

                    // 臀围
                    if (info.Hipline > 0)
                    {
                        CurrentActorInfo.Hipline = info.Hipline;
                    }

                    // 出生地
                    if (!string.IsNullOrEmpty(info.BirthPlace))
                    {
                        CurrentActorInfo.BirthPlace = info.BirthPlace;
                    }

                    // 爱好
                    if (!string.IsNullOrEmpty(info.Hobby))
                    {
                        CurrentActorInfo.Hobby = info.Hobby;
                    }

                    // 保存原始返回内容
                    _lastRawResponse = result.RawResponse ?? "无原始返回内容";

                    var message = $"AI 补全成功！置信度：{result.Confidence:P0}\n\n";
                    message += $"已更新字段：生日={info.Birthday}, 年龄={info.Age}, 身高={info.Height}\n";
                    message += $"可以点击\"查看原始返回\"按钮查看完整的 AI 返回内容";

                    MessageNotify.Success(message);
                }
                else
                {
                    // 保存原始返回内容
                    _lastRawResponse = result.RawResponse ?? "无原始返回内容";

                    var errorMsg = $"AI 补全失败：{result.Message}\n\n";
                    if (!string.IsNullOrEmpty(result.RawResponse))
                    {
                        errorMsg += $"原始返回：{result.RawResponse}";
                    }
                    MessageNotify.Error(errorMsg);
                }
            }
            catch (Exception ex)
            {
                MessageNotify.Error($"AI 补全出错：{ex.Message}");
            }
            finally
            {
                // 恢复按钮状态
                btnAICompletion.IsEnabled = true;
                btnAICompletion.Content = "AI 补全信息";
            }
        }

        /// <summary>
        /// 查看原始返回内容
        /// </summary>
        private void ShowRawResponse_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_lastRawResponse))
            {
                MessageNotify.Error("还没有原始返回内容，请先点击\"AI 补全信息\"按钮");
                return;
            }

            var dialog = new Jvedio.Windows.Dialog_ShowRawResponse(_lastRawResponse);
            dialog.ShowDialog();
        }
    }
}