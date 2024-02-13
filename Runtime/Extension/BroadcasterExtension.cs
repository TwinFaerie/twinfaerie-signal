using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TF.Signal
{
    public static class BroadcasterExtension
    {
        public static void Publish<T>(this Broadcaster publisher, T signal, CancellationToken cancellation = default) where T : ISignal
        {
            publisher.PublishAsync(signal, cancellation).Forget();
        }
        
        public static IDisposable Subscribe<T>(this Broadcaster subscriber, Func<T, UniTask> action) where T : ISignal
        {
            return subscriber.Subscribe<T>((signal, cancellation) => action(signal));
        }

        public static IDisposable Subscribe<T>(this Broadcaster subscriber, Action<T> action) where T : ISignal
        {
            return subscriber.Subscribe<T>((signal, cancellation) =>
            {
                action(signal);
                return UniTask.CompletedTask;
            });
        }
        
        public static IDisposable Subscribe(this Broadcaster subscriber, Type type, Func<ISignal, UniTask> action)
        {
            return subscriber.Subscribe(type, (signal, cancellation) => action(signal));
        }

        public static IDisposable Subscribe(this Broadcaster subscriber, Type type, Action<ISignal> action)
        {
            return subscriber.Subscribe(type, (signal, cancellation) =>
            {
                action(signal);
                return UniTask.CompletedTask;
            });
        }
    }
}

