using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AsyncYield
{
    public delegate Task IteratorMethod<TItem, TResult>(YieldEnumerator<TItem, TResult> e);

    public class YieldEnumerator<TItem, TResult> : IEnumerator<TItem>, INotifyCompletion
    {
        private sealed class AbandonEnumeratorException : Exception { }
        private Exception _exception;
        private Action _continuation;

        private TItem _nextValue;
        private bool _hasNextValue;

        private TResult _result;
        private Task _task;

        public YieldEnumerator(IteratorMethod<TItem,TResult> iteratorMethod)
        {
            _task = iteratorMethod(this);
        }

        private void Execute()
        {
            // If we already have a buffered value that hasn't been
            // retrieved, we shouldn't do anything yet. If we don't
            // and there's no continuation to run, we've finished.
            // And if _task is null, we've been disposed.
            if (_hasNextValue || _continuation == null || _task == null)
                return;
 
            // Be ultra-careful not to run same _continuation twice
            var t = _continuation;
            _continuation = null;
            t(); // may or may not have stored a new _continuation
 
            // And may also have hit a snag!
            if (_task.Exception != null)
            {
                // Unpeel the AggregateException wrapping
                Exception inner = _task.Exception;
                while (inner is AggregateException)
                    inner = inner.InnerException;
 
                throw inner;
            }
        }

        public YieldEnumerator<TItem, TResult> GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted => false;

        public void OnCompleted(Action continuation)
        {
            Debug.Assert(_continuation == null);
            _continuation = continuation;
        }

        public TResult GetResult()
        {
            if (_exception != null)
            {
                var t = _exception;
                _exception = null;
                throw t;
            }
            
            return _result;
        }

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
            Execute();

            if (_hasNextValue)
            {
                Current = _nextValue;
                _hasNextValue = false;
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (_continuation != null)
            {
                _exception = new AbandonEnumeratorException();

                try
                {
                    Execute();
                }
                catch (AbandonEnumeratorException)
                {
                    Debug.Assert(_exception == null);
                }

                _task.Dispose();
                _task = null;
            }
        }
        
        public void Reset()
        {
            throw new NotImplementedException("Reset");
        }
         
        public void Return(TResult value)
        {
            _result = value;
        }
        
        public void Throw(Exception x)
        {
            _exception = x;
        }
    }

    public class YieldEnumerable<TItem, TResult> : IEnumerable<TItem>
    {
        private readonly IteratorMethod<TItem, TResult> _iteratorMethod;

        public YieldEnumerable(IteratorMethod<TItem, TResult> iteratorMethod)
        {
            _iteratorMethod = iteratorMethod;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return new YieldEnumerator<TItem, TResult>(_iteratorMethod);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}