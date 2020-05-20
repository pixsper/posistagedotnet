using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DBDesign.PosiStageDotNet.Networking;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBDesign.PosiStageDotNet.Tests
{
    [TestClass]
    public class UdpServiceTests
    {
        public const int UdpPort = 2710;

        [TestMethod]
        public async Task CanReceivePacket()
        {
            var ep = new IPEndPoint(IPAddress.Loopback, UdpPort);
            var service = new UdpService(ep);

            service.LocalEndPoint.Should().Be(ep, "because this value was set in the constructor");
            service.MulticastGroups.Should().BeEmpty("because no multicast groups were passed in the constructor");

            var data = System.Text.Encoding.UTF8.GetBytes("Test");
            var host = new UdpClient();

            service.PacketReceived += (s, e) =>
                e.Buffer.Should().BeEquivalentTo(data, "because we should receive the test data exactly");

            using (var monitoredService = service.Monitor())
            {
                service.StartListening();

                int bytesSent = await host.SendAsync(data, data.Length, ep);
                bytesSent.Should().Be(data.Length, "because the test data is this long");

                // Allow some time to receive the packet
                await Task.Delay(100);

                monitoredService.Should().Raise(nameof(service.PacketReceived), "because the event should be raised upon receiving a packet");
            }

            host.Dispose();

            service.Dispose();
        }

        [TestMethod]
        public async Task CanReceiveMulticastPacket()
        {
            var multicastIp = IPAddress.Parse("239.0.0.1");
            var ep = new IPEndPoint(IPAddress.Loopback, UdpPort);
            var service = new UdpService(ep);
            service.JoinMulticastGroup(multicastIp);

            service.LocalEndPoint.Should().Be(ep, "because this value was set in the constructor");
            service.MulticastGroups.Should().BeEquivalentTo(new[] { multicastIp }, "because this value was set in the constructor");

            var data = System.Text.Encoding.UTF8.GetBytes("Test");
            var host = new UdpClient();

            service.PacketReceived += (s, e) =>
                e.Buffer.Should().BeEquivalentTo(data, "because we should receive the test data exactly");

            using (var monitoredService = service.Monitor())
            {
                service.StartListening();

                int bytesSent = await host.SendAsync(data, data.Length, new IPEndPoint(multicastIp, ep.Port));
                bytesSent.Should().Be(data.Length, "because the test data is this long");

                // Allow some time to receive the packet
                await Task.Delay(100);

                monitoredService.Should().Raise(nameof(service.PacketReceived), "because the event should be raised upon receiving a packet");
            }

            host.Dispose();

            service.Dispose();
        }

        [TestMethod]
        public void CanManageListeningState()
        {
            var service = new UdpService(new IPEndPoint(IPAddress.Loopback, UdpPort));

            service.IsListening.Should().BeFalse("because the service is not listening");

            service.Invoking(s => s.StopListeningAsync())
                .Should().Throw<InvalidOperationException>("because the service is not listening");

            service.StartListening();

            service.IsListening.Should().BeTrue("because the service is listening");

            service.Invoking(s => s.StartListening())
                .Should().Throw<InvalidOperationException>("because the service is already listening");

            service.Dispose();
        }
    }
}
