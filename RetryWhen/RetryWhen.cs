using System;
using System.Reactive.Linq;

namespace RxNET.Extensions
{
    public static class RetryWhenExtenstion
    {
        public static IObservable<T> RetryWhen<T>(
            this IObservable<T> source,
            Func<IObservable<Exception>, IObservable<T>> predicate)
        {
            return RetryWhenRecursive(source, predicate);
        }
        
        private static IObservable<T> RetryWhenRecursive<T>(
            IObservable<T> source,
            Func<IObservable<Exception>, IObservable<T>> predicate)
        {
            return source.Catch((Exception e) =>
            {
                //
                // シーケンスから例外が Throw された場合、
                // predicate でリトライするかどうかを判定する。
                //
                return
                     predicate(Observable.Return(e))
                    .Catch((Exception ee) =>
                     {
                        //
                        // predicate から例外が Throw された場合、
                        // 後続のシーケンスに例外を流す（＝リトライせずオブザーバーに例外を捕捉させる）。
                        //
                        return Observable.Throw<T>(e);
                     })
                    .SelectMany(_ =>
                     {
                        //
                        // predicate から例外が Throw されなかった場合、
                        // 再度、起点となったシーケンスに繋げる（＝リトライ）。
                        //
                        return RetryWhenRecursive(source, predicate);
                     });
            });
        }
    }
}
