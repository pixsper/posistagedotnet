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
using System.Threading;
using System.Threading.Tasks;

namespace Imp.PosiStageDotNet
{
	internal sealed class Timer : CancellationTokenSource, IDisposable
	{
		public Timer(Action callback, int dueTime, int period)
		{
			Task.Delay(dueTime, Token).ContinueWith(async (t, s) =>
			{
				var action = (Action)s;

				while (true)
				{
					if (IsCancellationRequested)
						break;

#pragma warning disable 4014
					Task.Run(() => action());
#pragma warning restore 4014

					await Task.Delay(period).ConfigureAwait(true);
				}
			}, callback, CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
				TaskScheduler.Default);
		}

		public new void Dispose()
		{
			Cancel();
		}
	}
}