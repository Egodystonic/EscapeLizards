// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 10 2014 at 14:12 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap {
	public static partial class Extensions {
		/// <summary>
		/// Determines if the given type can be instantiated (and, optionally, only via a no-args constructor).
		/// </summary>
		/// <param name="this">The extended Type.</param>
		/// <param name="onlyNoArgsConstructor">If true, this method will return false if the type is instantiable but
		/// does not define a no-args constructor.</param>
		/// <returns>True if the type is instantiable (according to <paramref name="onlyNoArgsConstructor"/>), false if not.</returns>
		public static bool IsInstantiable(this Type @this, bool onlyNoArgsConstructor = false) {
			if (@this == null) throw new ArgumentNullException("this", "IsInstantiable called on a null Type.");

			if (@this.IsAbstract || @this.IsArray || @this.IsEnum || @this.IsInterface || @this.IsPointer
				|| @this.IsStatic()) return false;

			if (onlyNoArgsConstructor && @this.GetConstructor(new Type[0]) == null) return false;

			return true;
		}

		/// <summary>
		/// Returns the default value for the subject Type (e.g. null for reference types, and the default for value types).
		/// </summary>
		/// <param name="this">The extended Type.</param>
		/// <returns>default(T), where T is the type represented by <paramref name="this"/>.</returns>
		public static object GetDefaultValue(this Type @this) {
			if (@this == null) throw new ArgumentNullException("this", "GetDefaultValue called on a null Type.");
			return @this.IsValueType ? Activator.CreateInstance(@this) : null;
		}

		/// <summary>
		/// Returns whether or not a given type is 'static' (e.g. abstract and sealed).
		/// </summary>
		/// <param name="this">The extended Type.</param>
		/// <returns>True if the given type is static, false if not.</returns>
		public static bool IsStatic(this Type @this) {
			if (@this == null) throw new ArgumentNullException("this", "IsStatic called on a null Type.");
			return @this.IsAbstract && @this.IsSealed;
		}

		/// <summary>
		/// Ascertains if the given type is a numeric type (e.g. <see cref="int"/>).
		/// </summary>
		/// <param name="this">The extended Type.</param>
		/// <returns>True if the type represents a numeric type, false if not.</returns>
		public static bool IsNumeric(this Type @this) {
			if (@this == null) throw new ArgumentNullException("this", "IsNumeric called on a null Type.");
			return Number.IsNumericType(@this);
		}

		/// <summary>
		/// Returns whether or not objects of this type can be copied byte-for-byte in to another part of the system memory without
		/// potential segmentation faults (i.e. the type contains no managed references such as <see cref="string"/>s). This function will
		/// always return <c>false</c> for non-<see cref="ValueType"/>s.
		/// </summary>
		/// <param name="this">The extended Type.</param>
		/// <returns>True if the type can be copied (blitted), or false if not.</returns>
		public static bool IsBlittable(this Type @this) {
			if (@this == null) throw new ArgumentNullException("this", "IsBlittable called on a null Type.");

			return @this.IsValueType 
				&& @this.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					.All(fieldInfo => fieldInfo.FieldType.IsValueType || fieldInfo.FieldType.IsPointer)
				&& (@this.IsExplicitLayout || @this.IsLayoutSequential);
		}

		/// <summary>
		/// Get the <see cref="Type"/> associated with the subject <see cref="TypeCode"/>.
		/// </summary>
		/// <param name="this">The extended TypeCode.</param>
		/// <returns>A <see cref="Type"/> that <paramref name="this"/> represents.</returns>
		public static Type ToType(this TypeCode @this) {
			switch (@this) {
				case TypeCode.Boolean: return typeof(bool);
				case TypeCode.Byte: return typeof(byte);
				case TypeCode.Char: return typeof(char);
				case TypeCode.DBNull: return typeof(DBNull);
				case TypeCode.DateTime: return typeof(DateTime);
				case TypeCode.Decimal: return typeof(Decimal);
				case TypeCode.Double: return typeof(Double);
				case TypeCode.Int16: return typeof(Int16);
				case TypeCode.Int32: return typeof(Int32);
				case TypeCode.Int64: return typeof(Int64);
				case TypeCode.SByte: return typeof(SByte);
				case TypeCode.Single: return typeof(Single);
				case TypeCode.String: return typeof(string);
				case TypeCode.UInt16: return typeof(UInt16);
				case TypeCode.UInt32: return typeof(UInt32);
				case TypeCode.UInt64: return typeof(UInt64);
				default: return typeof(object);
			}
		}
	}
}