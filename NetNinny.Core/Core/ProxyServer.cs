using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
namespace NetNinny.Core
{
    /// <summary>
    /// Class whose instance is able to handle incoming connexion, and perform forwarding and content filtering.
    /// </summary>
    public sealed class ProxyServer
    {
        public enum CloseReason
        {
            ClosedByUser,
            Error
        }

        #region Constants
        public const int CLIENT_RECEIVE_TIMEOUT = 200;
        public const int SERVER_RECEIVE_TIMEOUT = 2000;
        #endregion

        #region Variables
        /// <summary>
        /// Log used by this server.
        /// </summary>
        Tools.LogSystem m_logSys;
        /// <summary>
        /// Filters that this connexion handler should use for content/url filtering.
        /// </summary>
        Filtering.FilterCollection m_filters;
        /// <summary>
        /// Returns a value indicating that the service is running.
        /// </summary>
        volatile bool m_running;
        /// <summary>
        /// Reason why the server has stopped running.
        /// </summary>
        CloseReason m_closeReason;
        /// <summary>
        /// Error message indicating why the server has closed, in case of an exception being thrown.
        /// </summary>
        string m_closeErrorMessage;
        /// <summary>
        /// Socket listening for incoming connexion.
        /// </summary>
        Socket m_servSocket;
        /// <summary>
        /// If set to true, the proxy will ask to receive pages with utf-8 encoding. (disables gzip/deflate).
        /// </summary>
        bool m_forceUtf8;
        #endregion

        #region Properties
        /// <summary>
        /// If set to true, the proxy will ask to receive pages with utf-8 encoding. (disables gzip/deflate).
        /// </summary>
        public bool ForceUtf8
        {
            get { return m_forceUtf8; }
            set { m_forceUtf8 = value; }
        }
        /// <summary>
        /// Gets the log system used by this server.
        /// </summary>
        public Tools.LogSystem LogSys
        {
            get { return m_logSys; }
            private set { m_logSys = value; }
        }

        /// <summary>
        /// Gets the reason why the server has exited.
        /// </summary>
        public CloseReason ExitReason
        {
            get { return m_closeReason; }
        }
        /// <summary>
        /// Gets the error message (if any) indicating why the server has closed, in case of an error.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                if (m_closeReason == CloseReason.Error)
                    return m_closeErrorMessage;
                else
                    throw new InvalidOperationException("The server has not exited with error.");
            }
        }
        /// <summary>
        /// Gets or sets the filters that this connexion handler should use for content/url filtering.
        /// </summary>
        public Filtering.FilterCollection Filters
        {
            get { return m_filters; }
            set { m_filters = value; }
        }

        /// <summary>
        /// Gets a value indicating if the server is running.
        /// </summary>
        public bool IsRunning
        {
            get { return m_running; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the log output to the standard output (console).
        /// </summary>
        /// <param name="verbosity"></param>
        public void SetLogToConsole(Tools.LogVerbosity verbosity = Tools.LogVerbosity.UserInfo_Error)
        {
            LogSys.Log += (Tools.LogEntry entry) => 
            {
                if ((entry.Verbosity & verbosity) == entry.Verbosity)
                {
                    Console.WriteLine(entry.Message);
                }
            };
        }
        /// <summary>
        /// Creates a new connexion handler.
        /// </summary>
        public ProxyServer()
        {
            m_filters = new Filtering.FilterCollection();
            m_logSys = new Tools.LogSystem();
            m_forceUtf8 = false;
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            m_running = false;
            m_servSocket.Close();
        }

        /// <summary>
        /// Starts running the proxy server on a new thread.
        /// </summary>
        public void Start(int port, string address, Filtering.FilterCollection filters)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                RunServer(port, address, filters);
            }));
            thread.Start();
            Thread.Sleep(10);
        }

        /// <summary>
        /// Starts running the proxy server in the current thread.
        /// The port and address of binding are configurable.
        /// </summary>
        public void RunServer(int port, string address, Filtering.FilterCollection filters)
        {
            if (m_running)
                throw new InvalidOperationException("The server is already running.");

            m_running = true;
            m_filters = filters;
            // Creates a new IPV4 socket using TCP.
            m_servSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // Debug value, should not be hardcoded in future versions of 
                // the program.
                int maxConnexions = 1000;

                // ------ FEATURE 7: Configurable proxy port ------ 
                // Binds the server socket to localhost at a configurable port.
                m_servSocket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
                m_servSocket.Listen(maxConnexions);
                LogSys.LogInfo("Server running on port " + ((IPEndPoint)m_servSocket.LocalEndPoint).Port + " at address " + address, -1);
                int clientCount = 1;
                while (m_running)
                {
                    // Connexion from the proxy to the client.
                    Socket clientConnexion = m_servSocket.Accept();
                    int clientNumber = clientCount;
                    LogSys.LogInfo("Incoming client connexion #" + clientNumber.ToString() + "...", clientNumber);
                    HandleConnexion(clientConnexion, clientNumber);
                    clientCount++;
                }

                // Properly ends the server
                m_closeReason = CloseReason.ClosedByUser;
            }
            catch (SocketException e)
            {
                LogSys.LogError("[Server:RunServer] SocketException : " + e.Message, -1);
                m_closeReason = CloseReason.Error;
                m_closeErrorMessage = e.Message;
            }
            finally
            {
                m_servSocket.Close();
                m_running = false;
            }
        }

        #region Handle connexion v2
        /// <summary>
        /// Creates and runs a new thread handling the given incomming clientConnexion.
        /// </summary>
        /// <param name="clientConnexion">The client socket to handle.</param>
        /// <param name="clientCount">The number of the client (for display / debug purposes)</param>
        public void HandleConnexion(Socket clientConnexion, int clientNumber)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {

                // Gets the browser request
                byte[] browserRequestBytes;
                var status = Tools.SocketUtils.ReceiveHeader(clientConnexion, out browserRequestBytes, 30000);

                if (browserRequestBytes.Length == 0)
                {
                    LogSys.LogInfo("Got empty request.", clientNumber);
                    return;
                }
                
                // Analyses the request.
                HttpHeader browserRequest = HttpHeader.FromBytes(browserRequestBytes);
                LogSys.LogEntry("[Verbose] Got request from browser: \r\n" + HttpFrame.DisplayString(browserRequest.ToString()), clientNumber, Tools.LogVerbosity.Verbose);

                // Does not support Tunneling
                if (browserRequest.FrameKind1 == FrameKind.CONNECT)
                {
                    HandleConnectMethod(browserRequest, clientConnexion, clientNumber);
                    return;
                }

                // ------ FEATURE 3: URL Filtering ------ 
                string url = browserRequest["Host"] + browserRequest.RessourcePath;
                bool isInvalidUrl = m_filters.ContainsFilteredItems(url);
                if (isInvalidUrl)
                {
                    HandleInvalidURL(browserRequest, clientConnexion, url, clientNumber);
                    return;
                }

                // ------ FEATURE 6: Compatible with all major browsers ------ 
                // Pre-processes the request for compatibility with major browsers, and other tweaks.
                PrepareRequest(browserRequest, clientNumber);
                
                // Creates a socket to the remote server.
                Socket serverSocket = CreateSocketToRemoveServer(browserRequest, clientNumber);
                if(serverSocket == null)
                {
                    LogSys.LogError("Connexion to server aborted.", clientNumber);
                    clientConnexion.Close();
                    LogSys.LogError("Client connection closed.", clientNumber);
                    return;
                }

                // ------ FEATURE 2: Handles Get requests ------ 
                if (browserRequest.FrameKind1 == FrameKind.GET || browserRequest.FrameKind1 == FrameKind.POST)
                {
                    // Sends the request to the remove server.
                    LogSys.LogEntry("[Verbose] Sending request to remote server: \r\n" + HttpFrame.DisplayString(browserRequest.ToString()), clientNumber, Tools.LogVerbosity.Verbose);
                    if (!Tools.SocketUtils.SendBytes(serverSocket, browserRequest.ToBytes()))
                    {
                        LogSys.LogError("Error occured while sending bytes to remote server : the connexion has been closed.", clientNumber);
                        return;
                    }
                }

                // ------ FEATURE 9: Handles Post requests ------ 
                // ------ + No size limit for Post data !
                if(browserRequest.FrameKind1 == FrameKind.POST)
                {
                    SendPostData(browserRequest, clientConnexion, serverSocket, clientNumber);
                }

                // Processes the server's response.
                if (browserRequest.FrameKind1 == FrameKind.POST || browserRequest.FrameKind1 == FrameKind.GET)
                {
                    ProcessResponse(serverSocket, clientConnexion, clientNumber);
                }
                
            }));
            thread.Start();
        }

        /// <summary>
        /// Processes the response from the server.
        /// </summary>
        /// <param name="clientConnexion"></param>
        /// <param name="request"></param>
        bool ProcessResponse(Socket serverConnexion, Socket clientConnexion, int clientNumber)
        {
            // Receives the header of the response
            byte[] headerBytes;
            LogSys.LogInfo("Retrieving header...", clientNumber);
            Tools.SocketUtils.ReceiveStatus status = Tools.SocketUtils.ReceiveHeader(serverConnexion, out headerBytes, 30000);
            
            // If something went wrong, close the connection.
            if (status == Tools.SocketUtils.ReceiveStatus.Timeout || status == Tools.SocketUtils.ReceiveStatus.ClosedPrematurely || headerBytes.Length == 0)
            {
                LogSys.LogInfo("Failed to receive header from remote server.", clientNumber);
                LogSys.LogInfo("Server connection closed.", clientNumber);
                serverConnexion.Close();
                return false;
            }

            // Analyses the response
            HttpHeader header = HttpHeader.FromBytes(headerBytes);
            LogSys.LogEntry("[Verbose] Header received from remote server :\r\n" + HttpFrame.DisplayString(header.ToString()), clientNumber, Tools.LogVerbosity.Verbose);
            
            // Processed the header.
            PrepareRequest(header, clientNumber);
            LogSys.LogEntry("[Verbose] Processed header                   :\r\n" + HttpFrame.DisplayString(header.ToString()), clientNumber, Tools.LogVerbosity.Verbose);
            
            // Now processes the server response.
            string contentEncoding = header["Content-Encoding"];
            string contentLengthStr = header["Content-Length"];
            long contentLength = contentLengthStr == "default" ? -1 : long.Parse(contentLengthStr);
            bool isChunked = header["Transfer-Encoding"].ToLower().Contains("chunked");
            LogSys.LogInfo("Received file. " + (isChunked ? " Chunked transfer encoding." : "Size: " + contentLengthStr), clientNumber);



            // ------ FEATURE 8: Smart filtering : only filters text with human-readable encoding (no gzip/deflate) ------ 
            bool isText = header["Content-Type"].Contains("text/html") || header["Content-Type"].Contains("text/plain");
            bool isHumanReadable = (contentEncoding == "default" || contentEncoding == "utf-8");

            if (isText && isHumanReadable)
            {
                LogSys.LogInfo("Content check stage : Received text file (" + header["Content-Type"] + ").", clientNumber);

                byte[] data;
                // Use the best method to retrieve data without loss.
                if((isChunked) || (!isChunked && contentLength > 0))
                    data = Tools.SocketUtils.ReceiveData(serverConnexion, contentLength);
                else
                    data = Tools.SocketUtils.ReceiveBytes(serverConnexion);

                // Transforms the data into a string.
                string dataStr = Encoding.UTF8.GetString(data);

                // ------ FEATURE 4: Content filtering ------ 
                // Content filtering.
                if (m_filters.ContainsFilteredItems(dataStr))
                {
                    // Create redirection request
                    LogSys.LogInfo("Filter stage: filtered text file. Redirecting", clientNumber);
                    var request = StandardHttpResponses.RedirectTo(header.Version, "http://www.ida.liu.se/~TDTS04/labs/2011/ass2/error2.html");

                    // Send it.
                    LogSys.LogEntry("[Verbose] Sending frame to client : " + HttpFrame.DisplayString(request.ToString()), clientNumber, Tools.LogVerbosity.Verbose);
                    Tools.SocketUtils.SendBytes(
                        clientConnexion,
                        request.ToBytes());
                    LogSys.LogInfo("Successfully sent frame to client", clientNumber);
                }
                else
                {
                    // OK file
                    LogSys.LogInfo("Filter stage: OK text file. Forwarding.", clientNumber);

                    // If content is OK, forward it.
                    HttpFrame frame = new HttpFrame()
                    {
                        Header = header,
                        Data = data
                    };

                    // Forward text
                    LogSys.LogEntry("[Verbose] Sending frame to client : " + HttpFrame.DisplayString(frame.ToString()), clientNumber, Tools.LogVerbosity.Verbose);
                    if(!Tools.SocketUtils.SendBytes(clientConnexion, frame.ToBytes()))
                    {
                        LogSys.LogError("Error occured while sending bytes to client : the connexion has been closed.", clientNumber);
                        return false;
                    }
                    LogSys.LogInfo("Successfully sent frame to client", clientNumber);
                }
            }
            else // non-text file
            {
                // Forwarding of header
                if(!Tools.SocketUtils.SendBytes(clientConnexion, header.ToBytes()))
                {
                    LogSys.LogError("Error occured while sending bytes to client : the connexion has been closed.", clientNumber);
                    return false;
                }

                // ------ FEATURE 5: No size limit on HTTP data                 ------ 
                // ------ (see Tools.SocketUtils.ForwardData / ForwardBytes)    ------
                LogSys.LogInfo("Content check stage : Non text file. Forwarding.", clientNumber);
                if ((isChunked) || (!isChunked && contentLength > 0))
                    Tools.SocketUtils.ForwardData(serverConnexion, clientConnexion, contentLength);
                else
                    Tools.SocketUtils.ForwardBytes(serverConnexion, clientConnexion);
                
                LogSys.LogInfo("Forwarding complete.", clientNumber);
            }

            // Closes both connexions
            LogSys.LogInfo("Closing client connection.", clientNumber);
            clientConnexion.Close();
            LogSys.LogInfo("Closing server connection.", clientNumber);
            serverConnexion.Close();

            // Returns OK
            return true;
        }
        /// <summary>
        /// Prepares the request before sending it to the remove server.
        /// </summary>
        void PrepareRequest(HttpHeader frame, int clientNumber)
        {
            // ------ FEATURE 1: Handles both HTTP 1.0 and 1.1         -----
            // ------ FEATURE 6: Compatible with all major browsers    -----

            // Change the first lane (which might have been modified by the browser knowing he speaks to
            // a proxy.
            string remoteHost = frame["Host"].Split(':')[0];
            frame.RessourcePath = frame.RessourcePath.Replace("http://" + remoteHost, "");
            
            // Force connexion close in case of http 1.1
            if(frame.Version == HttpVersion.V1_1)
                frame["Connection"] = "close"; // force connexion close on server side.
            frame["Proxy-Connection"] = "close";
            // Forces utf8.
            if(m_forceUtf8)
            {
                frame["Accept-Encoding"] = "utf-8";
            }
        }
        /// <summary>
        /// Handles the connect requests (sends a not supported message).
        /// </summary>
        void HandleConnectMethod(HttpHeader browserHeader, Socket clientConnexion, int clientNumber)
        {
            // Creates and send a not supported response.
            HttpFrame notSupported = StandardHttpResponses.Unsupported(browserHeader.Version);
            Tools.SocketUtils.SendBytes(
                clientConnexion,
                notSupported.ToBytes());

            LogSys.LogInfo("This proxy doesn't support SSL tunneling via CONNECT", clientNumber);
            LogSys.LogVerbose("[Verbose] Sending frame to client: \r\n" + HttpFrame.DisplayString(notSupported.ToString()), clientNumber);

            // Closes the connexion.
            clientConnexion.Close();
            LogSys.LogInfo("Client connection closed.", clientNumber);
        }
        /// <summary>
        /// Handles a request with invalid URL.
        /// </summary>
        void HandleInvalidURL(HttpHeader browserRequest, Socket clientConnexion, string url, int clientNumber)
        {
            // Send redirection
            HttpFrame redirectFrame = StandardHttpResponses.RedirectTo(browserRequest.Version, "http://www.ida.liu.se/~TDTS04/labs/2011/ass2/error1.html");
            Tools.SocketUtils.SendBytes(
                clientConnexion,
                redirectFrame.ToBytes());


            LogSys.LogInfo("Filtered url " + url + ", closing connection.", clientNumber);
            LogSys.LogVerbose("[Verbose] Sending frame to client: \r\n" + HttpFrame.DisplayString(redirectFrame.ToString()), clientNumber);

            // Closes the connexion.
            clientConnexion.Close();
            LogSys.LogInfo("Client connection closed.", clientNumber);
        }
        /// <summary>
        /// Sends post data to the remote server.
        /// </summary>
        void SendPostData(HttpHeader browserRequest, Socket clientConnexion, Socket serverConnexion, int clientNumber)
        {
            LogSys.LogInfo("Sending POST data.", clientNumber);

            string contentLengthStr = browserRequest["Content-Length"];
            long contentLength = contentLengthStr == "default" ? -1 : long.Parse(contentLengthStr);
            bool isChunked = browserRequest["Transfer-Encoding"].ToLower().Contains("chunked");

            // No size limit with ForwardData / ForwardBytes.
            if ((isChunked) || (!isChunked && contentLength > 0))
                Tools.SocketUtils.ForwardData(clientConnexion, serverConnexion, contentLength);
            else
                Tools.SocketUtils.ForwardBytes(clientConnexion, serverConnexion);

            LogSys.LogInfo("POST data successfully sent.", clientNumber);
        }
        /// <summary>
        /// Creates a socket to connect to the Remove server.
        /// </summary>
        /// <param name="frame">The frame used to extract adress/port information.</param>
        /// <returns></returns>
        Socket CreateSocketToRemoveServer(HttpHeader frame, int clientNumber)
        {
            // host and port to connect to.
            string host;
            int port = 0;
            IPAddress address;

            // This string might be "www.example.com:443".
            string frameHostStr = frame["Host"];
            Tools.SocketUtils.GetHostInformation(frameHostStr, out host, out port);

            // Gets the ip address using dns.
            IPAddress[] addresses;
            try
            {
                addresses = Dns.GetHostAddresses(host);
            }
            catch
            {
                LogSys.LogError("Could not resolve host name: " + host, clientNumber);
                return null;
            }
            // Uses the first address.
            address = addresses[0];


            // Creates the socket
            LogSys.LogInfo("Connection to remote server : " + host + ":" + port + " using address :" + address.ToString() + ".", clientNumber);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                sock.Connect(new IPEndPoint(address, port));
            }
            catch (SocketException e)
            {
                LogSys.LogError("Failed to connect to remote server. Reason: " + e.SocketErrorCode + " (Code: " + e.NativeErrorCode + ").", clientNumber);
                return null;
            }
            LogSys.LogInfo("Connected to remote server.", clientNumber);

            return sock;
        }
        
        #endregion

        #endregion
    }
}
