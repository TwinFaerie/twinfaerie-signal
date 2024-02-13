using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TF.Signal
{
    internal class SubscriptionSignal : IDisposable
    {
        private readonly Broadcaster parent;
        private readonly Type type;
        private readonly Func<ISignal, CancellationToken, UniTask> subscriber;

        public SubscriptionSignal(Broadcaster parent, Type type, Func<ISignal, CancellationToken, UniTask> subscriber)
        {
            this.parent = parent;
            this.type = type;
            this.subscriber = subscriber;
        }

        public void Dispose()
        {
            SignalWave wave = null;
            
            lock (parent.waveDictionary)
            {
                if (parent.waveDictionary.TryGetValue(type, out var entry))
                {
                    wave = entry;
                }
            }

            wave?.RemoveSubscriber(subscriber);
        }
    }
}