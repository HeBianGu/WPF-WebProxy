using HeBianGu.Base.WpfBase;
using HeBianGu.Control.Filter;
using HeBianGu.Control.LayerBox;
using HeBianGu.Control.PropertyGrid;
using HeBianGu.General.WpfControlLib;
using HeBianGu.Service.AppConfig;
using HeBianGu.Systems.Project;
using HeBianGu.Systems.XmlSerialize;
using HeBianGu.WebProxy.Titanium;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Http.Responses;
using Titanium.Web.Proxy.Models;

namespace HeBianGu.App.WebProxy
{
    internal class ShellViewModel : NotifyPropertyChanged
    {
        public ObservableCollection<ICommand> Commands { get; } = new ObservableCollection<ICommand>();
        private readonly ProxyServer _proxyServer;
        private readonly Dictionary<HttpWebClient, SessionListItem> sessionDictionary = new Dictionary<HttpWebClient, SessionListItem>();
        private readonly IPropertyConditonable _conditionable = new ButtonPropertyConfidtionsPrensenter(typeof(SessionListItem), x => x.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false);

        public ShellViewModel()
        {
            _conditionable.ConditionChanged += (l, k) =>
            {
                this.Sessions = this._cache.Where(x => k.IsMatch(x)).ToObservable();
            };

            this.Triggers.Add(new PredicateTrigger(x =>
            {
                if (x.Host == "www.baidu.com")
                {
                    MessageProxy.Notify.ShowInfo("你操作了百度：进程" + x.Process);
                }
            })
            { Name = "当访问[www.baidu.com]时触发弹窗提示" });

            this.Triggers.Add(new PredicateTrigger(x =>
            {
                if (x.Host == "www.bilibili.com")
                {
                    MessageProxy.Notify.ShowInfo("你操作了bilibili：进程" + x.Process);
                }
            })
            { Name = "当访问[www.bilibili.com]时触发弹窗提示" });

            this.Triggers.Add(new PredicateTrigger(x =>
            {
                if (x.Host == "passport.bilibili.com" && x.Url == "/x/passport-login/web/login")
                {
                    //var r = Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //    return MessageProxy.Windower.ShowSumit(x.HttpClient.Response.BodyString, "bilibili登录响应的数据包");
                    //});

                    MessageProxy.Presenter.Show(x.HttpClient.Response.BodyString, null, "bilibili登录响应的数据包");
                    //if (r == false)
                    //    return;

                    //MessageProxy.Messager.ShowStringProgress(x =>
                    //{
                    //    for (int i = 0; i < 100; i++)
                    //    {
                    //        x.MessageStr = $"正在根据登录token爬取视频{i + 1}/{100}条";
                    //        Thread.Sleep(20);
                    //    }
                    //});
                    //MessageProxy.Snacker.ShowTime("演示完成");
                }
            })
            { Name = "检测bilibili登录操作，获取token,爬取视频" });

            _proxyServer = new ProxyServer();

            //proxyServer.EnableHttp2 = true;

            //proxyServer.CertificateManager.CertificateEngine = CertificateEngine.DefaultWindows;

            ////Set a password for the .pfx file
            //proxyServer.CertificateManager.PfxPassword = "PfxPassword";

            ////Set Name(path) of the Root certificate file
            //proxyServer.CertificateManager.PfxFilePath = @"C:\NameFolder\rootCert.pfx";

            ////do you want Replace an existing Root certificate file(.pfx) if password is incorrect(RootCertificate=null)?  yes====>true
            //proxyServer.CertificateManager.OverwritePfxFile = true;

            ////save all fake certificates in folder "crts"(will be created in proxy dll directory)
            ////if create new Root certificate file(.pfx) ====> delete folder "crts"
            //proxyServer.CertificateManager.SaveFakeCertificates = true;

            _proxyServer.ForwardToUpstreamGateway = true;

            //increase the ThreadPool (for server prod)
            //proxyServer.ThreadPoolWorkerThread = Environment.ProcessorCount * 6;

            ////if you need Load or Create Certificate now. ////// "true" if you need Enable===> Trust the RootCertificate used by this proxy server
            //proxyServer.CertificateManager.EnsureRootCertificate(true);

            ////or load directly certificate(As Administrator if need this)
            ////and At the same time chose path and password
            ////if password is incorrect and (overwriteRootCert=true)(RootCertificate=null) ====> replace an existing .pfx file
            ////note : load now (if existed)
            //proxyServer.CertificateManager.LoadRootCertificate(@"C:\NameFolder\rootCert.pfx", "PfxPassword");

            //var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000);
            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, AppSetting.Instance.Port);
            _proxyServer.AddEndPoint(explicitEndPoint);
            //proxyServer.UpStreamHttpProxy = new ExternalProxy
            //{
            //    HostName = "158.69.115.45",
            //    Port = 3128,
            //    UserName = "Titanium",
            //    Password = "Titanium",
            //};

            //var socksEndPoint = new SocksProxyEndPoint(IPAddress.Any, 1080, true)
            //{
            //    // Generic Certificate hostname to use
            //    // When SNI is disabled by client
            //    //GenericCertificateName = "google.com"
            //};

            //proxyServer.AddEndPoint(socksEndPoint);

            _proxyServer.BeforeRequest += ProxyServer_BeforeRequest;
            _proxyServer.BeforeResponse += ProxyServer_BeforeResponse;
            _proxyServer.AfterResponse += ProxyServer_AfterResponse;
            explicitEndPoint.BeforeTunnelConnectRequest += ProxyServer_BeforeTunnelConnectRequest;
            explicitEndPoint.BeforeTunnelConnectResponse += ProxyServer_BeforeTunnelConnectResponse;
            _proxyServer.ClientConnectionCountChanged += delegate
            {
                //Application.Current.Dispatcher.Invoke(() => { ClientConnectionCount = _proxyServer.ClientConnectionCount; });
                ClientConnectionCount = _proxyServer.ClientConnectionCount;
            };
            _proxyServer.ServerConnectionCountChanged += delegate
            {
                //Application.Current.Dispatcher.Invoke(() => { ServerConnectionCount = _proxyServer.ServerConnectionCount; });
                ServerConnectionCount = _proxyServer.ServerConnectionCount;
            };
            _proxyServer.Start();

            //_proxyServer.SetAsSystemProxy(explicitEndPoint, ProxyProtocolType.AllHttp);

            //this.PropertyConfidtions = new PropertyConfidtionsPrensenter(typeof(SessionListItem), x => x.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false) { ID = "Default" };
            //this.PropertyConfidtions.Load();
        }

        private int _clientConnectionCount;
        public int ClientConnectionCount
        {
            get { return _clientConnectionCount; }
            set
            {
                _clientConnectionCount = value;
                RaisePropertyChanged();
            }
        }

        private int _serverConnectionCount;
        public int ServerConnectionCount
        {
            get { return _serverConnectionCount; }
            set
            {
                _serverConnectionCount = value;
                RaisePropertyChanged();
            }
        }

        private SessionListItem _selectedSession;
        public SessionListItem SelectedSession
        {
            get { return _selectedSession; }
            set
            {
                if (value == null && this.Sessions.Count > 0)
                    return;
                _selectedSession = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ITrigger> _triggers = new ObservableCollection<ITrigger>();
        /// <summary> 说明  </summary>
        public ObservableCollection<ITrigger> Triggers
        {
            get { return _triggers; }
            set
            {
                _triggers = value;
                RaisePropertyChanged();
            }
        }

        private List<SessionListItem> _cache = new List<SessionListItem>();

        private ObservableCollection<SessionListItem> _sessions = new ObservableCollection<SessionListItem>();
        public ObservableCollection<SessionListItem> Sessions
        {
            get { return _sessions; }
            set
            {
                _sessions = value;
                RaisePropertyChanged();
            }
        }


        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                RaisePropertyChanged();
            }
        }

        //private IPropertyConfidtions _propertyConfidtions;
        //public IPropertyConfidtions PropertyConfidtions
        //{
        //    get { return _propertyConfidtions; }
        //    set
        //    {
        //        _propertyConfidtions = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public IConditionable Conditionable => _conditionable;


        public RelayCommand StartCommand => new RelayCommand(l =>
        {
            _proxyServer.SetAsSystemProxy((ExplicitProxyEndPoint)_proxyServer.ProxyEndPoints[0], ProxyProtocolType.AllHttp);
            this.IsRunning = true;
        });

        public RelayCommand StopCommand => new RelayCommand(l =>
        {
            _proxyServer.RestoreOriginalProxySettings();
            this.IsRunning = false;

        });

        public RelayCommand OpenIECommand { get; set; } = new RelayCommand(l =>
        {
            string target = "http://www.microsoft.com";
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
        });

        //public RelayCommand FilterCommand => new RelayCommand(async l =>
        //{
        //    var r = await MessageProxy.Presenter.Show(_propertyConfidtions, null, "Filter");
        //    if (r == true)
        //    {
        //        _propertyConfidtions.Save();
        //        this.Sessions = this._cache.Where(x => this._propertyConfidtions.IsMatch(x)).ToObservable();
        //        MessageProxy.Snacker.ShowTime("保存成功");
        //    }
        //});

        //public RelayCommand FilterChangedCommand => new RelayCommand(l =>
        //{
        //    if (l is Popup popup && popup.IsOpen == false)
        //        return;
        //    this.Sessions = this._cache.Where(x => this._propertyConfidtions.IsMatch(x)).ToObservable();
        //    this._propertyConfidtions.Save();
        //});

        public RelayCommand SaveProjectCommand => new RelayCommand(async l =>
        {
            var item = new ProjectItem() { Title = DateTime.Now.ToString("yyyyMMddHHmmssfff"), Path = SystemPathSetting.Instance.Project };
            var r = await MessageProxy.PropertyGrid.ShowEdit(item, null, "Save", x => x.UsePropertyNames = "Title");
            if (r == false)
                return;
            ProjectService.Instance.Add(item);
            r = ProjectService.Instance.Save(out string message);
            if (r == false)
                await MessageProxy.Messager.ShowSumit(message);
            else
                MessageProxy.Snacker.ShowTime("保存成功");
        }, x => this.Sessions.Count > 0);

        private async Task ProxyServer_BeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
        {
            try
            {
                var hostname = e.HttpClient.Request.RequestUri.Host;
                if (hostname.EndsWith("webex.com"))
                    e.DecryptSsl = false;
                //await Application.Current.Dispatcher.InvokeAsync(() => { AddSession(e); });
                this.AddSession(e);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private async Task ProxyServer_BeforeTunnelConnectResponse(object sender, TunnelConnectSessionEventArgs e)
        {
            //await Application.Current.Dispatcher.InvokeAsync(() =>
            //{
            //    if (sessionDictionary.TryGetValue(e.HttpClient, out var item)) 
            //        item.Update(e);
            //});
            try
            {
                if (sessionDictionary.TryGetValue(e.HttpClient, out var item))
                    item.Update(e);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private async Task ProxyServer_BeforeRequest(object sender, SessionEventArgs e)
        {
            try
            {
                //if (e.HttpClient.Request.HttpVersion.Major != 2) return;
                SessionListItem item = AddSession(e); ;
                //await Application.Current.Dispatcher.InvokeAsync(() => { item = AddSession(e); });
                if (e.HttpClient.Request.HasBody)
                {
                    e.HttpClient.Request.KeepBody = true;
                    await e.GetRequestBody();
                    //if (item == SelectedSession) await Application.Current.Dispatcher.InvokeAsync(SelectedSessionChanged);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private async Task ProxyServer_BeforeResponse(object sender, SessionEventArgs e)
        {
            try
            {
                SessionListItem item = null;
                //await Application.Current.Dispatcher.InvokeAsync(() =>
                //{
                //    if (sessionDictionary.TryGetValue(e.HttpClient, out item)) 
                //        item.Update(e);
                //});

                if (sessionDictionary.TryGetValue(e.HttpClient, out item))
                    item.Update(e);

                //e.HttpClient.Response.Headers.AddHeader("X-Titanium-Header", "HTTP/2 works");

                //e.SetResponseBody(Encoding.ASCII.GetBytes("TITANIUMMMM!!!!"));

                if (item != null)
                {
                    if (e.HttpClient.Response.HasBody)
                    {
                        e.HttpClient.Response.KeepBody = true;
                        await e.GetResponseBody();
                        //await Application.Current.Dispatcher.InvokeAsync(() => { item.Update(e); });
                        item.Update(e);
                        //if (item == SelectedSession)
                        //    await Application.Current.Dispatcher.InvokeAsync(SelectedSessionChanged);
                    }
                    foreach (var rule in this.Triggers)
                    {
                        if (rule.IsVisible)
                            rule.Invoke(item);
                    }
                }




            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }


        }

        private async Task ProxyServer_AfterResponse(object sender, SessionEventArgs e)
        {
            //await Application.Current.Dispatcher.InvokeAsync(() =>
            //{
            //    if (sessionDictionary.TryGetValue(e.HttpClient, out var item)) item.Exception = e.Exception;
            //});

            try
            {
                if (sessionDictionary.TryGetValue(e.HttpClient, out var item))
                    item.Exception = e.Exception;

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private SessionListItem AddSession(SessionEventArgsBase e)
        {
            var item = CreateSessionListItem(e);
            //Sessions.Add(item);
            sessionDictionary.Add(e.HttpClient, item);
            this._cache.Insert(0, item);
            if (!_conditionable.IsMatch(item))
                return item;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                      {
                          this.Sessions.Insert(0, item);
                      }));


            //if (item.HttpClient.Response.HasBody)
            //    System.Diagnostics.Debug.WriteLine(item.HttpClient.Response.BodyString);

            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    this.Sessions.Insert(0, item);
            //});

            //foreach (var rule in this.Triggers)
            //{
            //    if (rule.IsVisible)
            //        rule.Invoke(item);
            //}
            return item;
        }

        private SessionListItem CreateSessionListItem(SessionEventArgsBase e)
        {
            //lastSessionNumber++;
            var isTunnelConnect = e is TunnelConnectSessionEventArgs;
            var item = new SessionListItem
            {
                //Number = lastSessionNumber,
                ClientConnectionId = e.ClientConnectionId,
                ServerConnectionId = e.ServerConnectionId,
                HttpClient = e.HttpClient,
                ClientRemoteEndPoint = e.ClientRemoteEndPoint,
                ClientLocalEndPoint = e.ClientLocalEndPoint,
                IsTunnelConnect = isTunnelConnect
            };

            //if (isTunnelConnect || e.HttpClient.Request.UpgradeToWebSocket)
            e.DataReceived += (sender, args) =>
            {
                var session = (SessionEventArgsBase)sender;
                if (sessionDictionary.TryGetValue(session.HttpClient, out var li))
                {
                    var connectRequest = session.HttpClient.ConnectRequest;
                    var tunnelType = connectRequest?.TunnelType ?? TunnelType.Unknown;
                    if (tunnelType != TunnelType.Unknown)
                        li.Protocol = TunnelTypeToString(tunnelType);

                    li.ReceivedDataCount += args.Count;

                    //if (tunnelType == TunnelType.Http2)
                    AppendTransferLog(session.GetHashCode() + (isTunnelConnect ? "_tunnel" : "") + "_received",
                        args.Buffer, args.Offset, args.Count);
                }
            };

            e.DataSent += (sender, args) =>
            {
                var session = (SessionEventArgsBase)sender;
                if (sessionDictionary.TryGetValue(session.HttpClient, out var li))
                {
                    var connectRequest = session.HttpClient.ConnectRequest;
                    var tunnelType = connectRequest?.TunnelType ?? TunnelType.Unknown;
                    if (tunnelType != TunnelType.Unknown)
                        li.Protocol = TunnelTypeToString(tunnelType);
                    li.SentDataCount += args.Count;

                    //if (tunnelType == TunnelType.Http2)
                    AppendTransferLog(session.GetHashCode() + (isTunnelConnect ? "_tunnel" : "") + "_sent",
                        args.Buffer, args.Offset, args.Count);
                }
            };

            if (e is TunnelConnectSessionEventArgs te)
            {
                te.DecryptedDataReceived += (sender, args) =>
                {
                    var session = (SessionEventArgsBase)sender;
                    //var tunnelType = session.HttpClient.ConnectRequest?.TunnelType ?? TunnelType.Unknown;
                    //if (tunnelType == TunnelType.Http2)
                    AppendTransferLog(session.GetHashCode() + "_decrypted_received", args.Buffer, args.Offset,
                        args.Count);
                };

                te.DecryptedDataSent += (sender, args) =>
                {
                    var session = (SessionEventArgsBase)sender;
                    //var tunnelType = session.HttpClient.ConnectRequest?.TunnelType ?? TunnelType.Unknown;
                    //if (tunnelType == TunnelType.Http2)
                    AppendTransferLog(session.GetHashCode() + "_decrypted_sent", args.Buffer, args.Offset, args.Count);
                };
            }

            item.Update(e);
            return item;
        }

        private void AppendTransferLog(string fileName, byte[] buffer, int offset, int count)
        {
            //string basePath = @"c:\!titanium\";
            //using (var fs = new FileStream(basePath + fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
            //{
            //    fs.Write(buffer, offset, count);
            //}
        }

        private string TunnelTypeToString(TunnelType tunnelType)
        {
            switch (tunnelType)
            {
                case TunnelType.Https:
                    return "https";
                case TunnelType.Websocket:
                    return "websocket";
                case TunnelType.Http2:
                    return "http2";
            }

            return null;
        }

        //private void ListViewSessions_OnKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Delete)
        //    {
        //        var isSelected = false;
        //        var selectedItems = ((ListView)sender).SelectedItems;
        //        //Sessions.SuppressNotification = true;
        //        foreach (var item in selectedItems.Cast<SessionListItem>().ToArray())
        //        {
        //            if (item == SelectedSession) isSelected = true;

        //            Sessions.Remove(item);
        //            sessionDictionary.Remove(item.HttpClient);
        //        }

        //        //Sessions.SuppressNotification = false;

        //        if (isSelected) SelectedSession = null;
        //    }
        //}

        private string _responseText;
        /// <summary> 说明  </summary>
        public string ResponseText
        {
            get { return _responseText; }
            set
            {
                _responseText = value;
                RaisePropertyChanged();
            }
        }


        private string _requestText;
        /// <summary> 说明  </summary>
        public string RequestText
        {
            get { return _requestText; }
            set
            {
                _requestText = value;
                RaisePropertyChanged();
            }
        }


        private ImageSource _imageSource;
        /// <summary> 说明  </summary>
        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                RaisePropertyChanged();
            }
        }


        private void SelectedSessionChanged()
        {
            if (SelectedSession == null)
            {
                RequestText = null;
                ResponseText = string.Empty;
                ImageSource = null;
                return;
            }

            const int truncateLimit = 1024;

            var session = SelectedSession.HttpClient;
            var request = session.Request;
            var fullData = (request.IsBodyRead ? request.Body : null) ?? Array.Empty<byte>();
            var data = fullData;
            var truncated = data.Length > truncateLimit;
            if (truncated) data = data.Take(truncateLimit).ToArray();

            //string hexStr = string.Join(" ", data.Select(x => x.ToString("X2")));
            var sb = new StringBuilder();
            sb.AppendLine("URI: " + request.RequestUri);
            sb.Append(request.HeaderText);
            sb.Append(request.Encoding.GetString(data));
            if (truncated)
            {
                sb.AppendLine();
                sb.Append($"Data is truncated after {truncateLimit} bytes");
            }

            sb.Append((request as ConnectRequest)?.ClientHelloInfo);
            RequestText = sb.ToString();

            var response = session.Response;
            fullData = (response.IsBodyRead ? response.Body : null) ?? Array.Empty<byte>();
            data = fullData;
            truncated = data.Length > truncateLimit;
            if (truncated) data = data.Take(truncateLimit).ToArray();

            //hexStr = string.Join(" ", data.Select(x => x.ToString("X2")));
            sb = new StringBuilder();
            sb.Append(response.HeaderText);
            sb.Append(response.Encoding.GetString(data));
            if (truncated)
            {
                sb.AppendLine();
                sb.Append($"Data is truncated after {truncateLimit} bytes");
            }

            sb.Append((response as ConnectResponse)?.ServerHelloInfo);
            if (SelectedSession.Exception != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append(SelectedSession.Exception);
            }

            ResponseText = sb.ToString();

            try
            {
                if (fullData.Length > 0)
                    using (var stream = new MemoryStream(fullData))
                    {
                        this.ImageSource =
                            BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
            }
            catch
            {
                this.ImageSource = null;
            }
        }

        //private void ButtonProxyOnOff_OnClick(object sender, RoutedEventArgs e)
        //{
        //    var button = (ToggleButton)sender;
        //    if (button.IsChecked == true)
        //        _proxyServer.SetAsSystemProxy((ExplicitProxyEndPoint)_proxyServer.ProxyEndPoints[0],
        //            ProxyProtocolType.AllHttp);
        //    else
        //        _proxyServer.RestoreOriginalProxySettings();
        //}
    }


}
