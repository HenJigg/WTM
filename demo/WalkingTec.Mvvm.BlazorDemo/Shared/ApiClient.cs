using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BootstrapBlazor.Components;
using Microsoft.Extensions.Hosting;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Core.Extensions;
using WalkingTec.Mvvm.Core.Json;

namespace WalkingTec.Mvvm.BlazorDemo.Shared
{
    public class ApiClient
    {
        public HttpClient Client { get; }

        public ApiClient(HttpClient client)
        {
            Client = client;
        }

        #region CallApi
        public async Task<ApiResult<T>> CallAPI<T>(string url, HttpMethodEnum method, HttpContent content, int? timeout = null, string proxy = null) where T : class
        {
            ApiResult<T> rv = new ApiResult<T>();
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return rv;
                }
                url = url.AppendQuery("culture=" + System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                //如果配置了代理，则使用代理
                //设置超时
                if (timeout.HasValue)
                {
                    Client.Timeout = new TimeSpan(0, 0, 0, timeout.Value, 0);
                }
                //填充表单数据
                HttpResponseMessage res = null;
                switch (method)
                {
                    case HttpMethodEnum.GET:
                        res = await Client.GetAsync(url);
                        break;
                    case HttpMethodEnum.POST:
                        res = await Client.PostAsync(url, content);
                        break;
                    case HttpMethodEnum.PUT:
                        res = await Client.PutAsync(url, content);
                        break;
                    case HttpMethodEnum.DELETE:
                        res = await Client.DeleteAsync(url);
                        break;
                    default:
                        break;
                }
                if (res == null)
                {
                    return rv;
                }
                rv.StatusCode = res.StatusCode;
                JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
                jsonOptions.PropertyNamingPolicy = null;
                jsonOptions.IgnoreNullValues = true;
                jsonOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                jsonOptions.AllowTrailingCommas = true;
                jsonOptions.Converters.Add(new StringIgnoreLTGTConverter());
                jsonOptions.Converters.Add(new JsonStringEnumConverter());
                jsonOptions.Converters.Add(new DateRangeConverter());
                jsonOptions.Converters.Add(new PocoConverter());
                jsonOptions.Converters.Add(new TypeConverter());
                jsonOptions.Converters.Add(new DynamicDataConverter());
                if (res.IsSuccessStatusCode == true)
                {
                    Type dt = typeof(T);
                    if (dt == typeof(byte[]))
                    {
                        rv.Data = await res.Content.ReadAsByteArrayAsync() as T;
                    }
                    else
                    {
                        string responseTxt = await res.Content.ReadAsStringAsync();
                        if (dt == typeof(string))
                        {
                            rv.Data = responseTxt as T;
                        }
                        else
                        {
                            rv.Data = JsonSerializer.Deserialize<T>(responseTxt,jsonOptions);
                        }
                    }
                }
                else
                {
                    string responseTxt = await res.Content.ReadAsStringAsync();
                    if (res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        try
                        {
                            rv.Errors = JsonSerializer.Deserialize<ErrorObj>(responseTxt,jsonOptions);
                        }
                        catch { }
                    }
                    rv.ErrorMsg = responseTxt;
                }

                return rv;
            }
            catch (Exception ex)
            {
                rv.ErrorMsg = ex.ToString();
                return rv;
            }
        }

        /// <summary>
        /// 使用Get方法调用api
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="domainName">Appsettings中配置的Domain key</param>
        /// <param name="url">调用地址</param>
        /// <param name="timeout">超时时间，单位秒</param>
        /// <param name="proxy">代理地址</param>
        /// <returns></returns>
        public async Task<ApiResult<T>> CallAPI<T>(string url, int? timeout = null, string proxy = null) where T : class
        {
            HttpContent content = null;
            //填充表单数据
            return await CallAPI<T>( url, HttpMethodEnum.GET, content, timeout, proxy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="domainName">Appsettings中配置的Domain key</param>
        /// <param name="url">调用地址</param>
        /// <param name="method">调用方式</param>
        /// <param name="postdata">提交字段</param>
        /// <param name="timeout">超时时间，单位秒</param>
        /// <param name="proxy">代理地址</param>
        /// <returns></returns>
        public async Task<ApiResult<T>> CallAPI<T>( string url, HttpMethodEnum method, IDictionary<string, string> postdata, int? timeout = null, string proxy = null) where T : class
        {
            HttpContent content = null;
            //填充表单数据
            if (!(postdata == null || postdata.Count == 0))
            {
                List<KeyValuePair<string, string>> paras = new List<KeyValuePair<string, string>>();
                foreach (string key in postdata.Keys)
                {
                    paras.Add(new KeyValuePair<string, string>(key, postdata[key]));
                }
                content = new FormUrlEncodedContent(paras);
            }
            return await CallAPI<T>(url, method, content, timeout, proxy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="domainName">Appsettings中配置的Domain key</param>
        /// <param name="url">调用地址</param>
        /// <param name="method">调用方式</param>
        /// <param name="postdata">提交的object，会被转成json提交</param>
        /// <param name="timeout">超时时间，单位秒</param>
        /// <param name="proxy">代理地址</param>
        /// <returns></returns>
        public async Task<ApiResult<T>> CallAPI<T>(string url, HttpMethodEnum method, object postdata, int? timeout = null, string proxy = null) where T : class
        {
            HttpContent content = new StringContent(JsonSerializer.Serialize(postdata), System.Text.Encoding.UTF8, "application/json");
            return await CallAPI<T>(url, method, content, timeout, proxy);
        }

        public async Task<ApiResult<string>> CallAPI( string url, HttpMethodEnum method, HttpContent content, int? timeout = null, string proxy = null)
        {
            return await CallAPI<string>(url, method, content, timeout, proxy);
        }

        /// <summary>
        /// 使用Get方法调用api
        /// </summary>
        /// <param name="domainName">Appsettings中配置的Domain key</param>
        /// <param name="url">调用地址</param>
        /// <param name="timeout">超时时间，单位秒</param>
        /// <param name="proxy">代理地址</param>
        /// <returns></returns>
        public async Task<ApiResult<string>> CallAPI( string url, int? timeout = null, string proxy = null)
        {
            return await CallAPI<string>( url, timeout, proxy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domainName">Appsettings中配置的Domain key</param>
        /// <param name="url">调用地址</param>
        /// <param name="method">调用方式</param>
        /// <param name="postdata">提交字段</param>
        /// <param name="timeout">超时时间，单位秒</param>
        /// <param name="proxy">代理地址</param>
        /// <returns></returns>
        public async Task<ApiResult<string>> CallAPI( string url, HttpMethodEnum method, IDictionary<string, string> postdata, int? timeout = null, string proxy = null)
        {
            return await CallAPI<string>(url, method, postdata, timeout, proxy);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domainName">Appsettings中配置的Domain key</param>
        /// <param name="url">调用地址</param>
        /// <param name="method">调用方式</param>
        /// <param name="postdata">提交的object，会被转成json提交</param>
        /// <param name="timeout">超时时间，单位秒</param>
        /// <param name="proxy">代理地址</param>
        /// <returns></returns>
        public async Task<ApiResult<string>> CallAPI( string url, HttpMethodEnum method, object postdata, int? timeout = null, string proxy = null)
        {
            return await CallAPI<string>(url, method, postdata, timeout, proxy);
        }

        #endregion

        public async Task<QueryData<T>> CallSearchApi<T>(string url, BaseSearcher searcher, QueryPageOptions options) where T : class, new()
        {
            if (string.IsNullOrEmpty(options.SortName) && options.SortOrder != SortOrder.Unset)
            {
                searcher.SortInfo = new SortInfo
                {
                    Property = options.SortName,
                    Direction = options.SortOrder == SortOrder.Desc ? SortDir.Desc : SortDir.Asc
                };
            }
            else
            {
                searcher.SortInfo = null;
            }
            var rv = await CallAPI<WtmApiResult<T>>(url, HttpMethodEnum.POST, searcher);
            QueryData<T> data = new QueryData<T>();
            data.Items = rv.Data?.Data;
            data.TotalCount = rv.Data?.Count ?? 0;
            return data;

        }

        public async Task<List<SelectedItem>> CallItemsApi(string url, HttpMethodEnum method = HttpMethodEnum.GET, object postdata=null, int? timeout = null, string proxy = null)
        {
            var result = await CallAPI<List<ComboSelectListItem>>(url, method, postdata, timeout, proxy);
            List<SelectedItem> rv = new List<SelectedItem>();
            if(result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (result.Data != null)
                {
                    foreach (var item in result.Data)
                    {
                        rv.Add(new SelectedItem
                        {
                            Text = item.Text,
                            Value = item.Value.ToString(),
                            Active = item.Selected
                        });
                    }
                }
            }
            return rv;
        }

    }

    public class WtmApiResult<T>
    {
        public List<T> Data { get; set; }
        public int Count { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
        public string Msg { get; set; }
        public int Code { get; set; }
    }
}
