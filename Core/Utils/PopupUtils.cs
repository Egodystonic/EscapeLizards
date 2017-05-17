// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 06 03 2015 at 08:07 by Ben Bowen

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ophidian.Losgap {
	/// <summary>
	/// A static class containing methods that make it possible to show native popup dialogs to the user.
	/// </summary>
	public static class PopupUtils {
		/// <summary>
		/// Shows a popup to the user requesting them to confirm an action by presenting the given <paramref name="text"/>/<paramref name="title"/>
		/// and an "OK" and "Cancel" button.
		/// </summary>
		/// <param name="title">The title of the popup.</param>
		/// <param name="text">The text to display.</param>
		/// <returns>True if the user clicked "OK", false if the user clicked "Cancel".</returns>
		public static bool ShowConfirmationPopup(string title, string text) {
			return MessageBox(IntPtr.Zero, text, title, 0x21U) == 1;
		}

		/// <summary>
		/// Shows a popup to the user with the given <paramref name="text"/>/<paramref name="title"/>
		/// and an "OK" button.
		/// </summary>
		/// <param name="title">The title of the popup.</param>
		/// <param name="text">The text to display.</param>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", 
			MessageId = "Ophidian.Losgap.PopupUtils.MessageBox(System.IntPtr,System.String,System.String,System.UInt32)",
			Justification = "The return value is only useful when you wish to get the user's input.")]
		public static void ShowNotificationPopup(string title, string text) {
			MessageBox(IntPtr.Zero, text, title, 0x40U);
		}

		[SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api",
			Justification = "I don't want to include the System.Windows.Forms library just to be able to pop up a messagebox."),
		SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass",
			Justification = "It's unnecessary to create another static class for one method, I think it's nicer to just leave it here."),
		DllImport("user32.dll", EntryPoint = "MessageBoxW",
			CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall)]
		private static extern uint MessageBox(
			IntPtr hWnd,
			[MarshalAs(UnmanagedType.LPTStr)] string text,
			[MarshalAs(UnmanagedType.LPTStr)] string caption,
			uint options
		);
	}
}