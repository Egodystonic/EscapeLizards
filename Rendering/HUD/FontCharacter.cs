// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 07 05 2015 at 18:01 by Ben Bowen

using System;
using System.Collections.Generic;

namespace Ophidian.Losgap.Rendering {
	internal struct FontCharacter : IEquatable<FontCharacter> {
		public readonly char UnicodeValue;
		public readonly Rectangle Boundary;
		public readonly ModelHandle ModelHandle;
		public readonly FragmentShader FragmentShader;
		public readonly ShaderResourceView FontMapResourceView;
		public readonly int YOffset;

		public FontCharacter(char unicodeValue, Rectangle boundary, ModelHandle modelHandle, FragmentShader fragmentShader, ShaderResourceView fontMapResourceView, int yOffset) {
			UnicodeValue = unicodeValue;
			Boundary = boundary;
			ModelHandle = modelHandle;
			FragmentShader = fragmentShader;
			FontMapResourceView = fontMapResourceView;
			YOffset = yOffset;
		}

		public Material CreateNewColorableMaterial() {
			Material result = new Material("Colorable Font Char " + UnicodeValue, FragmentShader);
			result.SetMaterialResource((ResourceViewBinding) FragmentShader.GetBindingByIdentifier(Font.CHAR_MAP_SHADER_RES_NAME), FontMapResourceView);
			return result;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(FontCharacter other) {
			return UnicodeValue == other.UnicodeValue && Boundary.Equals(other.Boundary) && ModelHandle.Equals(other.ModelHandle) && Equals(FragmentShader, other.FragmentShader) && Equals(FontMapResourceView, other.FontMapResourceView);
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
			return obj is FontCharacter && Equals((FontCharacter)obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				int hashCode = UnicodeValue.GetHashCode();
				hashCode = (hashCode * 397) ^ Boundary.GetHashCode();
				hashCode = (hashCode * 397) ^ ModelHandle.GetHashCode();
				hashCode = (hashCode * 397) ^ (FragmentShader != null ? FragmentShader.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (FontMapResourceView != null ? FontMapResourceView.GetHashCode() : 0);
				return hashCode;
			}
		}

		public static bool operator ==(FontCharacter left, FontCharacter right) {
			return left.Equals(right);
		}

		public static bool operator !=(FontCharacter left, FontCharacter right) {
			return !left.Equals(right);
		}
	}
}