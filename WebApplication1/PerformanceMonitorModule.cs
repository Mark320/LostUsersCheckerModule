using System;
using System.IO;
using System.Text;
using System.Web;
using Lucene.Net.Search;

namespace WebApplication1
{
    public class PerformanceMonitorModule : IHttpModule
    {
        public void Init(HttpApplication httpApp)
        {
            httpApp.BeginRequest += BeginRequest;
            httpApp.BeginRequest += BeginRequest2;
        }

        private static void BeginRequest(object sender, EventArgs e)
        {
            //HttpApplication application = (HttpApplication)sender;
            //HttpContext context = application.Context;
            //if ((context.Request.Url.AbsolutePath == "/testurl") || context.Request.Url.AbsolutePath == "/testurl2")
            //{
            //    context.Response.StatusCode = 200;
            //    context.ApplicationInstance.CompleteRequest();
            //}
        }

        private static void BeginRequest2(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;

            if (context.Response.StatusCode == 200 && context.Response.ContentType == "text/html")
            {
                context.Response.Filter =
                    new InjectScriptStream(context.Response.Filter, Guid.NewGuid());
            }
        }

        public void Dispose()
        {
        }
    }

    public class InjectScriptStream : MemoryStream
    {
        private readonly Stream ResponseStream;
        private readonly Guid Id;

        private const string Tag = @"
            <script defer src='testurl?guid={0}'></script>
            <noscript>
                <img src='testurl2?guid={0}' style='position:absolute;top:-50px;left-50px;' />
            </noscript>
        ";

        public InjectScriptStream(Stream stream, Guid id)
        {
            ResponseStream = stream;
            Id = id;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var stringValue = Encoding.UTF8.GetString(buffer);
            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                var position = stringValue.IndexOf("</body>", StringComparison.Ordinal);
                if (position != -1)
                {
                    stringValue = stringValue.Insert(position, string.Format(Tag, Id));
                    var editedBuffer = Encoding.UTF8.GetBytes(stringValue.ToCharArray());
                    ResponseStream.Write(editedBuffer, offset, editedBuffer.Length);
                }
            }

            ResponseStream.Write(buffer, offset, count);
        }
    }
}