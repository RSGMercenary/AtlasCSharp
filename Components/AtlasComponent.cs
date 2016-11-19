﻿using Atlas.Entities;
using Atlas.LinkList;
using Atlas.Signals;
using System;

namespace Atlas.Components
{
	abstract class AtlasComponent:IComponent
	{
		private LinkList<IEntity> managers = new LinkList<IEntity>();
		private Signal<IComponent, IEntity, int> managerAdded = new Signal<IComponent, IEntity, int>();
		private Signal<IComponent, IEntity, int> managerRemoved = new Signal<IComponent, IEntity, int>();

		private readonly bool isShareable = false;

		private bool isDisposedWhenUnmanaged = true;
		private bool isDisposed = false;
		private Signal<IComponent, bool, bool> isDisposedChanged = new Signal<IComponent, bool, bool>();

		public static implicit operator bool(AtlasComponent component)
		{
			return component != null;
		}

		public AtlasComponent() : this(false)
		{

		}

		public AtlasComponent(bool isShareable = false)
		{
			this.isShareable = isShareable;
		}

		public void Dispose()
		{
			if(managers.Count > 0)
			{
				IsDisposedWhenUnmanaged = true;
				RemoveManagers();
			}
			else
			{
				Disposing();
				IsDisposed = true;
				IsDisposedWhenUnmanaged = true;
				isDisposedChanged.Dispose();
				managerAdded.Dispose();
				managerRemoved.Dispose();
			}
		}

		protected virtual void Disposing()
		{

		}

		public bool IsDisposed
		{
			get
			{
				return isDisposed;
			}
			private set
			{
				if(isDisposed != value)
				{
					bool previous = isDisposed;
					isDisposed = value;
					isDisposedChanged.Dispatch(this, value, previous);
				}
			}
		}

		public Signal<IComponent, bool, bool> IsDisposedChanged
		{
			get
			{
				return isDisposedChanged;
			}
		}

		public bool IsDisposedWhenUnmanaged
		{
			get
			{
				return isDisposedWhenUnmanaged;
			}
			set
			{
				isDisposedWhenUnmanaged = value;
			}
		}

		public bool IsShareable
		{
			get
			{
				return isShareable;
			}
		}

		public Signal<IComponent, IEntity, int> ManagerAdded
		{
			get
			{
				return managerAdded;
			}
		}

		public Signal<IComponent, IEntity, int> ManagerRemoved
		{
			get
			{
				return managerRemoved;
			}
		}

		public IEntity Manager
		{
			get
			{
				return managers.Count == 1 ? managers.First.Value : null;
			}
		}

		public IReadOnlyLinkList<IEntity> Managers
		{
			get
			{
				return managers;
			}
		}

		public int GetManagerIndex(IEntity entity)
		{
			return managers.GetIndex(entity);
		}

		public bool SetManagerIndex(IEntity entity, int index)
		{
			return managers.SetIndex(entity, index);
		}

		public bool SwapEntities(AtlasEntity entity1, AtlasEntity entity2)
		{
			return managers.Swap(entity1, entity2);
		}

		public bool SwapComponentManagers(int index1, int index2)
		{
			return managers.Swap(index1, index2);
		}

		public IEntity AddManager(IEntity entity)
		{
			return AddManager(entity, null);
		}

		public IEntity AddManager(IEntity entity, Type type)
		{
			return AddManager(entity, type, managers.Count);
		}

		public IEntity AddManager(IEntity entity, int index)
		{
			return AddManager(entity, null, index);
		}

		public IEntity AddManager(IEntity entity, Type type = null, int index = int.MaxValue)
		{
			if(entity == null)
				return null;
			if(!managers.Contains(entity))
			{
				if(type == null)
				{
					type = GetType();
				}
				else if(!type.IsInstanceOfType(this))
				{
					return null;
				}
				if(entity.GetComponent(type) == this)
				{
					index = Math.Max(0, Math.Min(index, managers.Count));
					managers.Add(entity, index);
					IsDisposed = false;
					AddingManager(entity);
					managerAdded.Dispatch(this, entity, index);
				}
				else
				{
					entity.AddComponent(this, type, index);
				}
			}
			else
			{
				SetManagerIndex(entity, index);
			}
			return entity;
		}

		protected virtual void AddingManager(IEntity entity)
		{

		}

		public IEntity RemoveManager(IEntity entity)
		{
			if(entity == null)
				return null;
			if(!managers.Contains(entity))
				return null;
			Type type = entity.GetComponentType(this);
			if(type == null)
			{
				int index = managers.GetIndex(entity);
				RemovingManager(entity);
				managers.Remove(index);
				managerRemoved.Dispatch(this, entity, index);
				if(managers.Count <= 0 && isDisposedWhenUnmanaged)
				{
					Dispose();
				}
			}
			else
			{
				entity.RemoveComponent(type);
			}
			return entity;
		}

		public IEntity RemoveManager(int index)
		{
			if(index < 0)
				return null;
			if(index > managers.Count - 1)
				return null;
			return RemoveManager(managers[index]);
		}

		protected virtual void RemovingManager(IEntity entity)
		{

		}

		public bool RemoveManagers()
		{
			if(managers.Count <= 0)
				return false;
			while(managers.Count > 0)
				RemoveManager(managers.Last.Value);
			return true;
		}

		public string Dump(string indent = "")
		{
			return "";
		}
	}
}