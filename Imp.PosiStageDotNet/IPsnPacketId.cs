using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imp.PosiStageDotNet
{
	public interface IPsnPacketId
	{
		PsnChunkId Id { get; }
	}
}
