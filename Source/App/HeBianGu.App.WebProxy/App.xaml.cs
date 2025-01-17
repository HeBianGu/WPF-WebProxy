using HeBianGu.Base.WpfBase;
using HeBianGu.Control.Guide;
using HeBianGu.Control.ThemeSet;
using HeBianGu.General.WpfControlLib;
using HeBianGu.Service.Mvp;
using HeBianGu.Systems.About;
using HeBianGu.Systems.Feedback;
using HeBianGu.Systems.Identity;
using HeBianGu.Systems.Logger;
using HeBianGu.Systems.Notification;
using HeBianGu.Systems.Operation;
using HeBianGu.Systems.Project;
using HeBianGu.Systems.Repository;
using HeBianGu.Systems.Setting;
using HeBianGu.Systems.Survey;
using HeBianGu.Systems.Upgrade;
using HeBianGu.Systems.WinTool;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Media;

namespace HeBianGu.App.WebProxy
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : ApplicationBase
    {
        protected override MainWindowBase CreateMainWindow(StartupEventArgs e)
        {
            return new ShellWindow();
        }
        protected override void InitCatchExcetion()
        {
            //#if DEBUG
            //            return;
            //#endif
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }


        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddSetting();

            //  Do ：启用窗口加载和隐藏动画 需要引用 HeBianGu.Service.Animation
            services.AddWindowAnimation();

            //  Do ：启用消息提示 需要引用 HeBianGu.Control.Message
            services.AddMessageProxy();
            services.AddPropertyGridMessage();
            services.AddAutoColumnPagedDataGridMessage();
            services.AddNotifyMessage();
            //  Do ：启用对话框 需要引用HeBianGu.Window.MessageDialog
            services.AddWindowDialog();
            services.AddObjectWindowDialog();

            services.AddThemeRightViewPresenter();

            services.AddIdentity();
            services.AddStart(x =>
            {
                x.ProductFontSize = 85;
            });

            //services.AddLicense();
            //services.AddVip();

            services.AddProjectDefault();

            //  Do ：启用右上见配置按钮 需要添加引用HeBianGu.Systems.Setting
            services.AddSetting();
            //services.AddSettingViewPrenter();
            services.AddSettingPath();

            services.AddXmlSerialize();
            services.AddXmlMeta();

            #region - WindowMenu - 

            services.AddUserViewPresenter(x =>
            {
                x.PredefinePath = System.IO.Path.Combine("View");
            });
            services.AddRoleViewPresenter(x =>
            {
                x.PredefinePath = System.IO.Path.Combine("View");
            });
            services.AddOparationViewPresenter(x =>
            {
                x.PredefinePath = System.IO.Path.Combine("View");
            });
            services.AddDebugRepositoryViewPresenter(x =>
            {
                x.PredefinePath = System.IO.Path.Combine("View");
            });
            services.AddNewProjectTreeViewPresenter(x =>
            {
                x.PredefinePath = System.IO.Path.Combine("File");
            });

            services.AddLoginOparationViewPresenter();
            services.AddLoggerViewPresenter();
            services.AddHomeViewPresenter(x =>
            {
                x.PredefinePath = System.IO.Path.Combine("View");
                x.AddPersenter(LoginOparationViewPresenter.Instance);
                x.AddPersenter(LoggerViewPresenter.Instance);
            });

            services.AddWindowMenuViewPresenter(x =>
            {
                x.AddPresenter(NewProjectTreeViewPresenter.Instance);
                //x.AddPreDefinePath(System.IO.Path.Combine("文件", "新建", "工程管理"));
                //x.AddPreDefinePath(System.IO.Path.Combine("文件", "打开", "工程管理"));
                //x.AddPreDefinePath(System.IO.Path.Combine("编辑", "复制", "工程管理"));
                //x.AddPreDefinePath(System.IO.Path.Combine("视图", "打开", "工程管理", "工程管理"));
                x.AddPresenter(HomeViewPresenter.Instance);
                x.AddPresenter(UserViewPresenter.Instance);
                x.AddPresenter(RoleViewPresenter.Instance);
                x.AddPresenter(OparationViewPresenter.Instance);
                x.AddPresenter(new RepositoryViewPresenter()
                {
                    Name = "错误日志",
                    ViewModel = new RepositoryViewModel<hl_dm_error>(),
                    PredefinePath = "View"
                });
                x.AddPresenter(new RepositoryViewPresenter()
                {
                    Name = "运行日志",
                    ViewModel = new RepositoryViewModel<hl_dm_info>(),
                    PredefinePath = "View"
                });
                x.AddPresenter(DebugRepositoryViewPresenter.Instance);
                x.AddPresenter(new RepositoryViewPresenter()
                {
                    Name = "警告日志",
                    ViewModel = new RepositoryViewModel<hl_dm_warn>(),
                    PredefinePath = "View"
                });
                x.AddPresenter(new RepositoryViewPresenter()
                {
                    Name = "严重日志",
                    ViewModel = new RepositoryViewModel<hl_dm_fatal>(),
                    PredefinePath = "View"
                });
            });



            //services.AddWindowMenuViewPresenter(x =>
            //{
            //    x.AddPresenter(NewProjectTreeViewPresenter.Instance);
            //    x.AddPresenter();
            //});
            #endregion

            #region - WindowStatus -
            services.AddWinToolViewPresenter();
            services.AddWinSpecialFolderViewPresenter();
            services.AddNotificationViewPresenter();
            services.AddFastFileViewPresenter();
            services.AddExtensionViewPresenter();
            services.AddFavoriteViewPresenter();
            services.AddRecenterViewPresenter();
            services.AddProjectViewPresenter();
            services.AddAsnycProgressViewPresenter();
            services.AddCopyRightViewPresenter();
            services.AddWindowMessageToolViewPresenter();
            services.AddWindowStatusViewPresenter(x =>
            {
                x.AddPersenter(CopyRightViewPresenter.Instance);
                //x.AddPersenter(ProjectViewPresenter.Instance);
                x.AddPersenter(WinToolViewPresenter.Instance);
                x.AddPersenter(WinSpecialFolderViewPresenter.Instance);
                x.AddPersenter(NotificationViewPresenter.Instance);
                x.AddPersenter(FastFileViewPresenter.Instance);
                x.AddPersenter(ExtensionViewPresenter.Instance);
                x.AddPersenter(FavoriteViewPresenter.Instance);
                x.AddPersenter(RecenterViewPresenter.Instance);
                x.AddPersenter(AsnycProgressViewPresenter.Instance);
                x.AddPersenter(WindowMessageToolViewPresenter.Instance);
            });
            #endregion

            #region - More -
            services.AddUpgradeViewPresenter();
            //services.AddLicense();
            //services.AddLicenseViewPresenter();
            //services.AddVip();
            //services.AddVipViewPresenter();
            services.AddSurveyViewPresenter();
            services.AddFeedbackViewPresenter();
            //services.AddLogoutViewPresenter();
            services.AddXmlWebSerializerService();
            services.AddAutoUpgrade(x =>
            {
                x.Uri = "https://gitee.com/hebiangu/wpf-auto-update/raw/master/Install/WebProxy/AutoUpdate.xml";
                x.UseIEDownload = true;
            });
            services.AddAboutViewPresenter();
            services.AddWebsiteViewPresenter();
            services.AddMoreViewPresenter(x =>
            {
                x.AddPersenter(UpgradeViewPresenter.Instance);
                //x.AddPersenter(LicenseViewPresenter.Instance);
                //x.AddPersenter(VipViewPresenter.Instance);
                x.AddPersenter(SurveyViewPresenter.Instance);
                x.AddPersenter(LogoutViewPresenter.Instance);
                x.AddPersenter(FeedbackViewPresenter.Instance);
                x.AddPersenter(WebsiteViewPresenter.Instance);
                x.AddPersenter(AboutViewPresenter.Instance);
                //x.AddPersenter(LogoutViewPresenter.Instance);

            });
            #endregion

            #region - WindowCaption -
            services.AddLoginViewPresenter();
            services.AddGuideViewPresenter();
            services.AddHideWindowViewPresenter();
            services.AddSettingViewPrenter();
            services.AddHideWindowViewPresenter();
            services.AddThemeRightViewPresenter();
            services.AddWindowCaptionViewPresenter(x =>
            {
                x.AddPersenter(LoginViewPresenter.Instance);
                x.AddPersenter(MoreViewPresenter.Instance);
                x.AddPersenter(GuideViewPresenter.Instance);
                x.AddPersenter(HideWindowViewPresenter.Instance);
                x.AddPersenter(SettingViewPresenter.Instance);
                x.AddPersenter(ThemeRightToolViewPresenter.Instance);
            });

            #endregion

            #region - LoginManager -
            services.AddUserViewPresenter();
            services.AddRoleViewPresenter();
            services.AddOparationViewPresenter();
            services.AddPasswordEditViewPresenter();
            services.AddLogoutViewPresenter();
            services.AddLoginManagerViewPresenter(x =>
            {
                x.AddPersenter(UserViewPresenter.Instance);
                x.AddPersenter(RoleViewPresenter.Instance);
                x.AddPersenter(PasswordEditViewPresenter.Instance);
                x.AddPersenter(OparationViewPresenter.Instance);
                x.AddPersenter(LogoutViewPresenter.Instance);

            });
            #endregion

            #region - DataBase -
            services.AddDbContext();
            services.AddSystemRepository();
            services.AddDataBaseInitService();
            services.AddDbLogger();
            #endregion


        }

        protected override void Configure(IApplicationBuilder app)
        {
            base.Configure(app);
            app.UseSetting(x =>
            {
                x.Settings.Add(AppSetting.Instance);
            });

            //  Do：设置默认主题
            app.UseLocalTheme(l =>
            {
                l.AccentColor = (Color)ColorConverter.ConvertFromString("#FF0093FF");
                l.DefaultFontSize = 14D;
                l.FontSize = FontSize.Small;
                l.ItemHeight = 35;
                l.RowHeight = 40;
                //l.ItemWidth = 120;
                l.ItemCornerRadius = 5;
                l.AnimalSpeed = 5000;
                l.AccentColorSelectType = 0;
                l.IsUseAnimal = false;
                l.ThemeType = ThemeType.DarkBlack;
                l.Language = Language.Chinese;
                l.StyleType = StyleType.Single;
                l.AccentBrushType = AccentBrushType.LinearGradientBrush;
            });


        }

    }


    [Displayer(Name = "应用配置", GroupName = SystemSetting.GroupApp)]
    public class AppSetting : LazySettingInstance<AppSetting>
    {
        private int _port;
        [DefaultValue(8123)]
        [Display(Name = "端口")]
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                RaisePropertyChanged();
            }
        }


    }
}
