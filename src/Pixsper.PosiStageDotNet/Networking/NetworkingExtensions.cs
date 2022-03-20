// Copyright (c) 2022 Pixsper Ltd. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Net;
using System.Net.Sockets;

namespace Pixsper.PosiStageDotNet.Networking;

internal static class NetworkingExtensions
{
	public static bool IsIPv4Multicast(this IPAddress ipAddress)
	{
		if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
			return false;

		var ipBytes = ipAddress.GetAddressBytes();
		return ipBytes[0] >= 224 && ipBytes[1] <= 239;
	}
}