using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TF.Signal
{
    internal sealed class SignalWave
    {
        private readonly List<Func<ISignal, CancellationToken, UniTask>> subscribers = new();

        public void AddSubscriber(Func<ISignal, CancellationToken, UniTask> subscriber)
        {
            lock (subscribers)
            {
                subscribers.Add(subscriber);
            }
        }

        public void RemoveSubscriber(Func<ISignal, CancellationToken, UniTask> subscriber)
        {
            lock (subscribers)
            {
                subscribers.Remove(subscriber);
            }
        }

        public UniTask FireAllAsync(ISignal signal, CancellationToken cancellation = default)
        {
            UniTask[] buffer;
            lock (subscribers)
            {
                buffer = CappedArrayPool<UniTask>.Shared8Limit.Rent(subscribers.Count);
                for (var i = 0; i < subscribers.Count; i++)
                {
                    buffer[i] = subscribers[i](signal, cancellation);
                }
            }

            try
            {
                return UniTask.WhenAll(buffer);
            }
            finally
            {
                CappedArrayPool<UniTask>.Shared8Limit.Return(buffer);
            }
        }
    }
}