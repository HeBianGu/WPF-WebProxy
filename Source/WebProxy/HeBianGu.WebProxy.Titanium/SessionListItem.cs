using HeBianGu.Base.WpfBase;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;

namespace HeBianGu.WebProxy.Titanium
{
    public class SessionListItem : INotifyPropertyChanged, ISelectViewModel
    {
        private long? bodySize;
        private Guid clientConnectionId;
        private Exception exception;
        private string host;
        private int processId;
        private string protocol;
        private string method;
        private long receivedDataCount;
        private long sentDataCount;
        private Guid serverConnectionId;
        private string statusCode;
        private string url;

        //public int Number { get; set; }

        private bool _isSelected;
        [XmlIgnore]
        [Browsable(false)]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        [Display(Name = "Client Id")]
        public Guid ClientConnectionId
        {
            get => clientConnectionId;
            set => SetField(ref clientConnectionId, value);
        }
        [Display(Name = "Server Id")]
        public Guid ServerConnectionId
        {
            get => serverConnectionId;
            set => SetField(ref serverConnectionId, value);
        }

        [XmlIgnore]
        [Browsable(false)]
        public HttpWebClient HttpClient { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public IPEndPoint ClientLocalEndPoint { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public IPEndPoint ClientRemoteEndPoint { get; set; }

        [Display(Name = "Protocol")]
        public string Protocol
        {
            get => protocol;
            set => SetField(ref protocol, value);
        }

        [Display(Name = "Host")]
        public string Host
        {
            get => host;
            set => SetField(ref host, value);
        }
        [Display(Name = "Url")]
        public string Url
        {
            get => url;
            set => SetField(ref url, value);
        }


        [Display(Name = "Method")]
        public string Method
        {
            get => method;
            set => SetField(ref method, value);
        }


        [Display(Name = "Process")]
        public string Process
        {
            get
            {
                try
                {
                    var process = System.Diagnostics.Process.GetProcessById(processId);
                    return process.ProcessName + ":" + processId;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }
        [Display(Name = "Status Code")]
        public string StatusCode
        {
            get => statusCode;
            set => SetField(ref statusCode, value);
        }

        [Display(Name = "IsTunnel")]
        public bool IsTunnelConnect { get; set; }


        [Display(Name = "Body Size")]
        public long? BodySize
        {
            get => bodySize;
            set => SetField(ref bodySize, value);
        }
        [Display(Name = "Process Id")]
        public int ProcessId
        {
            get => processId;
            set
            {
                if (SetField(ref processId, value)) OnPropertyChanged(nameof(Process));
            }
        }
        [Display(Name = "ReceivedDataCount")]
        public long ReceivedDataCount
        {
            get => receivedDataCount;
            set => SetField(ref receivedDataCount, value);
        }

        [Display(Name = "SentDataCount")]
        public long SentDataCount
        {
            get => sentDataCount;
            set => SetField(ref sentDataCount, value);
        }

        [XmlIgnore]
        [Browsable(false)]
        public Exception Exception
        {
            get => exception;
            set => SetField(ref exception, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Update(SessionEventArgsBase args)
        {
            var request = HttpClient.Request;
            var response = HttpClient.Response;
            var statusCode = response?.StatusCode ?? 0;
            StatusCode = statusCode == 0 ? "-" : statusCode.ToString();
            Protocol = request.HttpVersion.Major == 2 ? "http2" : request.RequestUri.Scheme;
            ClientConnectionId = args.ClientConnectionId;
            ServerConnectionId = args.ServerConnectionId;

            if (IsTunnelConnect)
            {
                Host = "Tunnel to";
                Url = request.RequestUri.Host + ":" + request.RequestUri.Port;
            }
            else
            {
                Host = request.RequestUri.Host;
                Url = request.RequestUri.AbsolutePath;
            }

            if (!IsTunnelConnect)
            {
                long responseSize = -1;
                if (response != null)
                {
                    if (response.ContentLength != -1)
                        responseSize = response.ContentLength;
                    else if (response.IsBodyRead && response.Body != null) responseSize = response.Body.Length;
                }

                BodySize = responseSize;
            }

            ProcessId = HttpClient.ProcessId.Value;
            this.Method = this.HttpClient.Request.Method;
            //if (this.HttpClient.Response.IsBodyRead)
            //    System.Diagnostics.Debug.WriteLine(this.HttpClient.Response.BodyString);
        }
    }

}