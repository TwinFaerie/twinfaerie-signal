using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TF.Signal
{
    public class Broadcaster : IDisposable
    {
        public static Broadcaster Default = new();
        
        private bool isDisposed;
        
        internal readonly Dictionary<Type, SignalWave> waveDictionary = new();

        public void Dispose()
        {
            lock (waveDictionary)
            {
                waveDictionary.Clear();
                isDisposed = true;
            }
        }

        public UniTask PublishAsync<T>(T signal, CancellationToken cancellation = default) where T : ISignal
        {
            SignalWave wave = null;
            
            lock (waveDictionary)
            {
                if (isDisposed)
                { throw new ObjectDisposedException("AsyncMessageBus"); }

                if (waveDictionary.TryGetValue(typeof(T), out var entry))
                {
                    wave = entry;
                }
            }

            if (wave == null)
                return UniTask.CompletedTask;
            
            return wave.FireAllAsync(signal, cancellation);
        }

        public IDisposable Subscribe(Type type, Func<ISignal, CancellationToken, UniTask> action, CancellationToken cancellationToken = default)
        {
            SignalWave wave;
            
            lock (waveDictionary)
            {
                if (isDisposed)
                { throw new ObjectDisposedException("AsyncMessageBus"); }

                if (waveDictionary.TryGetValue(type, out var entry))
                {
                    wave = entry;
                }
                else
                {
                    wave = new SignalWave();
                    waveDictionary.Add(type, wave);
                }
                
                wave.AddSubscriber(action);
            }
            
            return new SubscriptionSignal(this, type, action);
        }
        
        public IDisposable Subscribe<T>(Func<T, CancellationToken, UniTask> action, CancellationToken cancellationToken = default) where T : ISignal
        {
            return Subscribe(typeof(T), (signal, cancellation) =>
            {
                var item = action((T)signal, cancellation);
                return item;
            }, cancellationToken);
        }
    }
}

