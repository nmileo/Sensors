using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.SPOT;

namespace Sensors
{
    public class PachubeSockets
    {
        private const string apiKey = "S1lxnWOv4P-bh5t2yig9oZPVgnBeAo4DubGLG09ppgk";
        private const string feedId = "25788";

        public bool WriteToPachube(string sample)
        {
            bool result = false;
            Socket connection = null;

            try
            {
                connection = Connect("api.pachube.com", 5000);
            }
            catch(Exception ex)
            {
                Debug.Print("Connection error");
                Debug.Print(ex.Message);
            }

            if (connection != null)
            {
                try
                {
                    Debug.Print("Sending " + sample);
                    SendRequest(connection, apiKey, feedId, sample);
                    Debug.Print("Sent");
                    result = true;
                }
                catch (SocketException ex)
                {
                    Debug.Print("Send error");
                    Debug.Print(ex.Message);
                }
                finally
                {
                    connection.Close();
                    connection = null;
                }
            }
            return result;
        }

        private Socket Connect(string host, int timeout)
        {
            // look up host's domain name, to find IP address(es)
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            // extract a returned address
            IPAddress hostAddress = hostEntry.AddressList[0];
            IPEndPoint remoteEndPoint = new IPEndPoint(hostAddress, 80);

            // connect!
            Debug.Print("Connecting to " + hostAddress + " over port 80");
            var connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connection.Connect(remoteEndPoint);
            connection.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            connection.SendTimeout = timeout;
            Debug.Print("Connected!");
            return connection;
        }

        private void SendRequest(Socket s, string apiKey, string feedId, string content)
        {
            byte[] contentBuffer = Encoding.UTF8.GetBytes(content);
            const string CRLF = "\r\n";
            var requestLine = "PUT /v2/feeds/" + feedId + ".csv HTTP/1.1" + CRLF;
            byte[] requestLineBuffer = Encoding.UTF8.GetBytes(requestLine);
            var headers =
                "Host: api.pachube.com" + CRLF +
                "X-PachubeApiKey: " + apiKey + CRLF +
                "Content-Type: text/csv" + CRLF +
                "Content-Length: " + contentBuffer.Length + CRLF +
                CRLF;
            byte[] headersBuffer = Encoding.UTF8.GetBytes(headers);
            s.Send(requestLineBuffer);
            s.Send(headersBuffer);
            s.Send(contentBuffer);
        }
    }
}