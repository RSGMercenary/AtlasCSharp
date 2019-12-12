﻿using Atlas.ECS.Components.Component;
using Atlas.ECS.Entities;
using System.Text;

namespace Atlas.ECS.Utilities
{
	public static class AtlasECS
	{
		#region Entities
		public static string AncestorsToString(this IEntity entity, int depth = -1, bool localNames = true, string indent = "")
		{
			var text = new StringBuilder();
			if(entity.Parent != null && depth != 0)
			{
				var parent = entity.Parent;
				text.Append(parent.AncestorsToString(depth - 1, localNames, indent));
				var ancestor = parent;
				while(ancestor != null && depth-- != 0)
				{
					text.Append("  ");
					ancestor = ancestor.Parent;
				}
			}
			text.Append(indent);
			text.AppendLine(localNames ? entity.LocalName : entity.GlobalName);
			return text.ToString();
		}

		public static string DescendantsToString(this IEntity entity, int depth = -1, bool localNames = true, string indent = "")
		{
			var text = new StringBuilder();
			text.Append(indent);
			text.AppendLine(localNames ? entity.LocalName : entity.GlobalName);
			if(depth-- != 0)
			{
				foreach(var child in entity.Children)
					text.Append(child.DescendantsToString(depth, localNames, indent + "  "));
			}
			return text.ToString();
		}

		/// <summary>
		/// Returns a formatted and indented string of the <see cref="IEntity"/> hierarchy.
		/// </summary>
		/// <param name="depth">Adds children recursively to the output until the given depth. -1 is the entire hierarchy.</param>
		/// <param name="addComponents">Adds components to the output.</param>
		/// <param name="addManagers">Adds component entities to the output.</param>
		/// <param name="addSystems">Adds systems to the output.</param>
		/// <param name="indent"></param>
		/// <returns></returns>
		public static string ToInfoString(this IEntity entity, int depth = -1, bool addComponents = true, bool addEntities = false, string indent = "", StringBuilder text = null)
		{
			text = text ?? new StringBuilder();

			var name = (entity.IsRoot) ? entity.GlobalName : $"Child {entity.ParentIndex + 1}";
			text.AppendLine($"{indent}{name}");
			text.AppendLine($"{indent}  {nameof(entity.GlobalName)}   = {entity.GlobalName}");
			text.AppendLine($"{indent}  {nameof(entity.LocalName)}    = {entity.LocalName}");
			text.AppendLine($"{indent}  {nameof(entity.AutoDispose)}  = {entity.AutoDispose}");
			text.AppendLine($"{indent}  {nameof(entity.Sleeping)}     = {entity.Sleeping}");
			text.AppendLine($"{indent}  {nameof(entity.FreeSleeping)} = {entity.FreeSleeping}");

			text.AppendLine($"{indent}  {nameof(Components)} ({entity.Components.Count})");
			if(addComponents)
			{
				int index = 0;
				foreach(var component in entity.Components.Values)
					component.ToInfoString(addEntities, ++index, $"{indent}    ", text);
			}

			text.AppendLine($"{indent}  {nameof(entity.Children)}   ({entity.Children.Count})");
			if(depth-- != 0)
			{
				foreach(var child in entity.Children)
					child.ToInfoString(depth, addComponents, addEntities, $"{indent}    ", text);
			}
			return text.ToString();
		}
		#endregion

		#region Components
		public static string ToInfoString(this IComponent component, bool addEntities, int index = 0, string indent = "", StringBuilder text = null)
		{
			text = text ?? new StringBuilder();
			text.Append($"{indent}Component");
			if(index > 0)
				text.Append($" {index}");
			text.AppendLine();
			text.AppendLine($"{indent}  Instance    = {component.GetType().FullName}");
			if(component.Manager != null)
				text.AppendLine($"{indent}  Interface   = {component.Manager.GetComponentType(component).FullName}");
			text.AppendLine($"{indent}  {nameof(component.AutoDispose)} = {component.AutoDispose}");
			text.AppendLine($"{indent}  {nameof(component.IsShareable)} = {component.IsShareable}");
			if(component.IsShareable)
			{
				text.AppendLine($"{indent}  Entities ({component.Managers.Count})");
				if(addEntities)
				{
					index = 0;
					foreach(var entity in component.Managers)
					{
						text.AppendLine($"{indent}    Entity {++index}");
						text.AppendLine($"{indent}      Interface  = {entity.GetComponentType(component).FullName}");
						text.AppendLine($"{indent}      {nameof(entity.GlobalName)} = {entity.GlobalName}");
					}
				}
			}
			return text.ToString();
		}
		#endregion
	}
}