﻿namespace Atlas.Engine.Messages
{
	interface IKeyValueMessage<TSender, TKey, TValue> : IMessage<TSender>
	{
		TKey Key { get; }
		TValue Value { get; }
	}
}