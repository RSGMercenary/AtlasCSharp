﻿namespace Atlas.Core.Messages
{
	public abstract class ValueMessage<TMessenger, TValue> : Message<TMessenger>, IValueMessage<TMessenger, TValue>
		where TMessenger : IMessenger
	{
		public ValueMessage(TMessenger messenger, TValue value) : base(messenger)
		{
			Value = value;
		}

		public TValue Value { get; }
	}
}