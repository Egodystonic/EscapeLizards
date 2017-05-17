// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 24 02 2015 at 14:54 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// RenderCommandQueue Item: Represents a queued item in the render-command-chain. 
	/// Each item can be either a <see cref="RenderCommand"/> or an <see cref="Action"/>.
	/// </summary>
	public struct RCQItem : IEquatable<RCQItem> {
		/// <summary>
		/// If this RCQItem represents a queued render command, this field contains the value of that command. Otherwise, this field is <c>null</c>.
		/// </summary>
		public readonly RenderCommand? RenderCommand;
		/// <summary>
		/// If this RCQItem represents a deferred action, this field contains the value of that action. Otherwise, this field is <c>null</c>.
		/// </summary>
		public readonly Action Action;

		/// <summary>
		/// Whether or not this RCQItem represents a deferred action or a deferred render command. If this property returns <c>true</c>,
		/// then <see cref="Action"/> will have a value. Otherwise, if this property returns <c>false</c>, <see cref="RenderCommand"/> will
		/// have a value.
		/// </summary>
		public bool IsDeferredAction {
			get {
				return Action != null;
			}
		}

		internal RCQItem(RenderCommand renderCommand) {
			RenderCommand = renderCommand;
			Action = null;
		}

		internal RCQItem(Action action) {
			RenderCommand = null;
			Action = action;
		}

		/// <summary>
		/// Returns a string representing this instance and its current state/values.
		/// </summary>
		/// <returns>
		/// A new string that details this object.
		/// </returns>
		public override string ToString() {
			return "Queued " + (IsDeferredAction ? "Action" : RenderCommand.ToString());
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(RCQItem other) {
			return RenderCommand == other.RenderCommand && Equals(Action, other.Action);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param>
		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			return obj is RCQItem && Equals((RCQItem)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				return (RenderCommand.GetHashCode() * 397) ^ (Action != null ? Action.GetHashCode() : 0);
			}
		}

		/// <summary>
		/// Determines whether or not the two RCQItems represent either two identical <see cref="RenderCommand"/>s, or the same <see cref="Action"/>.
		/// </summary>
		/// <param name="left">The first RCQItem.</param>
		/// <param name="right">The second RCQItem.</param>
		/// <returns>True if the two RCQItems represent identical queued commands or actions. False if not.</returns>
		public static bool operator ==(RCQItem left, RCQItem right) {
			return left.Equals(right);
		}

		/// <summary>
		/// Determines whether or not the two RCQItems represent different <see cref="RenderCommand"/>s, or <see cref="Action"/>s.
		/// </summary>
		/// <param name="left">The first RCQItem.</param>
		/// <param name="right">The second RCQItem.</param>
		/// <returns>True if the two RCQItems represent different queued commands or actions. False if the commands/actions are identical.</returns>
		public static bool operator !=(RCQItem left, RCQItem right) {
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns the <see cref="RenderCommand"/> that the given <paramref name="operand"/> represents.
		/// </summary>
		/// <param name="operand">The RCQItem that represents a <see cref="Rendering.RenderCommand"/>. If <see cref="IsDeferredAction"/>
		/// is <c>true</c>, returns <c>null</c>.</param>
		/// <returns><c>operand.RenderCommand</c>.</returns>
		public static implicit operator RenderCommand?(RCQItem operand) {
			return operand.RenderCommand;
		}

		/// <summary>
		/// Returns the <see cref="Action"/> that the given <paramref name="operand"/> represents.
		/// </summary>
		/// <param name="operand">The RCQItem that represents an <see cref="System.Action"/>. If <see cref="IsDeferredAction"/>
		/// is <c>false</c>, returns null.</param>
		/// <returns><c>operand.Action</c>.</returns>
		public static implicit operator Action(RCQItem operand) {
			return operand.Action;
		}
	}
}