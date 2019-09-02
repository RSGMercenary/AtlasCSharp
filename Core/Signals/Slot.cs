﻿using System;

namespace Atlas.Core.Signals
{
	public class SlotBase : ISlotBase
	{
		private int priority = 0;
		public ISignalBase Signal { get; internal set; }
		public Delegate Listener { get; internal set; }

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
			get => priority;
			set
			{
				if(priority == value)
					return;
				priority = value;
				(Signal as SignalBase)?.Prioritize(this);
			}
		}
	}

	public class SlotBase<TSignal, TDelegate> : SlotBase
		where TSignal : ISignalBase
		where TDelegate : Delegate
	{
		public new TSignal Signal
		{
			get => (TSignal)base.Signal;
			set => base.Signal = value;
		}

		public new TDelegate Listener
		{
			get => (TDelegate)base.Listener;
			set => base.Listener = value;
		}
	}

	public class Slot : SlotBase<ISignal, Action>, ISlot, IDispatch
	{
		public bool Dispatch()
		{
			Listener();
			return true;
		}
	}

	public class Slot<T1> : SlotBase<ISignal<T1>, Action<T1>>, ISlot<T1>, IDispatch<T1>
	{
		public virtual bool Dispatch(T1 item1)
		{
			Listener(item1);
			return true;
		}
	}

	public class Slot<T1, T2> : SlotBase<ISignal<T1, T2>, Action<T1, T2>>, ISlot<T1, T2>, IDispatch<T1, T2>
	{
		public bool Dispatch(T1 item1, T2 item2)
		{
			Listener.Invoke(item1, item2);
			return true;
		}
	}

	public class Slot<T1, T2, T3> : SlotBase<ISignal<T1, T2, T3>, Action<T1, T2, T3>>, ISlot<T1, T2, T3>, IDispatch<T1, T2, T3>
	{
		public bool Dispatch(T1 item1, T2 item2, T3 item3)
		{
			Listener.Invoke(item1, item2, item3);
			return true;
		}
	}

	public class Slot<T1, T2, T3, T4> : SlotBase<ISignal<T1, T2, T3, T4>, Action<T1, T2, T3, T4>>, ISlot<T1, T2, T3, T4>, IDispatch<T1, T2, T3, T4>
	{
		public bool Dispatch(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			Listener.Invoke(item1, item2, item3, item4);
			return true;
		}
	}
}