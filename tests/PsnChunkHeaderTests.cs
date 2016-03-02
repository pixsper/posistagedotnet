using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imp.PosiStageDotNet.Tests
{
	[TestClass]
	public class PsnChunkHeaderTests
	{
		[TestMethod]
		public void CanConvertToInt32AndBack()
		{
			var header1 = new PsnChunkHeader(56, 63, true);

			uint value = header1.ToUInt32();

			var header2 = PsnChunkHeader.FromUInt32(value);

			header1.Should().Be(header2, "because converting from an int and back should produce the same value");
		}
	}
}
