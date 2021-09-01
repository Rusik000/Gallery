using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Size = System.Drawing.Size;

namespace GalleryAK
{
    public partial class MainWindow : Window
    {        
        private static string BinaryToBase64Compat(byte[] content)
        {
            string base64 = Convert.ToBase64String(content).Replace('+', '-').Replace('/', '_');
            return base64;
        }

        public async Task GoogleImageSearch(string imagePath, bool includeFileName, CancellationToken cancelToken)
        {            
            if (memStream == null)
            {
                return;
            }
            byte[] data = memStream.ToArray();

            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;

            using (var client = new HttpClient(handler))
            {
                var form = new MultipartFormDataContentCompat();
                form.Add(new StringContent(BinaryToBase64Compat(data)), "image_content");
                if (includeFileName)
                {
                    form.Add(new StringContent(Path.GetFileName(imagePath)), "filename");
                }
                var response = await client.PostAsync("https://images.google.com/searchbyimage/upload", form, cancelToken);
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }
                if (response.StatusCode != HttpStatusCode.Redirect)
                {
                    MessageBox.Show("Expected redirect to results page, got " + (int)response.StatusCode);
                    return;
                }                
                string resultUrl = response.Headers.Location.ToString();
                TryOpenBrowser(resultUrl);                               
            }
        }
        private class MultipartFormDataContentCompat : MultipartContent
        {
            public MultipartFormDataContentCompat() : base("form-data")
            {
                FixBoundaryParameter();
            }

            public MultipartFormDataContentCompat(string boundary) : base("form-data", boundary)
            {
                FixBoundaryParameter();
            }

            public override void Add(HttpContent content)
            {
                base.Add(content);
                AddContentDisposition(content, null, null);
            }

            public void Add(HttpContent content, string name)
            {
                base.Add(content);
                AddContentDisposition(content, name, null);
            }

            public void Add(HttpContent content, string name, string fileName)
            {
                base.Add(content);
                AddContentDisposition(content, name, fileName);
            }

            private void AddContentDisposition(HttpContent content, string name, string fileName)
            {
                var headers = content.Headers;
                if (headers.ContentDisposition == null)
                {
                    headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = QuoteString(name),
                        FileName = QuoteString(fileName)
                    };
                }
            }

            private void FixBoundaryParameter()
            {
                var boundary = Headers.ContentType.Parameters.Single(p => p.Name == "boundary");
                boundary.Value = boundary.Value.Trim('"');
            }

            private static string QuoteString(string str)
            {
                return '"' + str + '"';
            }
        }
        
        private static bool TryOpenBrowser(string GoogleUrl)
        {
            try
            {
                if (GoogleUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || GoogleUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    Process.Start(GoogleUrl);
                    return true;
                }
                else
                {
                    MessageBox.Show("Security concern, task.result does not start with \"http\" : " + GoogleUrl);
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to open browser: " + ex);
                return false;
            }
        }
    }
}

