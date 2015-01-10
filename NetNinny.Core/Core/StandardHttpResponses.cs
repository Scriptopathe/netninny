using System.Text;

namespace NetNinny.Core
{
    /// <summary>
    /// Class that contains several standart http responses.
    /// </summary>
    public class StandardHttpResponses
    {
        /// <summary>
        /// Returns a 302 found response with a redirection to the given url.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpFrame RedirectTo(HttpVersion version, string url)
        {
            /*  Example of output :

                HTTP/1.1 302 Found
                Location: http://www.iana.org/domains/example/
            */
            HttpFrame frame = new HttpFrame();
            frame.Version = version;
            frame.ResponseCode = "302 Found";
            frame.FrameKind1 = FrameKind.RESPONSE;
            frame.Data = new byte[0];
            frame["Location"] = url;
            return frame;
        }
        /// <summary>
        /// Returns a not supported request.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpFrame Unsupported(HttpVersion version)
        {
            HttpFrame frame = new HttpFrame();
            frame.Version = version;
            frame.ResponseCode = "405 Method Not Allowed";
            frame.FrameKind1 = FrameKind.RESPONSE;
            frame["Content-Type"] = "text/html";
            frame.Data = Encoding.UTF8.GetBytes("<html><body><b>Unsupported CONNECT request</b></body></html>\r\n\r\n");
            frame["Content-Length"] = frame.Data.Length.ToString();
            return frame;
        }
        
    }
}
