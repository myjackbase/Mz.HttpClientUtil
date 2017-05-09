using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Mz.HttpClientUtil
{
    public class HttpMessageBuilder
    {
        private static HttpClient _defaultClient = new HttpClient();

        private readonly HttpRequestMessage _message;
        private readonly Dictionary<string, object> _query;
        private readonly Dictionary<string, object> _uriSegments;
        private AuthenticationHeaderValue _authentication;
        private Uri _baseUri;
        private string _resourceUri;

        public HttpMessageBuilder(HttpRequestMessage message = null)
        {
            _message = message ?? new HttpRequestMessage();
            _query = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            _uriSegments = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
        }

        public HttpRequestMessage RequestMessage
        {
            get
            {
                return _message;
            }
        }

        #region Uri Setting
        public HttpMessageBuilder SetUri(Uri uri)
        {
            this.RequestMessage.RequestUri = uri;
            return this;
        }

        public HttpMessageBuilder SetUri(string uri)
        {
            return this.SetUri(new Uri(uri));
        }

        public HttpMessageBuilder SetBaseUri(Uri baseUri)
        {
            this._baseUri = baseUri;
            return this;
        }

        public HttpMessageBuilder SetBaseUri(string baseUri)
        {
            return SetBaseUri(new Uri(baseUri));
        }

        public HttpMessageBuilder SetResourceUri(string resourceUri)
        {
            this._resourceUri = resourceUri;
            return this;
        }
        #endregion

        #region Header Setting
        public HttpMessageBuilder AddHeader(string key, object value)
        {
            if (!string.IsNullOrEmpty(key) && value != null)
            {
                this.RequestMessage.Headers.Add(key, value.ToString());
            }

            return this;
        }

        public HttpMessageBuilder AddHeader(string headers)
        {
            foreach (var header in headers.Split(','))
            {
                var headerObj = header.Split(':');
                if (headerObj.Length == 2) AddHeader(headerObj[0], headerObj[1]);
            }

            return this;
        }

        public HttpMessageBuilder AddHeaders(IEnumerable<KeyValuePair<string, object>> headers)
        {
            if (headers == null)
            {
                return this;
            }

            foreach (var keyValuePair in headers)
            {
                this.AddHeader(keyValuePair.Key, keyValuePair.Value);
            }

            return this;
        }
        #endregion

        #region Method Setting
        public HttpMessageBuilder SetMethod(HttpMethod method)
        {
            this.RequestMessage.Method = method;
            return this;
        }
        #endregion

        #region Query Setting
        public HttpMessageBuilder AddUriSegment(string key, object value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return this;
            }

            this._uriSegments.Add(key, value);
            return this;
        }

        public HttpMessageBuilder AddQuery(string key, object value)
        {
            if (!string.IsNullOrEmpty(key) && value != null)
            {
                this._query[key] = value;
            }

            return this;
        }

        public HttpMessageBuilder AddQuery(IEnumerable<KeyValuePair<string, object>> queries)
        {
            if (queries == null)
            {
                return this;
            }

            foreach (var keyValuePair in queries)
            {
                this.AddQuery(keyValuePair.Key, keyValuePair.Value);
            }

            return this;
        }
        #endregion

        #region Auth Setting
        public HttpMessageBuilder SetAuthentication(string scheme, string parameter)
        {
            this._authentication = new AuthenticationHeaderValue(scheme, parameter);
            return this;
        }
        #endregion

        #region Content Setting
        public HttpMessageBuilder SetContent(HttpContent content)
        {
            this.RequestMessage.Content = content;
            return this;
        }

        public HttpMessageBuilder SetJsonContent<T>(T objToSerialize, JsonSerializerSettings serializerSettings = null)
        {
            return this.SetContent(typeof(T) == typeof(string) ? new JsonContent(objToSerialize) : new JsonContent(objToSerialize, serializerSettings));
        }
        #endregion

        public HttpRequestMessage ToHttpRequestMessage()
        {
            Uri uri = null; // Fill back to message's uri.
            if (this._baseUri == null && this._resourceUri == null)
            {
                uri = this._message.RequestUri;
                if (uri == null)
                {
                    throw new ArgumentException("No URI configured on the request.");
                }
            }

            if (uri == null)
            {
                // If no base URL has been set, just use the Resource URI and assume it's complete.
                uri = this._baseUri == null ? new Uri(this._resourceUri) : new Uri(this._baseUri, this._resourceUri);
            }

            // Add URL segments.
            if (this._uriSegments.Any())
            {
                foreach (var uriSegment in this._uriSegments)
                {
                    var strUri = uri.ToString();
                    strUri = strUri.Replace(
                        string.Format("{{{0}}}", System.Uri.EscapeDataString(uriSegment.Key)),
                        System.Uri.EscapeDataString(uriSegment.Value.ToString()));
                    uri = new Uri(strUri);
                }
            }

            // Add query string.
            if (this._query.Any())
            {
                var queryStringBuilder = new StringBuilder();
                queryStringBuilder.Append('?');
                var queryList = this._query.ToList();
                for (int i = 0; i < queryList.Count; i++)
                {
                    var q = queryList[i];
                    queryStringBuilder.Append(System.Uri.EscapeDataString(q.Key));
                    queryStringBuilder.Append('=');
                    queryStringBuilder.Append(System.Uri.EscapeDataString(q.Value.ToString()));

                    // If this is not the last query string segment, add the delimiter.
                    if (i != queryList.Count - 1)
                    {
                        queryStringBuilder.Append('&');
                    }
                }

                var combined = UrlHelper.Combine(uri.ToString(), queryStringBuilder.ToString());
                uri = new Uri(combined, UriKind.RelativeOrAbsolute);
            }

            // Add authentication header value.
            if (this._authentication != null)
            {
                this._message.Headers.Authorization = this._authentication;
            }

            this._message.RequestUri = uri;
            return this._message;
        }

        public Task<HttpResponseMessage> SendAsync(HttpClient client = null)
        {
            return (client ?? _defaultClient).SendAsync(this.ToHttpRequestMessage());
        }

        public event OnRequestDelegate OnRequest;
        public delegate void OnRequestDelegate(HttpRequestMessage request, ReqContext reqContext);

        public event OnResponseDelegate OnResponse;
        public delegate void OnResponseDelegate(HttpResponseMessage response, ReqContext reqContext);

        public event OnErrorDelegate OnError;
        public delegate void OnErrorDelegate(Exception origEx, HttpRequestMessage request, ReqContext reqContext);
    }
}
