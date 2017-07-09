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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using I2PNet;

namespace SampleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting session1...");

            var session1 = new I2PSession(samPort: 7656);
            session1.InitializeAsync().Wait();

            Console.WriteLine("Looking up stats.i2p");
            Console.WriteLine(session1.NameLookupAsync("stats.i2p").Result);

            Console.WriteLine("Starting session 2...");

            var session2 = new I2PSession(7656, listenPort: 5001);
            session2.InitializeAsync().Wait();

            session2.IncomingConnection += Session2OnIncomingConnection;

            Console.WriteLine("Setting up listening on session 2");
            session2.ListenForIncomingConnectionsAsync().Wait();

            Task.Delay(1000).Wait();

            Console.WriteLine("Connecting from session 1 to session 2");

            var stream = session1.ConnectAsync(session2.LocalDestination).Result;
            var writer = new BinaryWriter(stream);
            writer.Write("Testing 123...\n");

            Task.Delay(1000).Wait();

            while (true)
            {
                Console.Write("# ");
                string message = Console.ReadLine();
                if (message == null)
                    return;
                writer.Write(message);
            }
        }

        private static void Session2OnIncomingConnection(II2PSession sender, AcceptConnectionEventArgs e)
        {
            var reader = new BinaryReader(e.Client.GetStream());
            while (true)
                Console.WriteLine(">> Received: " + reader.ReadString());
        }
    }
}
