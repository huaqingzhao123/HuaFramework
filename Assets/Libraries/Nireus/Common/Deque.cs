using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nireus
{
	public class Deque<T> : List<T>
	{
		public T shift()
		{
			T t = default(T);
			if (this.Count > 0)
			{
				t = this[0];
				this.RemoveAt(0);
			}
			return t;
		}

		public T pop()
		{
			T t = default(T);
			if (this.Count > 0)
			{
				t = this[this.Count - 1];
				this.RemoveAt(this.Count - 1);
			}
			return t;
		}

		public void push(T t)
		{
			this.Add(t);
		}

		public void unshift(T t)
		{
			this.Insert(0, t);
		}
	}
}