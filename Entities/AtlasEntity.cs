﻿using Atlas.Components;
using Atlas.Engine;
using Atlas.LinkList;
using Atlas.Signals;
using Atlas.Systems;
using System;
using System.Collections.Generic;

namespace Atlas.Entities
{
	sealed class AtlasEntity:IEntity
	{
		private IEngine engine;
		private Signal<IEntity, IEngine, IEngine> engineChanged = new Signal<IEntity, IEngine, IEngine>();

		private string globalName = Guid.NewGuid().ToString("N");
		private Signal<IEntity, string, string> globalNameChanged = new Signal<IEntity, string, string>();

		private string localName = Guid.NewGuid().ToString("N");
		private Signal<IEntity, string, string> localNameChanged = new Signal<IEntity, string, string>();

		private LinkList<IEntity> children = new LinkList<IEntity>();
		private Dictionary<string, IEntity> childLocalNames = new Dictionary<string, IEntity>();
		private Signal<IEntity, IEntity, int> childAdded = new Signal<IEntity, IEntity, int>();
		private Signal<IEntity, IEntity, int> childRemoved = new Signal<IEntity, IEntity, int>();
		//Bool is true for inclusive (1 through 4) and false for exclusive (1 and 4)
		private Signal<IEntity, int, int, bool> childIndicesChanged = new Signal<IEntity, int, int, bool>(); //Indices of children

		private IEntity parent;
		private Signal<IEntity, IEntity, IEntity> parentChanged = new Signal<IEntity, IEntity, IEntity>();
		private Signal<IEntity, int, int> parentIndexChanged = new Signal<IEntity, int, int>(); //Index within parent

		private Dictionary<Type, IComponent> components = new Dictionary<Type, IComponent>();
		private Signal<IEntity, IComponent, Type> componentAdded = new Signal<IEntity, IComponent, Type>();
		private Signal<IEntity, IComponent, Type> componentRemoved = new Signal<IEntity, IComponent, Type>();

		private HashSet<Type> systems;
		private Signal<IEntity, Type> systemAdded = new Signal<IEntity, Type>();
		private Signal<IEntity, Type> systemRemoved = new Signal<IEntity, Type>();

		private int sleeping = 0;
		private Signal<AtlasEntity, int, int> sleepingChanged = new Signal<AtlasEntity, int, int>();

		private int sleepingParentIgnored = 0;
		private Signal<AtlasEntity, int, int> sleepingParentIgnoredChanged = new Signal<AtlasEntity, int, int>();

		private bool isDisposed = false;
		private bool isDisposedWhenUnmanaged = true;

		public static implicit operator bool(AtlasEntity entity)
		{
			return entity != null;
		}

		public AtlasEntity(string globalName = "", string localName = "")
		{
			GlobalName = globalName;
			LocalName = localName;
		}

		public void Dispose()
		{
			if(engine != null)
			{
				IsDisposedWhenUnmanaged = true;
				Parent = null;
			}
			else
			{
				IsDisposed = true;
				RemoveChildren();
				RemoveComponents();
				Parent = null;
				Sleeping = 0;
				SleepingParentIgnored = 0;
				IsDisposedWhenUnmanaged = true;
				GlobalName = "";
				LocalName = "";

				engineChanged.Dispose();
				globalNameChanged.Dispose();
				localNameChanged.Dispose();
				childAdded.Dispose();
				childRemoved.Dispose();
				parentChanged.Dispose();
				componentAdded.Dispose();
				componentRemoved.Dispose();
				sleepingChanged.Dispose();
				sleepingParentIgnoredChanged.Dispose();
			}
		}

		public IEngine Engine
		{
			get
			{
				return engine;
			}
			set
			{
				if(value != null)
				{
					if(engine == null && value.HasEntity(this))
					{
						IEngine previous = engine;
						engine = value;
						engineChanged.Dispatch(this, value, previous);
					}
				}
				else
				{
					if(engine != null && !engine.HasEntity(this))
					{
						IEngine previous = engine;
						engine = value;
						engineChanged.Dispatch(this, value, previous);
					}
				}
			}
		}

		public Signal<IEntity, IEngine, IEngine> EngineChanged
		{
			get
			{
				return engineChanged;
			}
		}

		public string GlobalName
		{
			get
			{
				return globalName;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					return;
				if(globalName == value)
					return;
				if(engine != null && engine.HasEntity(value))
					return;
				string previous = globalName;
				globalName = value;
				globalNameChanged.Dispatch(this, value, previous);
			}
		}

		public Signal<IEntity, string, string> GlobalNameChanged
		{
			get
			{
				return globalNameChanged;
			}
		}

		public string LocalName
		{
			get
			{
				return localName;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					return;
				if(localName == value)
					return;
				if(parent != null && parent.HasChild(value))
					return;
				string previous = localName;
				localName = value;
				localNameChanged.Dispatch(this, value, previous);
			}
		}

		public Signal<IEntity, string, string> LocalNameChanged
		{
			get
			{
				return localNameChanged;
			}
		}

		public IEntity Root
		{
			get
			{
				return engine != null ? engine.Manager : null;
			}
		}

		public bool HasComponent<TType>() where TType : IComponent
		{
			return HasComponent(typeof(TType));
		}

		public bool HasComponent(Type type)
		{
			return components.ContainsKey(type);
		}

		public TComponent GetComponent<TComponent, TType>() where TComponent : IComponent, TType
		{
			return (TComponent)GetComponent(typeof(TType));
		}

		public TType GetComponent<TType>() where TType : IComponent
		{
			return (TType)GetComponent(typeof(TType));
		}

		public IComponent GetComponent(Type type)
		{
			return components.ContainsKey(type) ? components[type] : null;
		}

		public Type GetComponentType(IComponent component)
		{
			if(component == null)
				return null;
			foreach(Type type in components.Keys)
			{
				if(components[type] == component)
				{
					return type;
				}
			}
			return null;
		}

		public IReadOnlyDictionary<Type, IComponent> Components
		{
			get
			{
				return components;
			}
		}

		//New component with Type
		public TComponent AddComponent<TComponent, TType>() where TComponent : IComponent, TType, new()
		{
			return (TComponent)AddComponent(new TComponent(), typeof(TType), 0);
		}

		//Component with Type
		public TComponent AddComponent<TComponent, TType>(TComponent component) where TComponent : IComponent, TType
		{
			return (TComponent)AddComponent(component, typeof(TType), component.Managers.Count);
		}

		//Component with Type, index
		public TComponent AddComponent<TComponent, TType>(TComponent component, int index) where TComponent : IComponent, TType
		{
			return (TComponent)AddComponent(component, typeof(TType), index);
		}

		//New Component
		public TComponent AddComponent<TComponent>() where TComponent : IComponent, new()
		{
			return (TComponent)AddComponent(new TComponent(), null, 0);
		}

		//Component
		public TComponent AddComponent<TComponent>(TComponent component) where TComponent : IComponent
		{
			return (TComponent)AddComponent(component, null, component.Managers.Count);
		}

		//Component, index
		public TComponent AddComponent<TComponent>(TComponent component, int index) where TComponent : IComponent
		{
			return (TComponent)AddComponent(component, null, index);
		}

		public IComponent AddComponent(IComponent component, Type type = null, int index = int.MaxValue)
		{
			if(component == null)
				return null;
			if(!component.IsShareable && component.Managers.Count > 0)
				return null;
			if(type == null)
				type = component.GetType();
			else if(!type.IsInstanceOfType(component))
				return null;
			if(!components.ContainsKey(type) || components[type] != component)
			{
				RemoveComponent(type);
				components.Add(type, component);
				component.AddManager(this, type, index);
				componentAdded.Dispatch(this, component, type);
			}
			return component;
		}

		public Signal<IEntity, IComponent, Type> ComponentAdded
		{
			get
			{
				return componentAdded;
			}
		}

		public TComponent RemoveComponent<TComponent, TType>() where TComponent : IComponent, TType
		{
			return (TComponent)RemoveComponent(typeof(TType));
		}

		public TType RemoveComponent<TType>() where TType : IComponent
		{
			return (TType)RemoveComponent(typeof(TType));
		}

		public IComponent RemoveComponent(Type type)
		{
			if(type == null)
				return null;
			if(!components.ContainsKey(type))
				return null;
			IComponent component = components[type];
			components.Remove(type);
			component.RemoveManager(this);
			componentRemoved.Dispatch(this, component, type);
			return component;
		}

		public Signal<IEntity, IComponent, Type> ComponentRemoved
		{
			get
			{
				return componentRemoved;
			}
		}

		public IComponent RemoveComponent(IComponent component)
		{
			return RemoveComponent(GetComponentType(component));
		}

		public void RemoveComponents()
		{
			foreach(Type type in components.Keys)
			{
				RemoveComponent(type);
			}
		}

		public IEntity Parent
		{
			get
			{
				return parent;
			}
			set
			{
				SetParent(value);
			}
		}

		public bool HasChild(IEntity child)
		{
			return children.Contains(child);
		}

		public bool HasChild(string localName)
		{
			return GetChild(localName) != null;
		}

		public IEntity AddChild(IEntity child)
		{
			return AddChild(child, children.Count);
		}

		public IEntity AddChild(IEntity child, int index)
		{
			if(child == null)
				return null;
			if(child.Parent == this)
			{
				if(childLocalNames.ContainsKey(child.LocalName) && childLocalNames[child.LocalName] != child)
				{
					child.LocalName = Guid.NewGuid().ToString("N");
				}
				if(!childLocalNames.ContainsKey(child.LocalName))
				{
					childLocalNames.Add(child.LocalName, child);
					children.Add(child, index);
					child.LocalNameChanged.Add(ChildLocalNameChanged);
					if(IsSleeping && !child.IsSleepingParentIgnored)
					{
						++child.Sleeping;
					}
					childAdded.Dispatch(this, child, index);
					childIndicesChanged.Dispatch(this, index, children.Count, true);
					for(int i = index + 1; i < children.Count; ++i)
					{
						IEntity sibling = children[i];
						sibling.ParentIndexChanged.Dispatch(sibling, i, i - 1);
					}
				}
				else
				{
					SetChildIndex(child, index);
				}
			}
			else
			{
				child.SetParent(this, index);
			}
			return child;
		}

		private void ChildLocalNameChanged(IEntity child, string next, string previous)
		{
			childLocalNames.Remove(previous);
			childLocalNames.Add(next, child);
		}

		public Signal<IEntity, IEntity, int> ChildAdded
		{
			get
			{
				return childAdded;
			}
		}

		public IEntity RemoveChild(IEntity child)
		{
			if(child != null)
			{
				if(child.Parent != this)
				{
					if(childLocalNames.ContainsKey(child.LocalName))
					{
						int index = children.GetIndex(child);
						children.Remove(index);
						childLocalNames.Remove(child.LocalName);
						child.LocalNameChanged.Remove(ChildLocalNameChanged);
						childRemoved.Dispatch(this, child, index);
						childIndicesChanged.Dispatch(this, index, children.Count, true);
						for(int i = index; i < children.Count; ++i)
						{
							IEntity sibling = children[i];
							sibling.ParentIndexChanged.Dispatch(sibling, i, i + 1);
						}
						if(IsSleeping && !child.IsSleepingParentIgnored)
						{
							--child.Sleeping;
						}
						return child;
					}
				}
				else
				{
					child.SetParent(null, -1);
					return child;
				}
			}
			return null;
		}

		public Signal<IEntity, IEntity, int> ChildRemoved
		{
			get
			{
				return childRemoved;
			}
		}

		public IEntity RemoveChild(int index)
		{
			if(index < 0)
				return null;
			if(index > children.Count - 1)
				return null;
			return RemoveChild(children.Get(index));
		}

		public void RemoveChildren()
		{
			while(children.First != null)
			{
				children.Last.Value.Dispose();
			}
		}

		public bool SetParent(IEntity parent = null, int index = int.MaxValue)
		{
			//Can't set the parent of the root.
			if(Root == this)
				return false;
			//Parents are the same.
			if(this.parent == parent)
				return false;
			//Can't set a parent's ancestor (this) as a child.
			if(HasHierarchy(parent))
				return false;
			IEntity previousParent = this.parent;
			int previousIndex = -1;
			this.parent = parent;
			if(previousParent != null)
			{
				previousIndex = previousParent.GetChildIndex(this);
				previousParent.RemoveChild(previousIndex);
			}
			if(parent != null)
			{
				IsDisposed = false;
				index = Math.Max(0, Math.Min(index, parent.Children.Count));
				parent.AddChild(this, index);
			}
			else
			{
				index = -1;
			}
			parentChanged.Dispatch(this, parent, previousParent);
			if(index != previousIndex)
			{
				parentIndexChanged.Dispatch(this, index, previousIndex);
			}

			if(this.parent == null && IsDisposedWhenUnmanaged)
			{
				Dispose();
			}
			return true;
		}

		public Signal<IEntity, IEntity, IEntity> ParentChanged
		{
			get
			{
				return parentChanged;
			}
		}

		public bool HasHierarchy(IEntity entity)
		{
			while(entity != this && entity != null)
			{
				entity = entity.Parent;
			}
			return entity == this;
		}

		public IEntity GetHierarchy(string hierarchy)
		{
			if(string.IsNullOrWhiteSpace(hierarchy))
				return null;
			string[] localNames = hierarchy.Split('/');
			IEntity entity = this;
			foreach(string localName in localNames)
			{
				if(localName == "..")
				{
					entity = entity.Parent;
				}
				else
				{
					entity = entity.GetChild(localName);
				}
				if(entity == null)
				{
					break;
				}
			}
			return entity;
		}

		public bool SetHierarchy(string hierarchy, int index)
		{
			return SetParent(GetHierarchy(hierarchy), index);
		}

		public IEntity GetChild(string localName)
		{
			return childLocalNames.ContainsKey(localName) ? childLocalNames[localName] : null;
		}

		public IEntity GetChild(int index)
		{
			if(index < 0)
				return null;
			if(index > children.Count - 1)
				return null;
			return children.Get(index);
		}

		public int GetChildIndex(IEntity child)
		{
			return children.GetIndex(child);
		}

		public bool SetChildIndex(IEntity child, int index)
		{
			int previous = children.GetIndex(child);

			if(previous == index)
				return true;
			if(previous < 0)
				return false;

			index = Math.Max(0, Math.Min(index, children.Count - 1));

			int next = index;

			children.Remove(previous);
			children.Add(child, next);

			if(next > previous)
			{
				childIndicesChanged.Dispatch(this, previous, next, true);

				//Children shift down 0<-[1]
				for(index = previous; index < next; ++index)
				{
					child = children[index];
					child.ParentIndexChanged.Dispatch(child, index, index + 1);
				}
			}
			else
			{
				childIndicesChanged.Dispatch(this, next, previous, true);

				//Children shift up [0]->1
				for(index = previous; index > next; --index)
				{
					child = children[index];
					child.ParentIndexChanged.Dispatch(child, index, index - 1);
				}
			}
			child.ParentIndexChanged.Dispatch(child, next, previous);
			return true;
		}

		public Signal<IEntity, int, int> ParentIndexChanged
		{
			get
			{
				return parentIndexChanged;
			}
		}

		public Signal<IEntity, int, int, bool> ChildIndicesChanged
		{
			get
			{
				return childIndicesChanged;
			}
		}

		public bool SwapChildren(AtlasEntity child1, AtlasEntity child2)
		{
			if(child1 == null)
				return false;
			if(child2 == null)
				return false;
			int index1 = children.GetIndex(child1);
			int index2 = children.GetIndex(child2);
			return SwapChildren(index1, index2);
		}

		public bool SwapChildren(int index1, int index2)
		{
			if(index1 < 0)
				return false;
			if(index2 < 0)
				return false;
			if(index1 > children.Count - 1)
				return false;
			if(index2 > children.Count - 1)
				return false;
			IEntity child1 = children[index1];
			IEntity child2 = children[index2];
			children.Swap(child1, child2);
			childIndicesChanged.Dispatch(this, Math.Min(index1, index2), Math.Max(index1, index2), false);
			child1.ParentIndexChanged.Dispatch(child1, index2, index1);
			child2.ParentIndexChanged.Dispatch(child2, index1, index2);
			return true;
		}

		public IReadOnlyLinkList<IEntity> Children
		{
			get
			{
				return children;
			}
		}

		public IReadOnlyDictionary<string, IEntity> ChildLocalNames
		{
			get
			{
				return childLocalNames;
			}
		}

		public Signal<AtlasEntity, int, int> SleepingChanged
		{
			get
			{
				return sleepingChanged;
			}
		}

		public int Sleeping
		{
			get
			{
				return sleeping;
			}
			set
			{
				if(sleeping != value)
				{
					int previous = sleeping;
					sleeping = value;
					sleepingChanged.Dispatch(this, value, previous);

					if(value > 0 && previous <= 0)
					{
						ILinkListNode<IEntity> current = children.First;
						while(current != null)
						{
							if(!current.Value.IsSleepingParentIgnored)
							{
								++current.Value.Sleeping;
							}
							current = current.Next;
						}
					}
					else if(value <= 0 && previous > 0)
					{
						ILinkListNode<IEntity> current = children.First;
						while(current != null)
						{
							if(!current.Value.IsSleepingParentIgnored)
							{
								--current.Value.Sleeping;
							}
							current = current.Next;
						}
					}
				}
			}
		}

		public bool IsSleeping
		{
			get
			{
				return sleeping > 0;
			}
		}

		public Signal<AtlasEntity, int, int> SleepingParentIgnoredChanged
		{
			get
			{
				return sleepingParentIgnoredChanged;
			}
		}

		public int SleepingParentIgnored
		{
			get
			{
				return sleepingParentIgnored;
			}
			set
			{
				if(sleepingParentIgnored != value)
				{
					int previous = sleepingParentIgnored;
					sleepingParentIgnored = value;
					sleepingParentIgnoredChanged.Dispatch(this, value, previous);

					if(parent != null && parent.IsSleeping)
					{
						if(value <= 0)
						{
							++Sleeping;
						}
						else
						{
							--Sleeping;
						}
					}
				}
			}
		}

		public bool IsSleepingParentIgnored
		{
			get
			{
				return sleepingParentIgnored > 0;
			}
		}

		public Signal<IEntity, Type> SystemAdded
		{
			get
			{
				return systemAdded;
			}
		}

		public Signal<IEntity, Type> SystemRemoved
		{
			get
			{
				return systemRemoved;
			}
		}

		public IReadOnlyCollection<Type> Systems
		{
			get
			{
				return (IReadOnlyCollection<Type>)systems;
			}
		}

		public bool HasSystem<T>() where T : ISystem
		{
			return HasSystem(typeof(T));
		}

		public bool HasSystem(Type type)
		{
			return systems != null && systems.Contains(type);
		}

		public bool AddSystem<T>() where T : ISystem
		{
			return AddSystem(typeof(T));
		}

		public bool AddSystem(Type type)
		{
			if(systems == null)
			{
				systems = new HashSet<Type>();
			}
			if(!systems.Contains(type))
			{
				systems.Add(type);
				systemAdded.Dispatch(this, type);
				return true;
			}
			return false;
		}

		public bool RemoveSystem<T>() where T : ISystem
		{
			return RemoveSystem(typeof(T));
		}

		public bool RemoveSystem(Type type)
		{
			if(systems != null)
			{
				if(systems.Contains(type))
				{
					systems.Remove(type);
					systemRemoved.Dispatch(this, type);
					if(systems.Count <= 0)
					{
						systems = null;
					}
					return true;
				}
			}
			return false;
		}

		public void RemoveSystems()
		{
			if(systems != null)
			{
				foreach(Type system in systems)
				{
					RemoveSystem(system);
				}
			}
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
				}
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
				if(IsDisposedWhenUnmanaged != value)
				{
					isDisposedWhenUnmanaged = value;

					if(parent == null && value)
					{
						Dispose();
					}
				}

			}
		}

		public string Dump(string indent = "")
		{
			int index;
			string text = indent;

			index = parent != null ? parent.GetChildIndex(this) + 1 : 1;
			text += "Child " + index;
			text += "\n  " + indent;
			text += "Global Name            = " + globalName;
			text += "\n  " + indent;
			text += "Local Name             = " + localName;
			text += "\n  " + indent;
			text += "Sleeping               = " + sleeping;
			text += "\n  " + indent;
			text += "Ignore Parent Sleeping = " + sleepingParentIgnored;
			text += "\n  " + indent;
			text += "Auto-Dispose           = " + isDisposedWhenUnmanaged;

			text += "\n  " + indent;
			text += "Components";
			index = 0;
			foreach(Type type in components.Keys)
			{
				IComponent component = components[type];
				text += "\n    " + indent;
				text += "Component " + (++index);
				text += "\n      " + indent;
				text += "Type					= " + type.FullName;
				text += "\n      " + indent;
				text += "Instance				= " + component.GetType().FullName;
				text += "\n      " + indent;
				text += "Shareable              = " + component.IsShareable;
				text += "\n      " + indent;
				text += "Managers				= " + component.Managers.Count;
				text += "\n      " + indent;
				text += "Auto-Dispose			= " + component.IsDisposedWhenUnmanaged;
			}

			text += "\n  " + indent;
			text += "Systems";
			index = 0;
			foreach(Type type in systems)
			{
				text += "\n    " + indent;
				text += "System " + (++index);
				text += "\n      " + indent;
				text += "Type					= " + type.FullName;
			}

			text += "\n  " + indent;
			text += "Children";
			text += "\n";
			for(index = 0; index < children.Count; ++index)
			{
				text += children[index].Dump(indent + "    ");
			}

			return text;
		}
	}
}