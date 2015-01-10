using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetNinny.Core
{
    #region Exceptions / Enums
    public class InexistantPropertyException : Exception { }
    public enum FrameKind { GET, POST, CONNECT, RESPONSE };
    public enum HttpVersion { V1_1, V1_0 };
    #endregion

    /// <summary>
    /// Describes a http header.
    /// </summary>
    public class HttpHeader
    {
        #region Variables
        /// <summary>
        /// Represents all the header lines in ASCII encoding.
        /// </summary>
        List<string> m_headerLines;
        /// <summary>
        /// Version of the http protocol used for this frame.
        /// </summary>
        HttpVersion m_version;
        /// <summary>
        /// Kind of the http frame (GET, POST, or RESPONSE).
        /// </summary>
        FrameKind m_frameKind;
        /// <summary>
        /// Path of the ressource asked for (in case of GET / POST)
        /// Happens just after the GET in the header.
        /// </summary>
        string m_ressourcePath;
        /// <summary>
        /// Response code in case of RESPONSE frame kind. (ex : "200 OK").
        /// </summary>
        string m_responseCode;
        #endregion


        #region Properties
        
        /// <summary>
        /// Path of the ressource asked for (in case of GET / POST)
        /// Happens just after the GET in the header.
        /// </summary>
        public string RessourcePath
        {
            get { return m_ressourcePath; }
            set { m_ressourcePath = value; }
        }
        /// <summary>
        /// Represents all the header lines, downcased in ASCII encoding.
        /// </summary>
        public List<string> HeaderLines
        {
            get { return m_headerLines; }
            set { m_headerLines = value; }
        }
        /// <summary>
        /// Gets or sets the property with the given name in the http header.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string this[string propertyName]
        {
            get
            {
                foreach (string line in m_headerLines)
                {
                    string[] lines = line.Split(new string[] { ": " }, StringSplitOptions.None);
                    if (lines[0].ToLower() == (propertyName.ToLower()))
                    {
                        return lines[1];
                    }
                }
                return "default";
            }
            set
            {
                for (int i = 0; i < m_headerLines.Count; i++)
                {
                    string line = m_headerLines[i].Split(new string[] { ": "}, StringSplitOptions.None)[0].ToLower();
                    if (line == (propertyName.ToLower()))
                    {
                        m_headerLines[i] = propertyName + ": " + value;
                        return;
                    }
                }

                // Create a new property if it isn't in the original header.
                m_headerLines.Add(propertyName + ": " + value);
            }
        }
        /// <summary>
        /// Kind of the http frame (GET, POST, or RESPONSE).
        /// </summary>
        public FrameKind FrameKind1
        {
            get { return m_frameKind; }
            set { m_frameKind = value; }
        }
        /// <summary>
        /// Response code in case of RESPONSE frame kind. (ex : "200 OK").
        /// </summary>
        public string ResponseCode
        {
            get { return m_responseCode; }
            set { m_responseCode = value; }
        }
        /// <summary>
        /// Version of the http protocol used for this frame.
        /// </summary>
        public HttpVersion Version
        {
            get { return m_version; }
            set { m_version = value; }
        }
#if DEBUG
        /// <summary>
        /// Same as ToString().
        /// Created for debugging purposes only.
        /// </summary>
        public string __AsString { get { return this.ToString(); } }
#endif
        #endregion

        /// <summary>
        /// Constructs a http header from bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static HttpHeader FromBytes(byte[] bytes)
        {
            return FromString(Encoding.ASCII.GetString(bytes));
        }
        /// <summary>
        /// Gets the string corresponding to the given HttpVersion.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string GetVersionString(HttpVersion version)
        {
            switch (version)
            {
                case HttpVersion.V1_0:
                    return "HTTP/1.0";
                case HttpVersion.V1_1:
                    return "HTTP/1.1";
                default: // for the compiler.
                    return "HTTP/1.1";
            }
        }
        /// <summary>
        /// Constructs a http header from an ascii string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static HttpHeader FromString(string ascii)
        {
            // Gets the header string.
            string header;
            if (ascii.Contains("\r\n\r\n"))
            {
                string[] parts = ascii.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);
                header = parts[0];
            }
            else
                header = ascii;

            // Gets the header.
            string[] headerLines = header.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string firstLine = headerLines[0];
            string[] httpHeaderLines = headerLines.ToList().GetRange(1, headerLines.Length - 1).ToArray();

            // Frame kind
            FrameKind kind;
            if (firstLine.Contains("GET"))
                kind = FrameKind.GET;
            else if (firstLine.Contains("POST"))
                kind = FrameKind.POST;
            else if (firstLine.Contains("CONNECT"))
                kind = FrameKind.CONNECT;
            else
                kind = FrameKind.RESPONSE;

            // Http version
            HttpVersion version;
            if (firstLine.Contains("HTTP/1.0"))
                version = HttpVersion.V1_0;
            else
                version = HttpVersion.V1_1;

            
            // Ressource path
            string ressourcePath = "";
            if(kind == FrameKind.GET || kind == FrameKind.POST)
            {
                ressourcePath = firstLine.Split(' ')[1];
            }

            // Response code
            string responseCode = "";
            if(kind == FrameKind.RESPONSE)
            {
                responseCode = firstLine.Replace(GetVersionString(version), "").Trim();
            }

            // Returns the created header.
            return new HttpHeader()
            {
                FrameKind1 = kind,
                HeaderLines = httpHeaderLines.ToList(),
                ResponseCode = responseCode,
                Version = version,
                RessourcePath = ressourcePath
            };
        }


        /// <summary>
        /// Gets the header string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder header = new StringBuilder();
            // Appends the first line of the HTTP frame.
            switch (this.FrameKind1)
            {
                case FrameKind.GET:
                    header.Append("GET " + RessourcePath + " " + GetVersionString(Version) + "\r\n");
                    break;
                case FrameKind.POST:
                    header.Append("POST " + RessourcePath + " " + GetVersionString(Version) + "\r\n");
                    break;
                case FrameKind.CONNECT:
                    header.Append("CONNECT " + RessourcePath + " " + GetVersionString(Version) + "\r\n");
                    break;
                case FrameKind.RESPONSE:
                    header.Append(GetVersionString(Version) + " " + ResponseCode + "\r\n");
                    break;
            }
            // Appends the header lines.
            header.Append(string.Join("\r\n", HeaderLines));
            header.Append("\r\n\r\n");
            return header.ToString();
        }
        /// <summary>
        /// Transforms this representation of an HttpFrame into bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return Encoding.ASCII.GetBytes(ToString());

        }


        /// <summary>
        /// Creates a new instance of HttpHeader.
        /// </summary>
        public HttpHeader()
        {
            m_headerLines = new List<string>();
        }
    }
 
}
