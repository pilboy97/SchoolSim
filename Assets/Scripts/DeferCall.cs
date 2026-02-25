using System;

namespace Game
{
    public class DeferCall:IDisposable
    {
        private readonly Action _call;

        public DeferCall(Action call)
        {
            _call = call;
        }
        public void Dispose()
        {
            _call?.Invoke();
        }
    }
}