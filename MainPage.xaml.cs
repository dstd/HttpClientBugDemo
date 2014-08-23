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
                    using (var response = await req.PostAsync("http://google.com/a", null, Cancellation.Token))
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