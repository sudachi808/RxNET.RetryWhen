using System;
using System.Reactive.Linq;

using RxNET.Extensions;

namespace RetryWhenDemo
{
    class MainClass
    {
        const int RETRY_MAX  = 10;
        const int RETRY_WAIT = 1;

        public static void Main(string[] args)
        {
            var service = new GreetingService();

            int retryAttempt = 0;

            service.Greet()
                   .RetryWhen(errors => {
                        return errors.SelectMany(error => {
                            if (error is NeedToRetryException && ++retryAttempt <= RETRY_MAX) {
                                return Observable.Return(String.Empty).Delay(TimeSpan.FromSeconds(RETRY_WAIT));
                            }
                            return Observable.Throw<string>(error);
                        });
                    })
                   .Subscribe(
                        greet => Console.WriteLine("OnNext: {0}", greet),
                        error => Console.WriteLine("OnError: {0}", error.Message),
                        ()    => Console.WriteLine("OnCompleted:")
                  );

            Console.ReadKey();
        }
    }
    
    internal class GreetingService
    {
        int attempt = 0;

        public IObservable<string> Greet()
        {
            return Observable.Defer(() => {
                Console.WriteLine("Defer [{0}]", ++attempt);
                if (attempt % 5 == 0) {
                    return Observable.Return("Hello");
                }
                return Observable.Throw<string>(new NeedToRetryException("Zzz..."));
            });
        }
    }

    class NeedToRetryException : Exception
    {
        public NeedToRetryException() : base() { }
        public NeedToRetryException(string message) : base(message) { }
        public NeedToRetryException(string message, Exception inner) : base(message, inner) { }
    }
}
