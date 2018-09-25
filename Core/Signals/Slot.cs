﻿using System;

namespace Atlas.Core.Signals
{
	public class SlotBase : ISlotBase
	{
		public static implicit operator bool(SlotBase slot)
		{
			return slot != null;
		}

		private int priority = 0;
		public ISignalBase Signal { get; internal set; }
		public Delegate Listener { get; internal set; }
		public Delegate Validator { get; set; }

		public SlotBase()
		{

		}

		public void Dispose()
		{
			if(Signal != null)
			{
				Signal.Remove(Listener);
			}
			else
			{
				Signal = null;
				Listener = null;
				priority = 0;
			}
		}

		public int Priority
		{
			get { return priority; }
			set
			{
				if(priority != value)
				{
					int previous = priority;
					priority = value;
					if(Signal != null)
						(Signal as SignalBase).PriorityChanged(this, value, previous);
				}
			}
		}
	}

	public class SlotBase<TSignal, TDelegate, TValidator> : SlotBase
		where TSignal : ISignalBase
		where TDelegate : Delegate
		where TValidator : Delegate
	{
		public new TSignal Signal
		{
			get { return (TSignal)base.Signal; }
			set { base.Signal = value; }
		}

		public new TDelegate Listener
		{
			get { return (TDelegate)base.Listener; }
			set { base.Listener = value; }
		}

		public new TValidator Validator
		{
			get { return (TValidator)base.Validator; }
			set { base.Validator = value; }
		}
	}

	public class Slot : SlotBase<ISignal, Action, Func<bool>>, ISlot, IDispatch
	{
		public bool Dispatch()
		{
			if(Validator == null || Validator())
				Listener();
			return true;
		}
	}

	public class Slot<T1> : SlotBase<ISignal<T1>, Action<T1>, Func<T1, bool>>, ISlot<T1>, IDispatch<T1>
	{
		public virtual bool Dispatch(T1 item1)
		{
			if(Validator == null || Validator(item1))
				Listener(item1);
			return true;
		}
	}

	public class Slot<T1, T2> : SlotBase<ISignal<T1, T2>, Action<T1, T2>, Func<T1, T2, bool>>, ISlot<T1, T2>, IDispatch<T1, T2>
	{
		public bool Dispatch(T1 item1, T2 item2)
		{
			if(Validator == null || Validator(item1, item2))
				Listener.Invoke(item1, item2);
			return true;
		}
	}

	public class Slot<T1, T2, T3> : SlotBase<ISignal<T1, T2, T3>, Action<T1, T2, T3>, Func<T1, T2, T3, bool>>, ISlot<T1, T2, T3>, IDispatch<T1, T2, T3>
	{
		public bool Dispatch(T1 item1, T2 item2, T3 item3)
		{
			if(Validator == null || Validator(item1, item2, item3))
				Listener.Invoke(item1, item2, item3);
			return true;
		}
	}

	public class Slot<T1, T2, T3, T4> : SlotBase<ISignal<T1, T2, T3, T4>, Action<T1, T2, T3, T4>, Func<T1, T2, T3, T4, bool>>, ISlot<T1, T2, T3, T4>, IDispatch<T1, T2, T3, T4>
	{
		public bool Dispatch(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			if(Validator == null || Validator(item1, item2, item3, item4))
				Listener.Invoke(item1, item2, item3, item4);
			return true;
		}
	}
}