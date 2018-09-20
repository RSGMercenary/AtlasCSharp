﻿using Atlas.Core.Messages;
using Atlas.Core.Objects;
using Atlas.ECS.Components;
using Atlas.ECS.Messages;
using Atlas.ECS.Objects;

namespace Atlas.ECS.Systems
{
	public abstract class AtlasSystem : AtlasSystem<ISystem>
	{

	}

	public abstract class AtlasSystem<T> : EngineObject<T>, ISystem<T>
		where T : class, ISystem
	{
		private int priority = 0;
		private int sleeping = 0;
		private double totalIntervalTime = 0;
		private double deltaIntervalTime = 0;
		private TimeStep timeStep = TimeStep.Variable;
		private TimeStep updateState = TimeStep.None;
		private bool updateLock = false;

		public AtlasSystem()
		{

		}

		public sealed override void Dispose()
		{
			if(State != ObjectState.Composed)
				return;
			//Can't destroy System mid-update.
			if(Engine != null && Engine.UpdateState != TimeStep.None)
				return;
			Engine = null;
			if(Engine == null)
				base.Dispose();
		}

		protected override void Disposing(bool finalizer)
		{
			Priority = 0;
			Sleeping = 0;
			base.Disposing(finalizer);
		}

		public sealed override IEngine Engine
		{
			get { return base.Engine; }
			set
			{
				if(value != null)
				{
					if(Engine == null && value.HasSystem(this))
					{
						base.Engine = value;
						AddFamilies();
						SyncTotalIntervalTime();
					}
				}
				else
				{
					if(Engine != null && !Engine.HasSystem(this))
					{
						RemoveFamilies();
						base.Engine = value;
						SyncTotalIntervalTime();
					}
				}
			}
		}

		protected virtual void AddFamilies() { }

		protected virtual void RemoveFamilies() { }

		#region Updating

		public void Update(double deltaTime)
		{
			if(IsSleeping)
				return;
			if(Engine?.CurrentSystem != this)
				return;
			if(updateLock)
				return;

			if(deltaIntervalTime > 0)
			{
				deltaTime = deltaIntervalTime;
				if(Engine.TotalVariableTime - totalIntervalTime < deltaIntervalTime)
					return;
				totalIntervalTime += deltaIntervalTime;
			}

			updateLock = true;
			UpdateState = TimeStep;
			Updating(deltaTime);
			UpdateState = TimeStep.None;
			updateLock = false;
		}

		protected virtual void Updating(double deltaTime) { }

		#endregion

		#region Sleeping

		public int Sleeping
		{
			get { return sleeping; }
			private set
			{
				if(sleeping == value)
					return;
				int previous = sleeping;
				sleeping = value;
				Dispatch<ISleepMessage<IReadOnlySystem>>(new SleepMessage<IReadOnlySystem>(this, value, previous));
			}
		}

		public bool IsSleeping
		{
			get { return sleeping > 0; }
			set
			{
				if(value)
					++Sleeping;
				else
					--Sleeping;
			}
		}

		#endregion

		public double DeltaIntervalTime
		{
			get { return deltaIntervalTime; }
			protected set
			{
				if(deltaIntervalTime == value)
					return;
				var previous = deltaIntervalTime;
				deltaIntervalTime = value;
				Dispatch<IIntervalMessage>(new IntervalMessage(this, value, previous));
				SyncTotalIntervalTime();
			}
		}

		/// <summary>
		/// Syncs this System's interval time to match other Systems with the same interval time.
		/// </summary>
		private void SyncTotalIntervalTime()
		{
			if(deltaIntervalTime <= 0)
				return;
			totalIntervalTime = 0;
			while(totalIntervalTime + deltaIntervalTime < Engine?.TotalVariableTime)
				totalIntervalTime += deltaIntervalTime;
		}

		public TimeStep TimeStep
		{
			get { return timeStep; }
			protected set
			{
				if(timeStep == value)
					return;
				var previous = timeStep;
				timeStep = value;
				Dispatch<IUpdateStateMessage<IReadOnlySystem>>(new UpdateStateMessage<IReadOnlySystem>(this, value, previous));
			}
		}

		public int Priority
		{
			get { return priority; }
			protected set
			{
				if(priority == value)
					return;
				int previous = priority;
				priority = value;
				Dispatch<IPriorityMessage>(new PriorityMessage(this, value, previous));
			}
		}

		public TimeStep UpdateState
		{
			get { return updateState; }
			private set
			{
				if(updateState == value)
					return;
				var previous = updateState;
				updateState = value;
				Dispatch<IUpdateStateMessage<IReadOnlySystem>>(new UpdateStateMessage<IReadOnlySystem>(this, value, previous));
			}
		}
	}
}