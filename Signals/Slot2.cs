﻿using System;

namespace Atlas.Signals
{
	sealed class Slot<T1, T2>:SlotBase, ISlot<T1, T2>
	{
		internal Slot()
		{

		}

		public new ISignal<T1, T2> Signal
		{
			get
			{
				return (ISignal<T1, T2>)signal;
			}
		}

		public new Action<T1, T2> Listener
		{
			get
			{
				return (Action<T1, T2>)listener;
			}
		}
	}
}