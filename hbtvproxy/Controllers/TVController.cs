using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace hbtvproxy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TVController : Controller
    {
        [HttpGet]
        [Route("getStreamUrl/{id}")]
        public async Task Get(string id)
        {
            string url="http://app.cjyun.org/video/player/playbill";
            string streamUrl= await get(url,id);
            HttpContext.Response.Redirect(streamUrl);
        }

        private async Task<string> get(string url,string id)
        {
            string result = string.Empty;

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                CookieContainer = cookies,
                UseCookies = true
            };


            using (var client = new System.Net.Http.HttpClient(handler))
            {
                var builder = new UriBuilder(url);
                builder.Port = -1;
                var query = HttpUtility.ParseQueryString(builder.Query);
                query["stream_id"] = id;
                query["site_id"] = "10008";
                builder.Query = query.ToString();
                string url_1 = builder.ToString();

                using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url_1)))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                    request.Headers.TryAddWithoutValidation("Accept-Language", "zh-CN");
                    request.Headers.TryAddWithoutValidation("Connection", "Keep-Alive");
                    request.Headers.TryAddWithoutValidation("DNT", "1");
                    request.Headers.TryAddWithoutValidation("DNT", "1");
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko");                    

                    using (var response = await client.SendAsync(request).ConfigureAwait(false))
                    {
                        using (Stream stream = await response.Content.ReadAsStreamAsync())
                        using (Stream decompressed = new GZipStream(stream, CompressionMode.Decompress))
                        using (StreamReader reader = new StreamReader(decompressed))
                        {
                            string jsonStr = reader.ReadToEnd().Trim().Replace(System.Environment.NewLine, "");
                            JObject jObj = JsonConvert.DeserializeObject<JObject>(jsonStr);
                            try
                            {
                                result = jObj["stream"].ToString();
                            }
                            catch
                            {
                                 string url_2="http://app.cjyun.org/video/player/stream";
                                 result=await get(url_2,id);
                            }

                        }                        
                    }
                }
            }
            return result;
        }



    }
}