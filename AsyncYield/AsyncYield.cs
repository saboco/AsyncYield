using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AsyncYield
{
    public class YieldEnumerable<TItem, TResult> : IEnumerable<TItem>
    {
        private readonly Action<YieldEnumerator<TItem, TResult>> _iteratorMethod;

        public YieldEnumerable(Action<YieldEnumerator<TItem, TResult>> iteratorMethod)
        {
            _iteratorMethod = iteratorMethod;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            var awaiter = new YieldEnumerator<TItem, TResult>();
            _iteratorMethod(awaiter);
            return awaiter;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class YieldEnumerator<TItem, TResult> : IEnumerator<TItem>, INotifyCompletion
    {
        private sealed class AbandonEnumeratorException : Exception { }
        private Exception _exception;
        private Action _continuation;

        private TItem _nextValue;
        private bool _hasNextValue;

        private TResult _result;

        public YieldEnumerator<TItem, TResult> GetAwaiter()
        {
            return this;
        }

        public void Return(TResult value)
        {
            _result = value;
        }
        
        public void Throw(Exception x)
        {
            _exception = x;
        }

        public TResult GetResult()
        {
            if (_exception != null)
            {
                var t = _exception;
                _exception = null;
                throw t;
            }

            _continuation = null;
            return _result;
        }

        public bool IsCompleted => false;

        public YieldEnumerator<TItem, TResult> YieldReturn(TItem value)
        {
            if (_hasNextValue)
                throw new InvalidOperationException();

            _nextValue = value;
            _hasNextValue = true;
            return this;
        }

        public TItem Current { get; private set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (!_hasNextValue)
            {
                _continuation?.Invoke();
            }

            if (!_hasNextValue)
                return false;

            Current = _nextValue;
            _hasNextValue = false;
            return true;
        }

        public void Reset()
        {
            _nextValue = default(TItem);
            _continuation = null;
        }

        public void Dispose()
        {
            if (_continuation != null)
            {
                _exception = new AbandonEnumeratorException();

                try
                {
                    _continuation();
                }
                catch (AbandonEnumeratorException)
                {
                    Debug.Assert(_exception == null);
                }

                _continuation = null;
            }
        }

        public void OnCompleted(Action continuation)
        {
            Debug.Assert(_continuation == null);
            _continuation = continuation;
        }
    }
}