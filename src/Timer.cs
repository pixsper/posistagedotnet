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
			base.Cancel();
		}
	}
}
