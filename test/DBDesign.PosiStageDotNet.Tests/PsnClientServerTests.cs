using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBDesign.PosiStageDotNet.Tests
{
    [TestClass]
    public class PsnClientServerTests
    {
        [TestMethod]
        public async Task CanSendAndReceivePsnPackets()
        {
            var server = new PsnServer("Test System", IPAddress.Loopback);

            var trackers = new[]
            {
                new PsnTracker(0, "Tracker Foo", Tuple.Create(1f, 2f, 3f), timestamp: 2452352452),
                new PsnTracker(1, "Tracker Bar", Tuple.Create(4f, 5f, 6f), Tuple.Create(7f, 8f, 9f))
            }.ToList();

            server.SetTrackers(trackers);

            var client = new PsnClient(IPAddress.Loopback);
            client.StartListening();

            using (var monitoredClient = client.Monitor())
            {
                server.SendData();
                server.SendInfo();

                await Task.Delay(100);

                monitoredClient.Should().Raise(nameof(PsnClient.DataPacketReceived));
                monitoredClient.Should().Raise(nameof(PsnClient.InfoPacketReceived));
            }

            var receivedTackers = client.Trackers.Values.ToList();

            receivedTackers.Should().BeEquivalentTo(trackers, options => 
                options.Excluding(o => o.DataLastReceived).Excluding(o => o.InfoLastReceived));

            client.Dispose();
            server.Dispose();
        }
    }
}
