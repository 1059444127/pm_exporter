using Pm.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Common
{
    public class PmMetricServer: PmLongTask
    {
        private HttpListener _httpListener;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private Task _pushGatewayTask;


        public PmMetricServer(string uriHttpListener)
        {
            Log = new PmLogger("MetricPusher");

            _httpListener = new HttpListener();
            //_httpListener.Prefixes.Add($"http://localhost:7000/");
            _httpListener.Prefixes.Add(uriHttpListener);
        }

        public override async Task Start()
        {
            
            _httpListener.Start();

            // Create a fake PushGateway on a background thread, to receive the data genertaed by MetricPusher.
            _pushGatewayTask = Task.Factory.StartNew(delegate
            {
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        // There is no way to give a CancellationToken to GCA() so, we need to hack around it a bit.
                        var getContext = _httpListener.GetContextAsync();
                        getContext.Wait(_cts.Token);
                        var context = getContext.Result;
                        var request = context.Request;
                        var response = context.Response;

                        try
                        {
                            PrintRequestDetails(request.Url);

                            string body;
                            using (var reader = new StreamReader(request.InputStream))
                            {
                                body = reader.ReadToEnd();
                            }

                            if (string.IsNullOrEmpty(body))
                            {
                                Console.WriteLine("Got empty document from pusher. This can be normal if nothing is pushed yet.");
                                Trace.TraceInformation("Got empty document from pusher. This can be normal if nothing is pushed yet.");
                            }
                            else
                            {
                                Console.WriteLine(body);
                                Trace.TraceInformation(body);
                            }

                            response.StatusCode = 204;
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            Console.WriteLine(string.Format("Error in fake PushGateway: {0}", ex));
                        }
                        finally
                        {
                            response.Close();
                        }
                    }
                }
                finally
                {
                    _httpListener.Stop();
                    _httpListener.Close();
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            _cts.Cancel();

            try
            {
                _pushGatewayTask.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void PrintRequestDetails(Uri requestUrl)
        {
            var segments = requestUrl.Segments;
            int idx;
            if (segments.Length < 2 || (idx = Array.IndexOf(segments, "job/")) < 0)
            {
                Console.WriteLine("# Unexpected label information");
                Trace.TraceInformation("# Unexpected label information");
                return;
            }
            StringBuilder sb = new StringBuilder("#");
            for (int i = idx; i < segments.Length; i++)
            {
                if (i == segments.Length - 1)
                {
                    continue;
                }
                sb.AppendFormat(" {0}: {1} |", segments[i].TrimEnd('/'), segments[++i].TrimEnd('/'));
            }
            Console.WriteLine(sb.ToString().TrimEnd('|'));
            Trace.TraceInformation(sb.ToString());
        }
    }
}

