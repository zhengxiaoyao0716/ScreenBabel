using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenBabel.Util
{
    public static class Recognition
    {
        // TODO configuration.
        private static readonly JObject configJSON = JObject.Parse(File.ReadAllText("./config.json"));
        private static readonly string appKey = (string) configJSON["appKey"];
        private static readonly string appSecret = (string)configJSON["appSecret"];
        private static readonly string ocrUrl = (string)configJSON["ocrUrl"];

        private static Component.Output output;
        private static SynchronizationContext context;
        internal static void Prepare()
        {
            context = SynchronizationContext.Current;
        }
        private static void UpdateResult(object state)
        {
            if (output == null)
            {
                output = new Component.Output();
                output.Closed += (object sender, EventArgs e) => output = null;
            }
            var result = (Dictionary<string, string>)state;
            output.Topmost = true;
            output.Show();
            output.Raw.Text = result["raw"];
            output.Out.Text = result["text"];
        }

        //private static async Task<string> RecognizeOffline(Image image)
        //{
        //    return await Task.Run(() =>
        //    {
        //        var engine = new TesseractEngine(@"./tessdata", "jpn", EngineMode.Default);
        //        var page = engine.Process(new Bitmap(image));
        //        return page.GetText();
        //    });
        //}

        //private static async Task<string> RecognizeOnline(Image image)
        //{
        //    var resp = await AzureOCRRequest(image);
        //    if (resp["regions"] == null)
        //    {
        //        return (string)resp["message"];
        //    }
        //    var regions = resp["regions"].SelectMany(region =>
        //        region["lines"].Select(line =>
        //            String.Join("", line["words"].Select(word => (string)word["text"]).ToList())
        //        ).ToList()
        //    ).ToList();
        //    return String.Join("\n", regions);
        //}

        internal static async void Recognize(Bitmap image)
        {
            //var text = await RecognizeOffline(image);
            //var text = await RecognizeOnline(image);
            await Task.Run(() => YouDaoOCR(image));
        }

        #region YouDao

        public static object HttpUtility { get; private set; }

        private static string YouDaoOCR(Bitmap image)
        {
            var bytes = ImgToBase64String(image);

            var from = "ja";
            var to = "zh-CHS";
            var type = "1";
            var salt = DateTime.Now.Millisecond.ToString();
            MD5 md5 = new MD5CryptoServiceProvider();
            string md5Str = appKey + bytes + salt + appSecret;
            byte[] output = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(md5Str));
            string sign = BitConverter.ToString(output).Replace("-", "");

            var dic = new Dictionary<string, string>
            {
                { "type", type.ToString() },
                { "from", from },
                { "to", to },
                { "appKey", appKey },
                { "salt", salt.ToString() },
                { "sign", sign },
                { "q", bytes },
            };

            var resp = JObject.Parse(Post(ocrUrl, dic));
            if ((int)resp["errorCode"] != 0)
            {
                return resp.ToString();
            }

            var regions = resp["resRegions"];
            var raw = String.Join("\n", regions.Select(region => (string)region["context"]).ToList());
            var text = String.Join("\n", regions.Select(region => (string)region["tranContent"]).ToList());
            context.Post(UpdateResult, new Dictionary<string, string> { { "raw", raw }, { "text", text } });
            return text;
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
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, WebUtility.UrlEncode(item.Value));
                i++;
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

        //const string ocrKey = "// TODO";
        //const string ocrUri = "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/ocr";
        //const string transKey = "// TODO";
        //const string transUri = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to=";
        //const string transTo = "zh-Hans";

        //private static async Task<string> AzureTranslate(string text)
        //{
        //    // https://docs.microsoft.com/zh-cn/azure/cognitive-services/translator/quickstart-csharp-translate

        //    System.Object[] body = new System.Object[] { new { Text = text } };
        //    var requestBody = JsonConvert.SerializeObject(body);

        //    using (var client = new HttpClient())
        //    using (var request = new HttpRequestMessage())
        //    {
        //        request.Method = HttpMethod.Post;
        //        request.RequestUri = new Uri(transUri + transTo);
        //        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        //        request.Headers.Add("Ocp-Apim-Subscription-Key", transKey);

        //        var response = await client.SendAsync(request);
        //        var responseBody = await response.Content.ReadAsStringAsync();
        //        var result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseBody), Formatting.Indented);

        //        return result;
        //    }
        //}

        //private static async Task<JObject> AzureOCRRequest(Image image)
        //{
        //    // https://docs.microsoft.com/zh-cn/azure/cognitive-services/Computer-vision/quickstarts/csharp-print-text

        //    try
        //    {

        //        HttpClient client = new HttpClient();

        //        // Request headers.
        //        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ocrKey);

        //        // Request parameters.
        //        string requestParameters = "language=unk&detectOrientation=true";

        //        // Assemble the URI for the REST API Call.
        //        string uri = ocrUri + "?" + requestParameters;

        //        HttpResponseMessage response;

        //        // Request body. Posts a locally stored JPEG image.
        //        byte[] byteData = ImageToBytes(image);

        //        using (ByteArrayContent content = new ByteArrayContent(byteData))
        //        {
        //            // This example uses content type "application/octet-stream".
        //            // The other content types you can use are "application/json"
        //            // and "multipart/form-data".
        //            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        //            // Make the REST API call.
        //            response = await client.PostAsync(uri, content);
        //        }

        //        // Get the JSON response.
        //        string contentString = await response.Content.ReadAsStringAsync();

        //        // Display the JSON response.
        //        return JObject.Parse(contentString);
        //    }
        //    catch (Exception e)
        //    {
        //        LogWriter.Log(e, "Azure request failed");
        //        return null;
        //    }
        //}

        //static byte[] ImageToBytes(Image image)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
        //        stream.Position = 0;

        //        var bytes = new Byte[stream.Length];
        //        stream.Read(bytes, 0, bytes.Length);

        //        return bytes;
        //    }
        //}

        #endregion
    }
}
