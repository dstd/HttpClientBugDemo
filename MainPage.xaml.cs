#define USE_HTTPCLIENT
//#define USE_HTTP_WEB_REQUEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HttpBugDemo.Resources;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HttpBugDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

#if USE_HTTPCLIENT
        public static CancellationTokenSource Cancellation = new CancellationTokenSource();
        //public static HttpClient Request = null;

        public HttpMessageHandler CreateClientHandler()
        {
            return new HttpClientHandler()
            {
                //AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            };
        }

        public HttpClient CreateApiClient(Uri baseUri)
        {
            var req = new HttpClient(this.CreateClientHandler()) { BaseAddress = baseUri };
            return req;
        }

        public async Task<string> ExecuteRequest()
        {
            try
            {
                using (var req = CreateApiClient(null))
                {
                    //Request = req;
                    var requestContent = new StringContent("{\"token\":\"xxxxxxxxxx\"}");
                    using (var response = await req.PostAsync("http://api.example.com:8080/polling", requestContent, Cancellation.Token))
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        //Request = null;
                        return json;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return "[cancelled]";
            }
        }

        public static void CancelRequests()
        {
            Cancellation.Cancel();
            //if (Request != null)
            //    Request.CancelPendingRequests();
        }

        public async void RunLongPolling()
        {
            while (!Cancellation.IsCancellationRequested)
            {
                var s = await ExecuteRequest();
                Debug.WriteLine("[response] {0}", s);
            }
        }
#elif USE_HTTP_WEB_REQUEST
        private static bool _cancelled = false;
        private static HttpWebRequest _request = null;
        private static byte[] _buffer = new byte[1024];
        private static Stream _stream = null;
        private static StringBuilder _result = new StringBuilder();

        private void LongPollingReadCallback(IAsyncResult asyncResult)
        {
            try
            {
                int read = _stream.EndRead(asyncResult);

                if (read > 0)
                {
                    if (_cancelled)
                        return;
                    _result.Append(Encoding.UTF8.GetString(_buffer, 0, read));
                    IAsyncResult asynchronousResult = _stream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(LongPollingReadCallback), this);
                }
                else
                {
                    if (_result.Length > 1)
                    {
                        // handle
                    }

                    _stream.Close();

                    RunLongPolling();
                }
            }
            catch (WebException)
            {
                RunLongPolling();
            }
        }

        public static void CancelRequests()
        {
            _cancelled = true;
            if (_request != null)
            {
                _request.Abort();
            }
        }

        public void RunLongPolling()
        {
            if (_cancelled)
                return;

            _request = WebRequest.CreateHttp("http://api.example.com:8080/polling");
            _request.Method = "POST";
            _request.ContentType = "application/json";
            _request.AllowReadStreamBuffering = false;
            _request.AllowAutoRedirect = false;

            _request.BeginGetRequestStream(asyncResult =>
            {
                var postStream = _request.EndGetRequestStream(asyncResult);
                byte[] byteArray = Encoding.UTF8.GetBytes("{\"token\":\"xxxxxxxxxx\"}");
                postStream.Write(byteArray, 0, byteArray.Length);
                postStream.Close();

                _request.BeginGetResponse(asyncResponse =>
                {
                    try
                    {
                        if (_cancelled)
                            return;

                        var someResponse = (WebResponse)_request.EndGetResponse(asyncResponse);
                        _stream = someResponse.GetResponseStream();

                        _stream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(LongPollingReadCallback), this);
                    }
                    catch (WebException)
                    {
                        if (!_cancelled)
                            RunLongPolling();
                    }

                }, null);
            }, null);
        }
#endif

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            RunLongPolling();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainPage.CancelRequests();
        }
    }
}