// This file is part of PosiStageDotNet.
// 
// PosiStageDotNet is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// PosiStageDotNet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with PosiStageDotNet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;

namespace DBDesign.PosiStageDotNet.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(new string('*', Console.WindowWidth - 1));
            Console.WriteLine("DBDesign.PosiStageDotNet.Client Example");
            Console.WriteLine(new string('*', Console.WindowWidth - 1));
            Console.WriteLine();

            PsnClient client;

            switch (args.Length)
            {
                case 0:
                    client = new PsnClient();
                    Console.WriteLine(
                        $"Listening on default multicast IP '{PsnClient.DefaultMulticastIp}', default port {PsnClient.DefaultPort}");
                    break;

                case 1:
                case 2:
                {
                    IPAddress ip;

                    if (!IPAddress.TryParse(args[0], out ip))
                    {
                        Console.WriteLine($"Invalid IP address value '{args[0]}'");
                        return;
                    }

                    int port;

                    if (args.Length == 2)
                    {
                        if (!int.TryParse(args[1], out port))
                        {
                            Console.WriteLine($"Invalid UDP port value '{args[1]}'");
                            return;
                        }

                        if (port < ushort.MinValue + 1 || port > ushort.MaxValue)
                        {
                            Console.WriteLine($"UDP port value out of valid range '{args[1]}'");
                            return;
                        }
                    }
                    else
                    {
                        port = PsnClient.DefaultPort;
                    }

                    client = new PsnClient(ip, port);
                    Console.WriteLine(
                        $"Listening on custom multicast IP '{PsnClient.DefaultMulticastIp}', custom port {PsnClient.DefaultPort}");
                }
                    break;

                default:
                    Console.WriteLine(
                        "Invalid args. Format is 'Imp.PosiStageDotNet.Client [CustomMulticastIP] [CustomPort]");
                    Console.WriteLine("E.g. 'Imp.PosiStageDotNet.Client 236.10.10.10 56565");
                    return;
            }

            Console.WriteLine("Press any key to exit");
            Console.WriteLine("");
            Console.WriteLine(new string('*', Console.WindowWidth - 1));
            Console.WriteLine("");

	        client.StartListening();

            while (!Console.KeyAvailable)
            {
                if (client.Trackers.Any())
                {
                    Console.WriteLine(new string('*', Console.WindowWidth - 1));
                    Console.WriteLine("");

                    foreach (var pair in client.Trackers)
                    {
                        Console.WriteLine(pair.Value);
                        Console.WriteLine("");
                    }

                    Console.WriteLine(new string('*', Console.WindowWidth - 1));
                    Console.WriteLine("");
                }

                Thread.Sleep(1000);
            }

            Console.ReadKey();

            Console.WriteLine("");
            Console.WriteLine(new string('*', Console.WindowWidth - 1));
            Console.WriteLine("");

            Console.WriteLine("Disposing client...");

            client.Dispose();

            Console.WriteLine("Client disposed. Exiting...");
            Console.WriteLine("");
            Console.WriteLine(new string('*', Console.WindowWidth - 1));
            Console.WriteLine("");
        }
    }
}