using Pm.Common;
using Pm.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pm.Common
{
    public class PmMetricPusher : PmLongTask
    {

        private CollectorRegistry _collectorRegistry;

        private readonly HttpClient _httpClient = new HttpClient();

        private readonly Uri _targetUrl;


        public PmMetricPusher(CollectorRegistry collectorRegistry, string endpoint, string job, string instance = null, int intervalSeconds = 60)
            : base(intervalSeconds)
        {
            Log = new PmLogger("MetricPusher");

            StringBuilder sb = new StringBuilder(string.Format("{0}/job/{1}", endpoint.TrimEnd('/'), job));

            if (!string.IsNullOrEmpty(instance))
            {
                //if (instance.Contains("\\") == true) instance = instance.Replace("\\", "%2f");
                if (instance.Contains("\\") == true) instance = instance.Replace("\\", "_");

                sb.AppendFormat("/instance/{0}", instance);
            }

            if (!Uri.TryCreate(sb.ToString(), UriKind.Absolute, out _targetUrl))
            {
                throw new ArgumentException("Endpoint must be a valid url", "endpoint");
            }

            Log.LogDebug($"targetUri:{_targetUrl}");

            _collectorRegistry = collectorRegistry;
        }

        private StringBuilder _sb = new StringBuilder(2048);


        protected override async Task WorkStep(CancellationToken ct)
        {
            try
            {
                _sb.Clear();
                _collectorRegistry.CollectAndExportAsText(ref _sb, CancelToken);
                var contents = _sb.ToString();

                var httpContent = new StringContent(contents, Encoding.UTF8, "application/plain");

                var response = await _httpClient.PostAsync(_targetUrl, httpContent);

                response.EnsureSuccessStatusCode();

                //race.TraceInformation(contents);
            }
            catch (HttpRequestException rex)
            {
                Log.LogError(rex);
                Trace.WriteLine($"HttpRequestException: {rex.Message}");
            }
            //catch (ScrapeFailedException ex)
            //{
            //    Trace.WriteLine($"Skipping metrics push due to failed scrape: {ex.Message}");
            //}
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Log.LogError(ex);

                Trace.WriteLine(string.Format("Error in MetricPusher: {0}", ex));
            }
        }
    }
}
