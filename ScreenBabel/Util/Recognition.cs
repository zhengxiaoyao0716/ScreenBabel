using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenBabel.Util
{
    public static class Recognition
    {
        private static Action<Action<FrameworkElement>> PostUIThread;
        internal static void Prepare(FrameworkElement element)
        {
            var context = SynchronizationContext.Current;
            PostUIThread = action => context.Post(state => action(element), null);
        }

        private static Component.Output output;
        private static void UpdateResult(object state)
        {
            if (output == null)
            {
                output = new Component.Output();
                output.Closed += (sender, e) => output = null;
            }
            var result = (Dictionary<string, string>)state;
            output.Topmost = true;
            output.Show();
            output.Raw.Text = result["raw"];
            output.Out.Text = result["text"];
        }
        
        internal static async Task Recognize(Bitmap image)
        {
            // TODO 去重复捕捉
            var raw = "";
            var text = "";
            switch (Resources.Setting.Default.RecognitionMode)
            {
                case "YouDao":
                    var result = await Task.Run(() => YouDaoOCR(image));
                    if (result != null)
                    {
                        raw = result[0];
                        text = result[1];
                    }
                    break;
                case "Azure":
                    raw = await AzureOCRRequest(image);
                    if (!String.IsNullOrEmpty(raw))
                    {
                        text = await AzureTranslate(raw);
                    }
                    break;
                case "Tesseract": // TODO 当前训练集识别率太低.
                default:
                    throw new ArgumentException("invalid value of RecognitionMode: " + Resources.Setting.Default.RecognitionMode);
            }
            PostUIThread(ele => UpdateResult(new Dictionary<string, string> { { "raw", raw }, { "text", text } }));
        }

        #region YouDao

        private static string[] YouDaoOCR(Bitmap image)
        {
            var appKey = Resources.Setting.Default.YouDaoAppKey;
            var appSecret = Resources.Setting.Default.YouDaoAppSecret;
            var ocrApi = Resources.Setting.Default.YouDaoOCRApi;
            if (String.IsNullOrEmpty(appKey) || String.IsNullOrEmpty(appSecret) || String.IsNullOrEmpty(ocrApi))
            {
                PostUIThread(element => MessageBox.Show(String.Format(
                    (string)element.TryFindResource("Text.Tip.InvalidRecognitionMode"),
                    (string)element.TryFindResource("Text.Setting.YouDao")
                )));
                return null;
            }

            var bytes = ImgToBase64String(image);
            var type = "1";
            var salt = DateTime.Now.Millisecond.ToString();
            MD5 md5 = new MD5CryptoServiceProvider();
            string md5Str = appKey + bytes + salt + appSecret;
            byte[] output = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(md5Str));
            string sign = BitConverter.ToString(output).Replace("-", "");

            var dic = new Dictionary<string, string>
            {
                { "type", type.ToString() },
                { "appKey", appKey },
                { "salt", salt.ToString() },
                { "sign", sign },
                { "q", bytes },
            };

            var resp = JObject.Parse(Post(ocrApi, dic));
            if ((int)resp["errorCode"] != 0)
            {
                PostUIThread(element => MessageBox.Show((string)resp.ToString()));
                return null;
            }

            var regions = resp["resRegions"];
            var raw = String.Join("\n", regions.Select(region => (string)region["context"]).ToList());
            var text = String.Join("\n", regions.Select(region => (string)region["tranContent"]).ToList());
            return new string[] { raw, text };
        }
        private static string ImgToBase64String(Bitmap image)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string Post(string url, Dictionary<string, string> dic)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            #region 添加Post 参数
            StringBuilder builder = new StringBuilder();
            foreach (var item in dic)
            {
                builder.AppendFormat("&{0}={1}", item.Key, WebUtility.UrlEncode(item.Value));
            }
            // Console.WriteLine(builder.ToString());
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        //private static string YouDaoTranslate(string q)
        //{
        //    string from = "ja";
        //    string to = "zh-CHS";
        //    string salt = DateTime.Now.Millisecond.ToString();
        //    MD5 md5 = new MD5CryptoServiceProvider();
        //    string md5Str = appKey + q + salt + appSecret;
        //    byte[] output = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(md5Str));
        //    string sign = BitConverter.ToString(output).Replace("-", "");

        //    string url = string.Format(
        //        "http://openapi.youdao.com/api?appKey={0}&q={1}&from={2}&to={3}&sign={4}&salt={5}",
        //        appKey, System.Web.HttpUtility.UrlDecode(q, System.Text.Encoding.GetEncoding("UTF-8")), from, to, sign, salt
        //    );
        //    WebRequest translationWebRequest = WebRequest.Create(url);

        //    WebResponse response = null;

        //    response = translationWebRequest.GetResponse();
        //    Stream stream = response.GetResponseStream();

        //    Encoding encode = Encoding.GetEncoding("utf-8");

        //    StreamReader reader = new StreamReader(stream, encode);
        //    var respStr = reader.ReadToEnd();
        //    var resp = JObject.Parse(respStr);
        //    if ((int)resp["errorCode"] != 0)
        //    {
        //        return respStr;
        //    }
        //    return String.Join("\n", resp["translation"].Select(text => (string)text).ToList());
        //}

        #endregion

        #region deprecated.Azure
        
        private static async Task<string> AzureOCRRequest(Image image)
        {
            // https://docs.microsoft.com/zh-cn/azure/cognitive-services/Computer-vision/quickstarts/csharp-print-text

            var ocrKey = Resources.Setting.Default.AzureOCRKey;
            var ocrApi = Resources.Setting.Default.AzureOCRApi;
            if (String.IsNullOrEmpty(ocrKey) || String.IsNullOrEmpty(ocrApi))
            {
                PostUIThread(element => MessageBox.Show(String.Format(
                    (string)element.TryFindResource("Text.Tip.InvalidRecognitionMode"),
                    (string)element.TryFindResource("Text.Setting.Azure")
                )));
                return "";
            }

            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ocrKey);

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = ImageToBytes(image);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Make the REST API call.
                response = await client.PostAsync(ocrApi, content);
            }

            // Get the JSON response.
            string contentString = await response.Content.ReadAsStringAsync();

            // Display the JSON response.
            var resp = JObject.Parse(contentString);

            if (resp["regions"] == null)
            {
                return (string)resp["message"];
            }
            var regions = resp["regions"].SelectMany(region =>
                region["lines"].Select(line =>
                    String.Join("", line["words"].Select(word => (string)word["text"]).ToList())
                ).ToList()
            ).ToList();
            return String.Join("\n", regions);
        }
        private static byte[] ImageToBytes(Image image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Position = 0;

                var bytes = new Byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                return bytes;
            }
        }

        private static async Task<string> AzureTranslate(string text)
        {
            // https://docs.microsoft.com/zh-cn/azure/cognitive-services/translator/quickstart-csharp-translate

            var transKey = Resources.Setting.Default.AzureTransKey;
            var transApi = Resources.Setting.Default.AzureTransApi;
            if (String.IsNullOrEmpty(transKey) || String.IsNullOrEmpty(transApi))
            {
                var promise = new TaskCompletionSource<string>();
                PostUIThread(element => {
                    var message = String.Format(
                        (string)element.TryFindResource("Text.Tip.InvalidRecognitionMode"),
                        (string)element.TryFindResource("Text.Setting.Azure")
                    );
                    promise.TrySetResult(message);
                    //MessageBox.Show(message);
                });
                return await promise.Task;
            }

            System.Object[] body = new System.Object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(transApi);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", transKey);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                var resp = JObject.Parse(responseBody);

                if (resp["translations"] == null)
                {
                    return (string)resp["message"];
                }
                var translations = resp["translations"].Select(translation => (string)translation["text"]).ToList();
                return String.Join("\n", translations);
            }
        }

        #endregion

        #region Tesseract

        //private static async Task<string> RecognizeOffline(Image image)
        //{
        //    return await Task.Run(() =>
        //    {
        //        var engine = new TesseractEngine(@"./tessdata", "jpn", EngineMode.Default);
        //        var page = engine.Process(new Bitmap(image));
        //        return page.GetText();
        //    });
        //}

        #endregion
    }
}
