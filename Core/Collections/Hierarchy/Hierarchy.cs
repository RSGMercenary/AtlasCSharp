﻿using System;

namespace Atlas.Core.Collections.Hierarchy
{
	[Flags]
	public enum Hierarchy
	{
		Self = 1,
		Sibling = 2,
		Parent = 4,
		Child = 8,
		Ancestor = 16,
		Descendent = 32,
		Root = 64,
		Up = Parent | Ancestor | Root,
		Down = Child | Descendent,
		All = Self | Sibling | Parent | Child | Ancestor | Descendent | Root
	}
}