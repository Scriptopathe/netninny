// Author: Josué Alvarez

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace NetNinny.Tools
{
    /// <summary>
    /// Contains a set of methods for dealing with sockets.
    /// </summary>
    public static class SocketUtils
    {
        #region Constants
        public const int RESPONSE_TIMEOUT = 5000;
        #endregion

        /// <summary>
        /// Forwards byte from one socket to another.
        /// This function does not buffer the data, allowing large amount of bytes 
        /// to be transferred seamlessly.
        /// 
        /// This function should be used only in cases when we can't know when the data received ends.
        /// If Content-Length is given, or chunked encoding, use ForwardData.
        /// </summary>
        /// <param name="from">Socket from which the bytes will be read.</param>
        /// <param name="to">Socket to which the read bytes will be sent.</param>
        public static void ForwardBytes(Socket from, Socket to, int timeout=RESPONSE_TIMEOUT)
        {
            to.ReceiveTimeout = timeout;
            // Buffer used to store bytes for a short amount of time
            byte[] buffer = new byte[100000];
            int receivedBytesCount = 0;
            try
            {
                do
                {
                    receivedBytesCount = from.Receive(buffer);
                    SendBytes(to, buffer, receivedBytesCount);
                } while (receivedBytesCount != 0);
            }
            catch(SocketException e)
            {
                // Timeout
                /*if (e.ErrorCode != 10060)
                    throw e;*/
            }
        }

        /// <summary>
        /// Forwards data from one socket to another.
        /// This function does not buffer the data, allowing large amount of bytes 
        /// to be transferred seamlessly.
        /// This function manages the content-length property or chunked encoding, ensuring
        /// that all bytes will be received or sent.
        /// </summary>
        /// <param name="from">The socket from where to receive the bytes.</param>
        /// <param name="timeout">Timeout for the receive function, after which it will stop receiving bytes.</param>
        /// <returns></returns>
        public static void ForwardData(Socket from, Socket to, long contentLength, int timeout = RESPONSE_TIMEOUT)
        {
            int bytesReceivedCount = 0; // number of bytes received by each call to receive.
            byte[] buffer = new byte[256]; // buffer used for each call of receive.

            // If not chunked encoding
            if (contentLength > 0)
            {
                long totalBytes = 0;
                do
                {
                    bytesReceivedCount = from.Receive(buffer, buffer.Length, SocketFlags.None);
                    
                    // Avoid high CPU Usage.
                    if (bytesReceivedCount == 0)
                    {
                        System.Threading.Thread.Sleep(1);
                        continue;
                    }

                    SendBytes(to, buffer, bytesReceivedCount);
                    totalBytes += bytesReceivedCount;
                } while (totalBytes != contentLength); //(bytesReceivedCount > 0);
            }
            else
            {
                while (true)
                {
                    int expectedBytes = 0;
                    int totalBytes = 0;
                    byte[] buff = new byte[1];
                    List<byte> tmpBuff = new List<byte>();
                    // We read bytes until line feed.
                    do
                    {
                        bytesReceivedCount = from.Receive(buff, buff.Length, SocketFlags.None);

                        // Avoids high cpu usage.
                        if(bytesReceivedCount == 0)
                        {
                            System.Threading.Thread.Sleep(1);
                            continue;
                        }
                        
                        tmpBuff.Add(buff[0]);
                        
                    } while (buff[0] != 0x0A); // stop at line feed.


                    // End of reception.
                    if (bytesReceivedCount == 0)
                        break;

                    // Sends the temporary buffer
                    byte[] tmpBuffArray = tmpBuff.ToArray();
                    SendBytes(to, tmpBuffArray);

                    // Gets the expected number of bytes
                    string tmp = Encoding.UTF8.GetString(tmpBuffArray);
                    expectedBytes = Int32.Parse(tmp, System.Globalization.NumberStyles.HexNumber);

                    if (expectedBytes != 0)
                    {

                        // We read the number of bytes expected.
                        do
                        {
                            bytesReceivedCount = from.Receive(buffer, Math.Min(buffer.Length, expectedBytes - totalBytes), SocketFlags.None);
                            
                            // Avoid High CPU usage.
                            if (bytesReceivedCount == 0)
                            {
                                System.Threading.Thread.Sleep(1);
                                continue;
                            }

                            totalBytes += bytesReceivedCount;
                            // Forward the bytes.
                            SendBytes(to, buffer, bytesReceivedCount);
                        } while (totalBytes != expectedBytes);
                    }

                    // Skip line feed.
                    do
                    {
                        bytesReceivedCount =  from.Receive(buff, buff.Length, SocketFlags.None);

                        // Avoids high cpu usage.
                        if (bytesReceivedCount == 0)
                        {
                            // In case of slow network, the receive functions returns 0 for some amount of time
                            // This prevents fast looping (and high CPU consumption).
                            System.Threading.Thread.Sleep(1);
                            continue;
                        }

                        SendBytes(to, buff, bytesReceivedCount);
                    } while (buff[0] != 0x0A);

                    if (expectedBytes == 0)
                    {
                        break;
                    }

                }
            }
        }
        /// <summary>
        /// Gets information name and port information about an host given a string in the standart host:port format.
        /// </summary>
        /// <param name="host">The host to parse.</param>
        /// <param name="hostname">OUT : the host name.</param>
        /// <param name="hostport">OUT : the host port number.</param>
        public static void GetHostInformation(string host, out string hostname, out int hostport)
        {
            if (host.Contains(":"))
            {
                // If the port is given in the host name, use this port.
                string[] tmp = host.Split(':');
                hostname = tmp[0];
                hostport = Int32.Parse(tmp[1]);
            }
            else
            {
                // If it's not given, use default http port (80).
                hostname = host;
                hostport = 80;
            }
        }
        
        /// <summary>
        /// Describes the status of the receive method.
        /// </summary>
        public enum ReceiveStatus
        {
            OK,
            ClosedPrematurely,
            Timeout
        }

        /// <summary>
        /// Receives the header bytes.
        /// This function will wait untill it encounters \r\n\r\n, or stop if a timeout value expired.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="timeout"></param>
        /// <returns>OK if the header was successfully transferred, Timeout or ClosedPrematurly otherwise.</returns>
        public static ReceiveStatus ReceiveHeader(Socket socket, out byte[] headerBytes, int timeout =RESPONSE_TIMEOUT)
        {
            // Represents \r\n\r\n
            const int CRLFCRLF = 0x0D0A0D0A;

            List<byte> buff = new List<byte>();
            byte[] byteReceived = new byte[1];
            int bytesReceivedCount;
            int last4Bytes = 0;
            headerBytes = new byte[0];

            // Timeout after which the socket will throw an exception.
            socket.ReceiveTimeout = timeout;
            try
            {

                do
                {
                    // Get bytes one by one.
                    bytesReceivedCount = socket.Receive(byteReceived);
                    if (bytesReceivedCount == 0)
                    {
                        System.Threading.Thread.Sleep(1);
                        continue;
                    }

                    // Shifts the last for bytes and add the last byte.
                    // If we get \r\n\r\n, last4bytes will be equal to CRLFCRLF
                    last4Bytes = (last4Bytes << 8) | byteReceived[0];

                    // Add the byte to the buffer.
                    buff.Add(byteReceived[0]);

                } while (last4Bytes != CRLFCRLF);
                headerBytes = buff.ToArray();
                
            }
            catch(SocketException e)
            {
                return ReceiveStatus.Timeout;
            }

            if(bytesReceivedCount == 0)
            {
                // End of connexion before end of request.
                return ReceiveStatus.ClosedPrematurely;
            }

            return ReceiveStatus.OK;
        }
        /// <summary>
        /// Receives all bytes from a socket.
        /// </summary>
        /// <param name="socket">The socket from where to receive the bytes.</param>
        /// <param name="timeout">Timeout for the receive function, after which it will stop receiving bytes.</param>
        /// <returns></returns>
        public static byte[] ReceiveBytes(Socket socket, int timeout = RESPONSE_TIMEOUT)
        {
            socket.ReceiveTimeout = timeout;
            
            int bytesReceivedCount; // number of bytes received by each call to receive.
            byte[] buffer = new byte[256]; // buffer used for each call of receive.
            List<byte[]> buffers = new List<byte[]>(); // all the buffers we got from the receive function.

            do
            {
                try
                {
                    bytesReceivedCount = socket.Receive(buffer, buffer.Length, SocketFlags.None);
                    if (bytesReceivedCount != 0)
                        buffers.Add(Tools.ArrayUtils.NewFromArray(buffer, bytesReceivedCount));

                }
                catch (SocketException ex)
                {
                    // If the socket times out, the we don't have anything more to receive.
                    if (ex.ErrorCode == 10060) // timeout
                        break;
                    else
                    {
                        return new byte[0];
                        //  throw new Exception();

                    }
                }

            } while (bytesReceivedCount == buffer.Length); //(bytesReceivedCount > 0);

            return Tools.ArrayUtils.Merge(buffers);
        }
        /// <summary>
        /// Receives the data coming from a http frame.
        /// Takes into consideration the field "Content-Length", or chunked encoding to ensure all the needed data
        /// has been transfered.
        /// </summary>
        /// <param name="socket">The socket from where to receive the bytes.</param>
        /// <param name="timeout">Timeout for the receive function, after which it will stop receiving bytes.</param>
        /// <returns></returns>
        public static byte[] ReceiveData(Socket socket, long contentLength, int timeout = RESPONSE_TIMEOUT)
        {
            int bytesReceivedCount = 0; // number of bytes received by each call to receive.
            byte[] buffer = new byte[256]; // buffer used for each call of receive.
            List<byte[]> buffers = new List<byte[]>(); // all the buffers we got from the receive function.

            // If not chunked encoding
            if (contentLength > 0)
            {
                long totalBytes = 0;
                do
                {
                    bytesReceivedCount = socket.Receive(buffer, buffer.Length, SocketFlags.None);
                    totalBytes += bytesReceivedCount;

                    // Avoids High CPU usage.
                    if (bytesReceivedCount == 0)
                    {
                        System.Threading.Thread.Sleep(1);
                        continue;
                    }
                    
                    buffers.Add(Tools.ArrayUtils.NewFromArray(buffer, bytesReceivedCount));

                } while (totalBytes != contentLength); //(bytesReceivedCount > 0);
            }
            else
            {
                while (true)
                {
                    int expectedBytes = 0;
                    int totalBytes = 0;
                    byte[] buff = new byte[1];
                    List<byte> tmpBuff = new List<byte>();
                    // We read bytes until line feed.
                    do
                    {
                        bytesReceivedCount = socket.Receive(buff, buff.Length, SocketFlags.None);
                        
                        // Avoids high cpu usage.
                        if(bytesReceivedCount == 0)
                        {
                            System.Threading.Thread.Sleep(1);
                            continue;
                        }

                        tmpBuff.Add(buff[0]);
                        buffers.Add(new byte[1] { buff[0] });
                        
                    } while (buff[0] != 0x0A); // stop at line feed.


                    // End of reception.
                    if (bytesReceivedCount == 0)
                        break;

                    // Gets the expected number of bytes
                    string tmp = Encoding.UTF8.GetString(tmpBuff.ToArray());
                    expectedBytes = Int32.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
                    if (expectedBytes != 0)
                    {
                        // We read the number of bytes expected.
                        do
                        {
                            bytesReceivedCount = socket.Receive(buffer, Math.Min(buffer.Length, expectedBytes - totalBytes), SocketFlags.None);
                            totalBytes += bytesReceivedCount;

                            // Avoids high CPU usage.
                            if(bytesReceivedCount == 0)
                            {
                                // In case of slow network, the receive functions returns 0 for some amount of time
                                // This prevents fast looping (and high CPU consumption).
                                System.Threading.Thread.Sleep(1);
                                continue;
                            }
                            
                            buffers.Add(Tools.ArrayUtils.NewFromArray(buffer, bytesReceivedCount));
                            
                        } while (totalBytes != expectedBytes);
                    }

                    // Skip line feed.
                    do
                    {
                        bytesReceivedCount = socket.Receive(buff, buff.Length, SocketFlags.None);

                        // Avoids High CPU usage.
                        if(bytesReceivedCount == 0)
                        {
                            System.Threading.Thread.Sleep(1);
                            continue;
                        }
                        
                        buffers.Add(new byte[1] { buff[0] });

                    } while (buff[0] != 0x0A);

                    if (expectedBytes == 0)
                    {
                        break;
                    }
                }
            }
            return Tools.ArrayUtils.Merge(buffers);
        }
        /// <summary>
        /// Send the given bytes to the given socket.
        /// </summary>
        /// <param name="socket">The socket where to send the bytes.</param>
        /// <param name="bytes">Bytes to send.</param>
        /// <param name="length">If lower than 0 : send all bytes, else, send the given amount of bytes.</param>
        /// <returns>Returns true if the bytes were sent, false if an error occurred.</returns>
        public static bool SendBytes(Socket socket, byte[] bytes, int length=-1)
        {
            if (length < 0)
                length = bytes.Length;

            int bytesSent;
            int totalBytesSent = 0;

            try
            {
                do
                {
                    bytesSent = socket.Send(bytes, totalBytesSent, length - totalBytesSent, SocketFlags.None);
                    totalBytesSent += bytesSent;
                } while (totalBytesSent != length);
            }
            catch (Exception e) { return false; }
            return true;
        }

    }
}
