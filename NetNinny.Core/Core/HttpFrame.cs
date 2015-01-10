using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetNinny.Core
{
    /// <summary>
    /// Describes a full Http frame.
    /// </summary>
    public class HttpFrame
    {

        #region Variables
        /// <summary>
        /// Represents the data bound to this Http frame.
        /// </summary>
        byte[] m_data;
        /// <summary>
        /// Header of http frame.
        /// </summary>
        HttpHeader m_header;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the header of the frame.
        /// </summary>
        public HttpHeader Header
        {
            get { return m_header; }
            set { m_header = value; }
        }
        /// <summary>
        /// Path of the ressource asked for (in case of GET / POST)
        /// Happens just after the GET in the header.
        /// </summary>
        public string RessourcePath
        {
            get { return m_header.RessourcePath; }
            set { m_header.RessourcePath = value; }
        }
        /// <summary>
        /// Represents all the header lines, downcased in ASCII encoding.
        /// </summary>
        public List<string> HeaderLines
        {
            get { return m_header.HeaderLines; }
            set { m_header.HeaderLines = value; }
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
                return m_header[propertyName];
            }
            set
            {
                m_header[propertyName] = value;
            }
        }
        /// <summary>
        /// Kind of the http frame (GET, POST, or RESPONSE).
        /// </summary>
        public FrameKind FrameKind1
        {
            get { return m_header.FrameKind1; }
            set { m_header.FrameKind1 = value; }
        }
        /// <summary>
        /// Response code in case of RESPONSE frame kind. (ex : "200 OK").
        /// </summary>
        public string ResponseCode
        {
            get { return m_header.ResponseCode; }
            set { m_header.ResponseCode = value; }
        }
        /// <summary>
        /// Represents the data bound to this Http frame.
        /// </summary>
        public byte[] Data
        {
            get { return m_data; }
            set { m_data = value; }
        }
        /// <summary>
        /// Version of the http protocol used for this frame.
        /// </summary>
        public HttpVersion Version
        {
            get { return m_header.Version; }
            set { m_header.Version = value; }
        }
#if DEBUG
        /// <summary>
        /// Same as ToString().
        /// Created for debugging purposes only.
        /// </summary>
        public string __AsString { get { return this.ToString(); } }
#endif
        #endregion

        #region Methods

        /// <summary>
        /// Public constructor for HttpFrame.
        /// </summary>
        public HttpFrame()
        {
            m_header = new HttpHeader();
        }



        /// <summary>
        /// Creates a new HttpFrame from bytes.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static HttpFrame FromBytes(byte[] frame)
        {
            string utf8 = Encoding.UTF8.GetString(frame);
            string[] parts = utf8.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);

            // Gets the header.
            string headerStr = parts[0];
            HttpHeader header = HttpHeader.FromString(headerStr);

            // Recreates the data.
            byte[] data = Encoding.UTF8.GetBytes(String.Join("\r\n\r\n", parts.ToList().GetRange(1, parts.Length - 1).ToArray()));

            return new HttpFrame()
            {
                Data = data,
                m_header = header
            };
        }
        /// <summary>
        /// Transforms this representation of an HttpFrame into bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            byte[] headerBytes = Encoding.UTF8.GetBytes(m_header.ToString());
            byte[] data = new byte[headerBytes.Length + Data.Length];
            headerBytes.CopyTo(data, 0);
            Data.CopyTo(data, headerBytes.Length);
            return data;
        }

        /// <summary>
        /// Returns a string representation of this HttpFrame.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(this.ToBytes());
        }
        /// <summary>
        /// Gets the string corresponding to the given HttpVersion.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        static string GetVersionString(HttpVersion version)
        {
            return HttpHeader.GetVersionString(version);
        }
        /// <summary>
        /// Gets a version of the string with visible \n\r for debugging purposes.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DisplayString(string str)
        {
            return str.Replace("\r", "\\r").Replace("\n", "\\n\r\n");
        }
        /// <summary>
        /// Gets the header as string.
        /// </summary>
        /// <returns></returns>
        public string HeaderString()
        {
            return m_header.ToString();
        }
        #endregion
    }
}
