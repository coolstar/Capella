using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Capella
{
    public class OAuthUtils
    {
        public string GetNonce()
        {
            Random rand = new Random();
            int nonce = rand.Next(1000000000);
            return nonce.ToString();
        }

        public string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        public string GetData(String url, String getData, Account account, bool allowCaching)
        {
            String cookieHeader;
            return GetData(url, getData, account, allowCaching, out cookieHeader);
        }

        public string GetData(String url, String getData, Account account, bool allowCaching, out String cookieHeader)
        {
            Console.WriteLine(url);
            try
            {
                String cachedFile = "";
                if (allowCaching)
                {
                    SHA1 sha1 = SHA1.Create();
                    byte[] hashData = sha1.ComputeHash(Encoding.UTF8.GetBytes("GET" + this.UrlEncode(account.accessToken) + "+" + this.UrlEncode(url) + "+" + this.UrlEncode(getData)));
                    StringBuilder cacheBuilder = new StringBuilder();
                    for (int i = 0; i < hashData.Length; i++)
                    {
                        cacheBuilder.Append(hashData[i].ToString());
                    }

                    cachedFile = cacheBuilder.ToString();
                    bool cacheValid = false;
                    if (File.Exists(Path.GetTempPath() + "Capella\\" + cachedFile + ".cache"))
                    {
                        if (!NetworkInterface.GetIsNetworkAvailable())
                            cacheValid = true;
                        DateTime lastModified = File.GetLastWriteTimeUtc(Path.GetTempPath() + "Capella\\" + cachedFile + ".cache");
                        TimeSpan ts = DateTime.UtcNow - lastModified;
                        if (ts.TotalMinutes < 2)
                        {
                            cacheValid = true;
                        }
                    }
                    if (cacheValid)
                    {
                        cookieHeader = "";
                        return File.ReadAllText(Path.GetTempPath() + "Capella\\" + cachedFile + ".cache", UTF8Encoding.UTF8);
                    }
                }

                String authorization = account.accessToken;

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(new Uri(url + "?" + getData, UriKind.Absolute));
                req.Method = "GET";
                req.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 11.0; Windows NT 6.1; en-US))";
                req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                req.Headers.Add("Authorization", "BEARER " + authorization);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();

                String cookie = response.GetResponseHeader("Set-Cookie");
                cookieHeader = cookie;

                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                String output = reader.ReadToEnd();
                if (allowCaching)
                {
                    if (!Directory.Exists(Path.GetTempPath() + "Capella\\"))
                    {
                        Directory.CreateDirectory(Path.GetTempPath() + "Capella\\");
                    }
                    File.WriteAllText(Path.GetTempPath() + "Capella\\" + cachedFile + ".cache", output);
                }
                reader.Dispose();
                reader.Close();
                return output;
            }
            catch (WebException err)
            {
                Console.WriteLine(err.ToString());
                using (WebResponse response = err.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse) response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                        cookieHeader = "";
                        return text;
                    }
                }
                return "";
            }
        }

        public string PostData(string url, string postData, Account account, bool allowCaching)
        {
            try
            {
                String cachedFile = "";
                if (allowCaching)
                {
                    SHA1 sha1 = SHA1.Create();
                    byte[] hashData = sha1.ComputeHash(Encoding.UTF8.GetBytes("POST" + this.UrlEncode(account.accessToken) + "+" + this.UrlEncode(url) + "+" + this.UrlEncode(postData)));
                    StringBuilder cacheBuilder = new StringBuilder();
                    for (int i = 0; i < hashData.Length; i++)
                    {
                        cacheBuilder.Append(hashData[i].ToString());
                    }

                    cachedFile = cacheBuilder.ToString();
                    bool cacheValid = false;
                    if (File.Exists(Path.GetTempPath() + "Capella\\" + cachedFile + ".cache"))
                    {
                        if (!NetworkInterface.GetIsNetworkAvailable())
                            cacheValid = true;
                        DateTime lastModified = File.GetLastWriteTimeUtc(Path.GetTempPath() + "Capella\\" + cachedFile + ".cache");
                        TimeSpan ts = DateTime.UtcNow - lastModified;
                        if (ts.TotalMinutes < 2)
                        {
                            cacheValid = true;
                        }
                    }
                    if (cacheValid)
                    {
                        return File.ReadAllText(Path.GetTempPath() + "Capella\\" + cachedFile + ".cache", UTF8Encoding.UTF8);
                    }
                }

                String authorization = account.accessToken;

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(new Uri(url, UriKind.Absolute));
                req.Method = "POST";
                req.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 11.0; Windows NT 6.1; en-US))";
                req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                req.Headers.Add("Authorization", "BEARER " + authorization);
                req.ContentType = "application/x-www-form-urlencoded";
                //req.ContentLength = data.Length;
                using (var writer = new StreamWriter(req.GetRequestStream()))
                    writer.Write(postData);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();

                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                String output = reader.ReadToEnd();
                if (allowCaching)
                {
                    if (!Directory.Exists(Path.GetTempPath() + "Capella\\"))
                    {
                        Directory.CreateDirectory(Path.GetTempPath() + "Capella\\");
                    }
                    File.WriteAllText(Path.GetTempPath() + "Capella\\" + cachedFile + ".cache", output);
                }
                reader.Close();
                reader.Dispose();
                return output;
            }
            catch (WebException err)
            {
                Console.WriteLine(err.ToString());
                using (WebResponse response = err.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
                return "";
            }
        }

        public string PostData(string url, string postData, String imageFieldName, String imagePath, Account account)
        {
            try
            {
                String fileType = "";
                if (imagePath.ToLower().EndsWith(".jpeg") || imagePath.ToLower().EndsWith(".jpg"))
                    fileType = "image/jpeg";
                else if (imagePath.ToLower().EndsWith(".png"))
                    fileType = "image/png";
                else if (imagePath.ToLower().EndsWith(".gif"))
                    fileType = "image/gif";
                else if (imagePath.ToLower().EndsWith(".bmp"))
                    fileType = "image/bmp";

                String authorization = account.accessToken;

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(new Uri(url, UriKind.Absolute));
                req.Method = "POST";
                req.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 11.0; Windows NT 6.1; en-US))";
                req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                req.Headers.Add("Authorization", "BEARER " + authorization);
                req.KeepAlive = true;

                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

                req.ContentType = "multipart/form-data; boundary=" + boundary;
                var newLine = Environment.NewLine;
                var fileHeaderFormat = "--" + boundary + newLine +
                                        "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" + newLine+
                                        "Content-Type: " + fileType + newLine + 
                                        "Content-Transfer-Encoding: base64" + newLine + newLine;

                Stream rs = req.GetRequestStream();

                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

                string[] rawPartsOfPostData = postData.Split('&');
                foreach (string parameter in rawPartsOfPostData)
                {
                    string[] parts = parameter.Split('=');
                    String key = parts[0];
                    String value = "";
                    if (parts.Count() > 1)
                        value = Uri.UnescapeDataString(parts[1]);

                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key, value);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
                rs.Write(boundarybytes, 0, boundarybytes.Length);

                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(headerTemplate, imageFieldName, Path.GetFileName(imagePath), fileType);
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                rs.Write(headerbytes, 0, headerbytes.Length);

                FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    rs.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(trailer, 0, trailer.Length);
                rs.Close();

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();

                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                String output = reader.ReadToEnd();
                Console.WriteLine(output);
                reader.Close();
                reader.Dispose();

                Console.WriteLine("Upload done.");

                return output;
            }
            catch (WebException err)
            {
                Console.WriteLine(err.ToString());
                using (WebResponse response = err.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
                return "";
            }
        }

        public string DeleteData(String url, Account account)
        {
            Console.WriteLine(url);
            try
            {
                String authorization = account.accessToken;

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(new Uri(url, UriKind.Absolute));
                req.Method = "DELETE";
                req.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 11.0; Windows NT 6.1; en-US))";
                req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                req.Headers.Add("Authorization", "BEARER " + authorization);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();

                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                String output = reader.ReadToEnd();
                reader.Dispose();
                reader.Close();
                return output;
            }
            catch (WebException err)
            {
                Console.WriteLine(err.ToString());
                using (WebResponse response = err.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                        return text;
                    }
                }
                return "";
            }
        }


        public void StreamGet(String url, String getData, Action<String, Account> callback, Account account)
        {
            try
            {
                String authorization = account.accessToken;

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(new Uri(url + "?" + getData, UriKind.Absolute));
                req.Method = "GET";
                req.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 11.0; Windows NT 6.1; en-US))";
                req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                req.Headers.Add("Authorization", "BEARER " + authorization);
                req.Timeout = 2000;
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();

                Stream stream = response.GetResponseStream();
                req.Timeout = Timeout.Infinite;
                //StreamReader reader = new StreamReader(stream);
                LineReader reader = new LineReader(stream, 10240, UTF8Encoding.UTF8);

                //Console.WriteLine("Streaming Started for account {0}!", account.accessToken);

                //JsonTextReader jsonReader = new Json(reader);
                while (true)
                {
                    try
                    {
                        string streamedData = reader.ReadLine();
                        //Console.WriteLine(streamedData);
                        callback(streamedData, account);
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine("Stream Parse Error: "+e.ToString());
                        stream.Flush();
                        req.Abort();
                        Thread.Sleep(1000);
                        //Console.WriteLine("Restarting stream for account {0}!", account.accessToken);
                        this.StreamGet(url, getData, callback, account);
                    }
                }
            }
            catch (WebException err)
            {
                Console.WriteLine(err.ToString());
                using (WebResponse response = err.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    //Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        //Console.WriteLine("Stream Error: " + text);
                    }
                    Thread.Sleep(1000);
                    //Console.WriteLine("Restarting stream for account {0}!", account.accessToken);
                    this.StreamGet(url, getData, callback, account);
                }
            }
        }

        public string GetSignature(string sigBaseString, string consumerSecretKey, string requestTokenSecretKey = null)
        {
            var signingKey = string.Format("{0}&{1}", consumerSecretKey, !string.IsNullOrEmpty(requestTokenSecretKey) ? requestTokenSecretKey : "");
            string oauth_signature;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(signingKey)))
            {
                oauth_signature = Convert.ToBase64String(
                    hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(sigBaseString)));
            }
            return oauth_signature;
        }

        public void getTokens(String endpoint, out String consumerKey, out String consumerSecret)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(new Uri(endpoint + "api/v1/apps", UriKind.Absolute));
            req.Method = "POST";
            req.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 11.0; Windows NT 6.1; en-US))";
            req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            req.KeepAlive = true;

            String request = "client_name=" + UrlEncode("Capella") + "&redirect_uris=" + UrlEncode("https://localhost/") + "&scopes=" + UrlEncode("read write follow");

            using (var writer = new StreamWriter(req.GetRequestStream()))
                writer.Write(request);

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();

            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);

            String output = reader.ReadToEnd();

            dynamic json = JsonConvert.DeserializeObject(output);

            consumerKey = json["client_id"];
            consumerSecret = json["client_secret"];

            reader.Close();
            reader.Dispose();
        }

        public string getAccountToken(String endpoint, String consumerKey, String consumerSecret, String username, String password, out String streamCookie)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(new Uri(endpoint + "oauth/token", UriKind.Absolute));
            req.Method = "POST";
            req.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 11.0; Windows NT 6.1; en-US))";
            req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            req.KeepAlive = true;

            String request = "client_id=" + UrlEncode(consumerKey) + "&client_secret=" + UrlEncode(consumerSecret) + "&grant_type=password&username=" + UrlEncode(username) + "&password=" + UrlEncode(password) + "&scope=" + UrlEncode("read write follow");

            using (var writer = new StreamWriter(req.GetRequestStream()))
                writer.Write(request);

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();

            String cookie = response.GetResponseHeader("Set-Cookie");
            streamCookie = cookie;
            Console.WriteLine(cookie);

            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);

            String output = reader.ReadToEnd();

            Console.WriteLine(output);

            String token = "";

            try
            {
                dynamic json = JsonConvert.DeserializeObject(output);
                token = json["access_token"];
            } catch (Exception e)
            {
                token = "";
            }

            reader.Close();
            reader.Dispose();
            return token;
        }

        public String UrlEncode(String str)
        {
            //String first = HttpUtility.UrlEncode(str);
            String first = Uri.EscapeDataString(str);
            first = first.Replace("(", "%28");
            first = first.Replace(")", "%29");
            first = first.Replace("!", "%21");
            first = first.Replace("'", "%27");
            Regex reg = new Regex(@"%[a-f0-9]{2}");
            String final = reg.Replace(first, m => m.Value.ToUpperInvariant());
            return final;
        }
    }
}
