using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Web;

namespace Kybs0.Net
{
    public class WebRequestBase
    {
        #region Request

        public virtual async Task<TResponse> GetAsync<TResponse>(string url, HttpRequest request = null, Dictionary<string, string> headersDict = null)
        {
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Method = "get";
            webRequest.ContentType = "application/json";
            if (headersDict != null)
            {
                foreach (var headerTuple in headersDict)
                {
                    webRequest.Headers.Add(headerTuple.Key, headerTuple.Value);
                }
            }

            if (request!=null)
            {
                var jsonData = JsonConvert.SerializeObject(request);
                byte[] databyte = Encoding.UTF8.GetBytes(jsonData);
                webRequest.ContentLength = databyte.Length;
                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    requestStream.Write(databyte, 0, databyte.Length);
                }
            }

            var response = await webRequest.GetResponseAsync();
            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(responseStream ?? throw new InvalidOperationException(),
                    Encoding.GetEncoding("utf-8")))
                {
                    string result = reader.ReadToEnd();
                    var decodeResult = UnicodeHelper.Unicode2String(result);
                    var dataResponse = JsonConvert.DeserializeObject<TResponse>(decodeResult);
                    return dataResponse;
                }
            }
        }

        #endregion

        #region Post
        public virtual async Task<TReponse> PostAsync<TReponse>(string url, HttpRequest request, Dictionary<string, string> headersDict = null)
        {
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Method = "post";
            webRequest.ContentType = "application/json;charset=utf-8";
            if (headersDict != null)
            {
                foreach (var headerTuple in headersDict)
                {
                    webRequest.Headers.Add(headerTuple.Key, headerTuple.Value);
                }
            }

            var jsonData = JsonConvert.SerializeObject(request);
            byte[] postdatabyte = Encoding.UTF8.GetBytes(jsonData);
            webRequest.ContentLength = postdatabyte.Length;
            using (Stream postStream = webRequest.GetRequestStream())
            {
                postStream.Write(postdatabyte, 0, postdatabyte.Length);
            }
            var response = await webRequest.GetResponseAsync();

            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(responseStream ?? throw new InvalidOperationException(),
                    Encoding.GetEncoding("utf-8")))
                {
                    string result = reader.ReadToEnd();
                    var decodeResult = UnicodeHelper.Unicode2String(result);
                    var dataResponse = JsonConvert.DeserializeObject<TReponse>(decodeResult);
                    return dataResponse;
                }
            }
        }

        /// <summary>
        /// Post using HttpClient
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual async Task<TReponse> PostByHttpAsync<TReponse, TRequest>(string requestUrl, HttpRequest request, Dictionary<string, string> headersDict = null)
        {
            var jsonData = JsonConvert.SerializeObject(request);
            HttpContent httpContent = new StringContent(jsonData);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (headersDict != null)
            {
                foreach (var headerTuple in headersDict)
                {
                    httpContent.Headers.Add(headerTuple.Key, headerTuple.Value);
                }
            }

            using (HttpClient httpClient = new HttpClient())
            {
                using (HttpResponseMessage response = httpClient.PostAsync(requestUrl, httpContent).Result)
                {
                    if (response.IsSuccessStatusCode && response.Content != null)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var dataResponse = JsonConvert.DeserializeObject<TReponse>(result);
                        return dataResponse;
                    }
                }
            }
            return default(TReponse);
        }

        #endregion
    }
}
