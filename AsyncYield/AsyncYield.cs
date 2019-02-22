using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AsyncYield
{
    public class YieldEnumerable<TItem> : IEnumerable<TItem>
    {
        private readonly Action<YieldEnumerator<TItem>> _iteratorMethod;

        public YieldEnumerable(Action<YieldEnumerator<TItem>> iteratorMethod)
        {
            _iteratorMethod = iteratorMethod;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            var awaiter = new YieldEnumerator<TItem>();
            _iteratorMethod(awaiter);
            return awaiter;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class YieldEnumerator<TItem> : IEnumerator<TItem>, INotifyCompletion
    {
        private sealed class AbandonEnumeratorException : Exception { }
        private Exception _exception;
        private Action _continuation;

        private TItem _nextValue;
        private bool _hasNextValue;

        private TItem _current;

        public YieldEnumerator<TItem> GetAwaiter()
        {
            return this;
        }

        public void GetResult()
        {
            if (_exception != null)
            {
                var t = _exception;
                _exception = null;
                throw t;
            }

            _continuation = null;
        }

        public bool IsCompleted => false;

        public YieldEnumerator<TItem> YieldReturn(TItem value)
        {
            if (_hasNextValue)
                throw new InvalidOperationException();

            _nextValue = value;
            _hasNextValue = true;
            return this;
        }

        public TItem Current => _current;

        object IEnumerator.Current => _current;

        public bool MoveNext()
        {
            if (!_hasNextValue)
            {
                _continuation?.Invoke();
            }

            if (!_hasNextValue)
                return false;

            _current = _nextValue;
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
                catch (AbandonEnumeratorException x)
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