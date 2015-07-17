using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Collections.Specialized;
using System.Net;
using System.Globalization;

namespace DlGoogleSpreedSheet
{
    
    static class Program
    {
        public class WebClientEx : WebClient
        {
            public WebClientEx(CookieContainer container)
            {
                this.container = container;
            }

            private readonly CookieContainer container = new CookieContainer();

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest r = base.GetWebRequest(address);
                var request = r as HttpWebRequest;
                if (request != null)
                {
                    request.CookieContainer = container;
                }
                return r;
            }

            protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
            {
                WebResponse response = base.GetWebResponse(request, result);
                ReadCookies(response);
                return response;
            }

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                WebResponse response = base.GetWebResponse(request);
                ReadCookies(response);
                return response;
            }

            private void ReadCookies(WebResponse r)
            {
                var response = r as HttpWebResponse;
                if (response != null)
                {
                    CookieCollection cookies = response.Cookies;
                    container.Add(cookies);
                }
            }
        }

        public static Dictionary<string,string> fileUrls;

        static void downloadAllSpreedSheetFiles()
        {
            fileUrls = new Dictionary<string, string>();
            int counter = 0;
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file =
               new System.IO.StreamReader(@"C:\Users\shahnewaz\Documents\GapMinder\config.txt");
            while ((line = file.ReadLine()) != null)
            {
                counter++;
                string[] arr = line.Split('\t');
                if (arr[5].StartsWith("http"))
                {
                    string url = @"{0}&usp=sharing&output=csv"; // REPLACE THIS WITH YOUR URL
                    url = String.Format(url, arr[5].Trim());

                    fileUrls.Add(arr[0].Trim(),arr[5].Trim());

                    WebClientEx wc = new WebClientEx(new CookieContainer());
                    wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:22.0) Gecko/20100101 Firefox/22.0");
                    wc.Headers.Add("DNT", "1");
                    wc.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                    wc.Headers.Add("Accept-Encoding", "deflate");
                    wc.Headers.Add("Accept-Language", "en-US,en;q=0.5");

                    string path = @"C:\Users\shahnewaz\Documents\GapMinder\Output\" + arr[0].Trim()+"";
                    //path += (arr[1].Equals("") || arr[1].StartsWith("---")) ? counter + "" : arr[1].Trim();
                    path += ".csv";

                    if (File.Exists(path))
                        continue;
                    var outputCSVdata = wc.DownloadString(url);
                    using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path))
                    {
                        sw.WriteLine(outputCSVdata);
                    }
                }
            }

            file.Close();

        }

        static void takeOnlyFirstRow()
        {
            string path = @"C:\Users\shahnewaz\Documents\GapMinder\sample.txt";
            string[] filePaths = Directory.GetFiles(@"C:\Users\shahnewaz\Documents\GapMinder\Output\", "*.csv");
            using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path))
            {
                foreach (string csvFile in filePaths)
                {
                    System.IO.StreamReader file =
                    new System.IO.StreamReader(csvFile);
                    sw.WriteLine(csvFile+"," + file.ReadLine());
                    file.Close();
                    file.Dispose();
                }
            }
        }

        static string[] SplitQuoted(this string input, char separator, char quotechar)
        {
            List<string> tokens = new List<string>();

            StringBuilder sb = new StringBuilder();
            bool escaped = false;
            foreach (char c in input)
            {
                if (c.Equals(separator) && !escaped)
                {
                    // we have a token
                    tokens.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else if (c.Equals(separator) && escaped)
                {
                    // ignore but add to string
                    sb.Append(c);
                }
                else if (c.Equals(quotechar))
                {
                    escaped = !escaped;
                    sb.Append(c);
                }
                else
                {
                    sb.Append(c);
                }
            }
            tokens.Add(sb.ToString().Trim());

            return tokens.ToArray();
        }

        static string[] getTokens(string line)
        {
            string[] tokens;
            //if (line[0] == '\"')
                tokens = SplitQuoted(line, ',', '\"');
            //else
                //tokens = line.Split(',');
            return tokens;
        }

        static void createSingleFileWithAllData()
        {
            string path = @"C:\Users\shahnewaz\Documents\GapMinder\allRawData.txt";
            string indicatorPath = @"C:\Users\shahnewaz\Documents\GapMinder\indicators.txt";
            StreamWriter sIndiator = (File.Exists(indicatorPath)) ? File.AppendText(indicatorPath)
                    : File.CreateText(indicatorPath);
            string[] filePaths = Directory.GetFiles(@"C:\Users\shahnewaz\Documents\GapMinder\Output\", "1915.csv");
            using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path))
            {
                foreach (string csvFile in filePaths)
                {
                    System.IO.StreamReader file =
                    new System.IO.StreamReader(csvFile);
                    string[] headerTokens = getTokens(file.ReadLine());
                    string line = null;

                    string fileID = csvFile.Replace(@"C:\Users\shahnewaz\Documents\GapMinder\Output\", "").Replace(".csv", "");

                    sIndiator.WriteLine( fileID + ","
                        + (headerTokens[0] == "" ? "N/A" : headerTokens[0].Replace(",",";")) + ","
                        + fileUrls[fileID].Split('=')[1].ToString());

                    while ((line = file.ReadLine()) != null)
                    {
                        string[] tokens = getTokens(line);
                        for (int i = 1; i < tokens.Length; i++)
                        {
                            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");
                            string str = tokens[i].Replace(",", "").Replace("\"", "")
                                .Replace("(","").Replace(")","").Trim();

                            //double value = 0.00;
                            //bool b = Double.TryParse(str
                            //    , System.Globalization.NumberStyles.Float, culture, out value);
                            //if (!b)
                            //    value = null;

                            sw.WriteLine(csvFile + ","
                                        + fileID + ","
                                        + tokens[0].Replace(',', ';') + ","
                                        + headerTokens[i] + ","
                                        + str);
                        }
                    }
                    file.Close();
                    file.Dispose();
                    //break;
                }
                sIndiator.Close();
            }
        }

        static void Main(string[] args)
        {
            downloadAllSpreedSheetFiles();
            //takeOnlyFirstRow();
            createSingleFileWithAllData();
        }

        //static void Main(string[] args)
        //{
        //    ////////////////////////////////////////////////////////////////////////////
        //    // STEP 1: Configure how to perform OAuth 2.0
        //    ////////////////////////////////////////////////////////////////////////////

        //    // TODO: Update the following information with that obtained from
        //    // https://code.google.com/apis/console. After registering
        //    // your application, these will be provided for you.

        //    string CLIENT_ID = "678185573787-mvlbq4ie4e0dq4f0sthujrak978m6uuo.apps.googleusercontent.com";

        //    // This is the OAuth 2.0 Client Secret retrieved
        //    // above.  Be sure to store this value securely.  Leaking this
        //    // value would enable others to act on behalf of your application!
        //    string CLIENT_SECRET = "qkRbEB9cDfYLMkCRa8FS-bX8";

        //    // Space separated list of scopes for which to request access.
        //    string SCOPE = "https://spreadsheets.google.com/feeds https://docs.google.com/feeds";

        //    // This is the Redirect URI for installed applications.
        //    // If you are building a web application, you have to set your
        //    // Redirect URI at https://code.google.com/apis/console.
        //    string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";

        //    ////////////////////////////////////////////////////////////////////////////
        //    // STEP 2: Set up the OAuth 2.0 object
        //    ////////////////////////////////////////////////////////////////////////////

        //    // OAuth2Parameters holds all the parameters related to OAuth 2.0.
        //    OAuth2Parameters parameters = new OAuth2Parameters();

        //    // Set your OAuth 2.0 Client Id (which you can register at
        //    // https://code.google.com/apis/console).
        //    parameters.ClientId = CLIENT_ID;

        //    // Set your OAuth 2.0 Client Secret, which can be obtained at
        //    // https://code.google.com/apis/console.
        //    parameters.ClientSecret = CLIENT_SECRET;

        //    // Set your Redirect URI, which can be registered at
        //    // https://code.google.com/apis/console.
        //    parameters.RedirectUri = REDIRECT_URI;

        //    ////////////////////////////////////////////////////////////////////////////
        //    // STEP 3: Get the Authorization URL
        //    ////////////////////////////////////////////////////////////////////////////

        //    // Set the scope for this particular service.
        //    parameters.Scope = SCOPE;

        //    // Get the authorization url.  The user of your application must visit
        //    // this url in order to authorize with Google.  If you are building a
        //    // browser-based application, you can redirect the user to the authorization
        //    // url.
        //    string authorizationUrl = OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
        //    Console.WriteLine(authorizationUrl);
        //    Console.WriteLine("Please visit the URL above to authorize your OAuth "
        //      + "request token.  Once that is complete, type in your access code to "
        //      + "continue...");
        //    parameters.AccessCode = "4/F3rCTci_Lz56hst507uixO-d351w-AwWDEcchhnsYYE.gv-iyLhcNWQZcp7tdiljKKafyOX5mgI";//Console.ReadLine();

        //    ////////////////////////////////////////////////////////////////////////////
        //    // STEP 4: Get the Access Token
        //    ////////////////////////////////////////////////////////////////////////////

        //    // Once the user authorizes with Google, the request token can be exchanged
        //    // for a long-lived access token.  If you are building a browser-based
        //    // application, you should parse the incoming request token from the url and
        //    // set it in OAuthParameters before calling GetAccessToken().
        //    OAuthUtil.GetAccessToken(parameters);
        //    string accessToken = parameters.AccessToken;
        //    Console.WriteLine("OAuth Access Token: " + accessToken);

        //    ////////////////////////////////////////////////////////////////////////////
        //    // STEP 5: Make an OAuth authorized request to Google
        //    ////////////////////////////////////////////////////////////////////////////

        //    // Initialize the variables needed to make the request
        //    //GOAuth2RequestFactory requestFactory =
        //    //    new GOAuth2RequestFactory(null, "MySpreadsheetIntegration-v1", parameters);
        //    //SpreadsheetsService service = new SpreadsheetsService("MySpreadsheetIntegration-v1");
        //    //service.RequestFactory = requestFactory;

        //    // Make the request to Google
        //    // See other portions of this guide for code to put here...


        //    //ServiceAccountCredential.Initializer initializer =
        //    //new ServiceAccountCredential.Initializer("")
        //    //{
        //    //    u
        //    //}.FromCertificate(certificate);

        //    string DownloadCsvUrl = "https://docs.google.com/feeds/download/spreadsheets/Export?key={0}&gid={1}&exportFormat={2}";


        //    var sp = new BaseClientService.Initializer()

        //    {
        //        ApiKey = accessToken,
        //        ApplicationName = "DlGoogleSpreedSheet"
        //    };

        //    Google.Apis.Download.MediaDownloader downloader = new Google.Apis.Download.MediaDownloader(sp);
        //    var outputStream = new MemoryStream();
        //    DownloadCsvUrl = String.Format(DownloadCsvUrl, "", "0", "csv");
        //    downloader.Download(DownloadCsvUrl, outputStream);


        //    //using (WebClient client = new WebClient())
        //    //{

        //    //    byte[] response =
        //    //    client.UploadValues("https://accounts.google.com/o/oauth2/auth", new NameValueCollection()
        //    //    {
        //    //        { "scope", "email profile" },
        //    //        { "redirect_uri", "urn:ietf:wg:oauth:2.0:oob" },
        //    //        { "response_type", "code" },
        //    //        { "client_id", "678185573787-mvlbq4ie4e0dq4f0sthujrak978m6uuo.apps.googleusercontent.com" }
        //    //    });

        //    //        string result = System.Text.Encoding.UTF8.GetString(response);
        //    //}

        //    // Create a request using a URL that can receive a post. 
        //    WebRequest request = WebRequest.Create("https://accounts.google.com/o/oauth2/auth?scope=email%20profile&redirect_uri=urn:ietf:wg:oauth:2.0:oob&response_type=code&client_id=678185573787-mvlbq4ie4e0dq4f0sthujrak978m6uuo.apps.googleusercontent.com&include_granted_scopes=true");
        //    // Set the Method property of the request to POST.
        //    request.Method = "POST";

        //    // Create POST data and convert it to a byte array.
        //    string postData = "This is a test that posts this string to a Web server.";
        //    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        //    // Set the ContentType property of the WebRequest.
        //    //request.ContentType = "application/x-www-form-urlencoded";
        //    // Set the ContentLength property of the WebRequest.
        //    //request.ContentLength = byteArray.Length;

        //    // Get the request stream.
        //    Stream dataStream = request.GetRequestStream();
        //    // Write the data to the request stream.
        //    //dataStream.Write(byteArray, 0, byteArray.Length);
        //    // Close the Stream object.
        //    //dataStream.Close();

        //    // Get the response.
        //    WebResponse response = request.GetResponse();
        //    // Display the status.
        //    Console.WriteLine(((HttpWebResponse)response).StatusDescription);
        //    // Get the stream containing content returned by the server.
        //    dataStream = response.GetResponseStream();
        //    // Open the stream using a StreamReader for easy access.
        //    StreamReader reader = new StreamReader(dataStream);
        //    // Read the content.
        //    string responseFromServer = reader.ReadToEnd();
        //    // Display the content.
        //    Console.WriteLine(responseFromServer);
        //    // Clean up the streams.
        //    reader.Close();
        //    dataStream.Close();
        //    response.Close();

        //}
    }
}
