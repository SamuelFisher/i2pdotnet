// This file is part of i2pdotnet.
// Copyright (c) 2016
//  
// i2pdotnet is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, version 3.
//  
// i2pdotnet is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//  
// You should have received a copy of the GNU Lesser General Public License
// along with i2pdotnet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace I2PNet
{
    public class I2PSession : II2PSession
    {
        private readonly int sessionId;
        private readonly I2PSamConnection controlSocket;
        
        private bool isListeningForIncomingConnections;

        public I2PSession(int samPort, int? listenPort = null)
        {
            SamPort = samPort;
            ListenPort = listenPort;
            sessionId = new Random().Next();
            controlSocket = new I2PSamConnection(SamPort);
        }

        public int SamPort { get; }

        public int? ListenPort { get; }

        public string LocalDestination { get; private set; }

        public event AcceptConnectionDelegate IncomingConnection;

        public async Task InitializeAsync()
        {
            await controlSocket.Connect();
            var response = await controlSocket.SendCommand($"SESSION CREATE STYLE=STREAM ID={sessionId} DESTINATION=TRANSIENT");
            LocalDestination = response["DESTINATION"];
        }

        public async Task<Stream> ConnectAsync(string remoteDestination)
        {
            var connectionStream = new I2PSamConnection(SamPort);
            await connectionStream.Connect();
            await connectionStream.SendCommand($"STREAM CONNECT ID={sessionId} DESTINATION={remoteDestination}");
            return connectionStream.GetStream();
        }

        public async Task ListenForIncomingConnectionsAsync()
        {
            if (isListeningForIncomingConnections)
                throw new InvalidOperationException("Already listening for incoming connections.");

            if (!ListenPort.HasValue)
                throw new InvalidOperationException("No listen port specified.");

            var listener = new TcpListener(IPAddress.Loopback, ListenPort.Value);
            isListeningForIncomingConnections = true;
            listener.Start();

            var streamForwarder = new I2PSamConnection(SamPort);
            await streamForwarder.Connect();
            await streamForwarder.SendCommand($"STREAM FORWARD ID={sessionId} PORT={ListenPort}");

            ListenForConnections(listener);
        }

        private void ListenForConnections(TcpListener listener)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    while (isListeningForIncomingConnections)
                    {
                        var client = await listener.AcceptTcpClientAsync();

                        var reader = new StreamReader(client.GetStream());
                        var status = reader.ReadLine();
                        string remoteDestination = status.Split(' ').First();

                        IncomingConnection?.Invoke(this, new AcceptConnectionEventArgs(client, remoteDestination));
                    }
                }
                finally
                {
                    listener.Stop();
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void StopListeningForIncomingConnections()
        {
            if (!isListeningForIncomingConnections)
                throw new InvalidOperationException("Not currently listening for incoming connections.");

            isListeningForIncomingConnections = false;
        }

        public async Task<string> NameLookupAsync(string address)
        {
            var result = await controlSocket.SendCommand($"NAMING LOOKUP NAME={address}");
            return result["VALUE"];
        }

        public void Dispose()
        {
            isListeningForIncomingConnections = false;
            controlSocket.Dispose();
        }
    }
}
