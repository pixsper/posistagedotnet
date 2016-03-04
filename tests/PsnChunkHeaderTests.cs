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