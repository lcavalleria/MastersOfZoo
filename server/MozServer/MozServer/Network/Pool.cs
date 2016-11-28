using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MozServer.Network
{
    class Pool<T> where T : new()
    {
        Stack<T> stack;
        public Pool()
        {
            stack = new Stack<T>();
        }
        public void Push(T item)
        {
            stack.Push(item);
        }
        public T Pop()
        {
            if (stack.Count > 0)
                return stack.Pop();
            else return new T();
        }
    }
}