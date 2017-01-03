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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace I2PNet
{
    internal class I2PSamConnection : IDisposable
    {
        private readonly int samPort;

        private TcpClient client;
        private StreamReader reader;
        private BinaryWriter writer;

        public I2PSamConnection(int samPort)
        {
            this.samPort = samPort;
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public NetworkStream GetStream() => client.GetStream();

        public async Task Connect()
        {
            client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, samPort);

            var stream = client.GetStream();

            writer = new BinaryWriter(stream);
            reader = new StreamReader(stream);

            await SendCommand("HELLO VERSION");
        }

        public Task<IDictionary<string, string>> SendCommand(string command)
        {
            Console.WriteLine("> " + command);

            writer.Write(Encoding.UTF8.GetBytes(command + "\n"));

            return Task.Run(() =>
            {
                var responseLine = reader.ReadLine();

                Console.WriteLine(responseLine);

                var response = responseLine.Split(' ');
                var responseDict = response.Skip(2)
                                            .Select(x => x.Split('='))
                                            .ToDictionary(x => x[0], x => x.Length < 2 ? x[0] : x[1]);

                responseDict["COMMAND"] = response[0];
                responseDict["METHOD"] = response[1];

                var result = responseDict["RESULT"];

                if (result != "OK")
                    throw new I2PException("Failed response to: " + command, result, responseLine);

                return (IDictionary<string, string>)responseDict;
            });
        }
    }
}
