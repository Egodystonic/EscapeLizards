// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 16 10 2014 at 15:16 by Ben Bowen

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ophidian.Losgap.Interop;

namespace Ophidian.Losgap {
	/// <summary>
	/// Represents the root control interface for the LOSGAP framework.
	/// </summary>
	public static partial class LosgapSystem {
		private const string DEFAULT_APP_NAME = "LosgapApp";
		/// <summary>
		/// The version of the LOSGAP framework that this system is running.
		/// </summary>
		public static readonly Version Version = new Version(0, 2);
		private static readonly object staticMutationLock = new object();
		private static string applicationName = DEFAULT_APP_NAME;
		private static DirectoryInfo installationDirectory = null;
		private static DirectoryInfo mutableDataDirectory = null;
		private static uint maxThreadCount = 64;
		private static int threadOversubscriptionFactor = 0;
		private static ThreadPriority slaveThreadPriority = ThreadPriority.Normal;

		/// <summary>
		/// Get or set the name of the application, used in various places.
		/// </summary>
		public static string ApplicationName {
			get {
				lock (staticMutationLock) {
					return applicationName;
				}
			}
			set {
				lock (staticMutationLock) {
					if (value == null) throw new ArgumentNullException("value");
					applicationName = value;
				}
			}
		}

		/// <summary>
		/// The location used by LOSGAP and your application to read and write transient or mutable persistent data.
		/// Data in this directory may be read, written, deleted, or moved at any time with no access issues.
		/// </summary>
		/// <remarks>
		/// If you do not wish to use the default value, 
		/// this property should be practically the first thing you set (i.e. before adding any modules, logging, etc).
		/// Attempting to set the value at any other time may result in a <see cref="InvalidOperationException"/> being thrown.
		/// Furthermore, this value can only be set once. 
		/// <para>
		/// You do not need to set any value; a sensible default will be chosen instead.
		/// </para>
		/// <para>
		/// If the given directory can not be found, is not valid, could not be accessed, or could not be created,
		/// an <see cref="ArgumentException"/> will be thrown, and the value will not be set.
		/// </para>
		/// </remarks>
		public static DirectoryInfo MutableDataDirectory {
			get {
				lock (staticMutationLock) {
					if (mutableDataDirectory == null) {
						MutableDataDirectory = new DirectoryInfo( // Note: Using property to set value in order to go through all the checks!
							Path.Combine(
								Environment.GetFolderPath(
									Environment.SpecialFolder.ApplicationData,
									Environment.SpecialFolderOption.Create
								),
								applicationName + @"\"
							)
						);
					}
					return mutableDataDirectory;
				}
			}
			set {
				if (value == null) throw new ArgumentNullException("value");
				lock (staticMutationLock) {
					if (mutableDataDirectory != null) {
						throw new InvalidOperationException("Can not set the data directory at this time.");
					}

					try {
						if (!value.Exists) value.Create();
					}
					catch (IOException e) {
						throw new ArgumentException("Provided value does not exist, and could not be created.", "value", e);
					}
					catch (UnauthorizedAccessException e) {
						throw new ArgumentException("Provided value does not exist, and could not be created.", "value", e);
					}

					mutableDataDirectory = value;
				}
			}
		}

		/// <summary>
		/// The location used by various parts of the framework to locate assets, files, etc. that were installed with the application.
		/// Data in this directory should be treated as read-only: Attempting to modify content at this location may result in an 
		/// access violation.
		/// </summary>
		/// <remarks>
		/// If you do not wish to use the default value, 
		/// this property should be practically the first thing you set (i.e. before adding any modules, logging, etc).
		/// Attempting to set the value at any other time may result in a <see cref="InvalidOperationException"/> being thrown.
		/// Furthermore, this value can only be set once. 
		/// <para>
		/// You do not need to set any value; a sensible default will be chosen instead.
		/// </para>
		/// <para>
		/// If the given directory can not be found, is not valid, could not be accessed, or could not be created,
		/// an <see cref="ArgumentException"/> will be thrown, and the value will not be set.
		/// </para>
		/// </remarks>
		public static DirectoryInfo InstallationDirectory {
			get {
				lock (staticMutationLock) {
					if (installationDirectory == null) {
						InstallationDirectory = new DirectoryInfo( // Note: Using property to set value in order to go through all the checks!
							Path.Combine(
								Environment.GetFolderPath(
									Environment.SpecialFolder.ProgramFiles,
									Environment.SpecialFolderOption.Create
								),
								applicationName + @"\"
							)
						);
					}
					return installationDirectory;
				}
			}
			set {
				if (value == null) throw new ArgumentNullException("value");
				lock (staticMutationLock) {
					if (installationDirectory != null) {
						throw new InvalidOperationException("Can not set the installation directory at this time.");
					}

					try {
						if (!value.Exists) value.Create();
					}
					catch (IOException e) {
						throw new ArgumentException("Provided value does not exist, and could not be created.", "value", e);
					}
					catch (UnauthorizedAccessException e) {
						throw new ArgumentException("Provided value does not exist, and could not be created.", "value", e);
					}

					installationDirectory = value;
				}
			}
		}

		/// <summary>
		/// The maximum number of threads that the pipeline is permitted to spawn and use for execution.
		/// </summary>
		/// <remarks>
		/// Attempting to set this value while <see cref="IsRunning"/> is true will result in an <see cref="InvalidOperationException"/> being
		/// thrown. A value of 0 is not permitted.
		/// </remarks>
		public static uint MaxThreadCount {
			get {
				lock (staticMutationLock) {
					return maxThreadCount;
				}
			}
			set {
				if (value < 1) throw new ArgumentOutOfRangeException("value", "MaxThreads must be positive.");
				lock (staticMutationLock) {
					if (IsRunning) throw new InvalidOperationException("Can not set max thread count while pipeline is running.");
					maxThreadCount = value;
				}
				Logger.Log("Max pipeline thread count set to " + value + ".");
			}
		}

		/// <summary>
		/// The number of threads that the pipeline will create in excess of the number of 
		/// <see cref="Environment.ProcessorCount">logical cores</see> that are present on this machine.
		/// </summary>
		/// <remarks>
		/// Attempting to set this value while <see cref="IsRunning"/> is true will result in an <see cref="InvalidOperationException"/> being
		/// thrown. A negative value will result in an undersubscription. 
		/// <para>
		/// The number of threads eventually spawned by the pipeline will never go below 1 or above <see cref="MaxThreadCount"/>, regardless of
		/// the value of this property.
		/// </para>
		/// </remarks>
		public static int ThreadOversubscriptionFactor {
			get {
				lock (staticMutationLock) {
					return threadOversubscriptionFactor;
				}
			}
			set {
				lock (staticMutationLock) {
					if (IsRunning) throw new InvalidOperationException("Can not set oversubscription factor while pipeline is running.");
					threadOversubscriptionFactor = value;
				}
				Logger.Log("Pipeline thread oversubscription factor set to " + value + ".");
			}
		}

		/// <summary>
		/// The priority given to slave threads.
		/// </summary>
		/// <remarks>
		/// Attempting to set this value while <see cref="IsRunning"/> is true will result in an <see cref="InvalidOperationException"/> being
		/// thrown.
		/// </remarks>
		public static ThreadPriority SlaveThreadPriority {
			get {
				lock (staticMutationLock) {
					return slaveThreadPriority;
				}
			}
			set {
				lock (staticMutationLock) {
					if (IsRunning) throw new InvalidOperationException("Can not set slave priority while pipeline is running.");
					slaveThreadPriority = value;
				}
				Logger.Log("Pipeline slave thread priority set to " + value + ".");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline",
			Justification = "The static initializer executes code, therefore is required.")]
		static LosgapSystem() {
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		}		

		/// <summary>
		/// Invokes the given <paramref name="action"/> on the <see cref="MasterThread"/> (that is the thread that 
		/// <see cref="Start"/> was called from).
		/// This function can create a heavy performance hit for the current frame, so it should only be used when necessary.
		/// </summary>
		/// <remarks>
		/// Call this method when you must ensure that all accesses to a particular resource must occur from the same thread (for example, 
		/// when working with the Windows event message pump).
		/// <para>
		/// This method will block until the <paramref name="action"/> has been completed. Any exceptions that occur when invoking
		/// <paramref name="action"/> will be thrown back to the caller.
		/// </para>
		/// </remarks>
		/// <param name="action">The action to invoke on the <see cref="MasterThread"/>.</param>
		public static void InvokeOnMaster(Action action) {
			InvokeOnMaster(action, true);
		}

		/// <summary>
		/// Invokes the given <paramref name="action"/> on the <see cref="MasterThread"/> (that is the thread that 
		/// <see cref="Start"/> was called from).
		/// This function can create a moderate performance hit for the current frame, so it should only be used when necessary.
		/// </summary>
		/// <remarks>
		/// Call this method when you must ensure that all accesses to a particular resource must occur from the same thread (for example, 
		/// when working with the Windows API event message pump).
		/// <para>
		/// This method will not block the caller, and will usually return before <paramref name="action"/> has been completed. 
		/// Any exceptions that occur when invoking <paramref name="action"/> will cause the application to close (and a log message will be written).
		/// </para>
		/// </remarks>
		/// <param name="action">The action to invoke on the <see cref="MasterThread"/>.</param>
		public static void InvokeOnMasterAsync(Action action) {
			InvokeOnMaster(action, false);
		}

		/// <summary>
		/// Invokes the given <paramref name="func"/> on the <see cref="MasterThread"/> (that is the thread that <see cref="Start"/> was called from).
		/// This function can create a heavy performance hit for the current frame, so it should only be used when necessary.
		/// </summary>
		/// <remarks>
		/// Call this method when you must ensure that all accesses to a particular resource must occur from the same thread (for example, 
		/// when working with the Windows API GDI+ message pump).
		/// <para>
		/// This method will block until the <paramref name="func"/> has been completed. Any exceptions that occur when invoking
		/// <paramref name="func"/> will be thrown back to the caller.
		/// </para>
		/// </remarks>
		/// <typeparam name="TResult">The return type of the <paramref name="func"/>.</typeparam>
		/// <param name="func">The function to invoke on the <see cref="MasterThread"/>.</param>
		/// <returns>The <typeparamref name="TResult"/> returned from the invocation of <paramref name="func"/>.</returns>
		public static TResult InvokeOnMaster<TResult>(Func<TResult> func) {
			Assure.NotNull(func);
			TResult result = default(TResult);
			InvokeOnMaster(() => {
				result = func();
			});
			return result;
		}

		/// <summary>
		/// Stops the application and logs the given error message (and optional exception) in the log file.
		/// The application will then close (e.g. it is not possible to restart the application with <see cref="Start"/>).
		/// </summary>
		/// <remarks>
		/// The application may not stop immediately, as the user is first asked whether they would like to open the log file. Use
		/// <see cref="Environment.FailFast(string,System.Exception)"/> if you need to exit immediately.
		/// </remarks>
		/// <param name="errorMessage">The error message to log and display to the user.</param>
		/// <param name="e">An exception, if any, associated with the error.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "Caught so we don't create an infinite loop of unhandled exceptions triggering ExitWithError etc.")]
		public static void ExitWithError(string errorMessage, Exception e = null) {
			try {
				if (errorMessage == null) errorMessage = "<No message>";
				Logger.Fatal(errorMessage, e);
				string popupText = "Unfortunately, " + ApplicationName + " has had to stop due to an unexpected error. ";
				popupText += "A description of the error follows.";
				popupText += Environment.NewLine + Environment.NewLine;
				popupText += errorMessage;
				popupText += Environment.NewLine + Environment.NewLine;
				popupText += "There may be more information in the log file. Press OK to open the log file now.";
				bool openLogFile = PopupUtils.ShowConfirmationPopup(ApplicationName + ": Fatal Error", popupText);
				if (openLogFile) {
					StringBuilder outLogApp = new StringBuilder(1024);
					string logApp;
					if ((int) FindExecutable(Logger.FileLocation, MutableDataDirectory.FullName, outLogApp) > 32) {
						logApp = outLogApp.ToString();
					}
					else logApp = @"C:\Windows\Notepad.exe";
					Logger.LoggingProvider.Dispose();
					ProcessStartInfo psi = new ProcessStartInfo(logApp) {
						UseShellExecute = false,
						Arguments = Path.Combine(MutableDataDirectory.FullName, Logger.FileLocation)
					};
					Process logViewProcess = new Process {
						StartInfo = psi
					};
					logViewProcess.Start();
				}
			}
			catch { }
			finally {
				Environment.Exit(-1);
			}
		}

		private static void InvokeOnMaster(Action action, bool synchronous) {
			Assure.NotNull(action);
			PipelineProcessor processorLocal;
			lock (staticMutationLock) {
				if (!IsRunning) {
					try {
						ThrowIfCurrentThreadIsNotMaster();
					}
					catch (InvalidOperationException e) {
						throw new InvalidOperationException("Can not execute given operation from any thread but the master when the " +
							"pipeline is not running. Either start the system or move the operation to the master thread.", e);
					}
					action();
					return;
				}
				processorLocal = processor;
			}
			PipelineMasterInvocation pmi = PipelineMasterInvocation.Create(action, synchronous);
			processorLocal.InvokeOnMaster(pmi);
			if (pmi.IsSynchronousCall) {
				if (pmi.RaisedException != null) ExceptionDispatchInfo.Capture(pmi.RaisedException).Throw();
				PipelineMasterInvocation.Free(pmi);
			}
		}

		private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
			Logger.Fatal("Memory Usage: " + new ByteSize(Process.GetCurrentProcess().WorkingSet64));
			Logger.Fatal("Memory Usage (Managed): " + new ByteSize(GC.GetTotalMemory(false)));
#if DEBUG // Don't exit on debug mode so that the IDE can catch the exception and we can inspect it
			try {
				Logger.Fatal("Unhandled exception (" + (e.ExceptionObject as Exception).GetAllMessages() + ")", (e.ExceptionObject as Exception));
			}
			catch (Exception exception) {
				Debug.WriteLine("Failed to write exception details to log file: " + exception);
			}
#else
			ExitWithError("Unhandled exception (" + (e.ExceptionObject as Exception).GetAllMessages() + ")", (e.ExceptionObject as Exception));
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0",
			Justification = "Private function"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "2",
			Justification = "Private function"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "1",
			Justification = "Private function"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", 
			Justification = "Prefer to keep this one quietly in here, as it's no use to anyone but this class."), 
		DllImport("shell32.dll")]
		private static extern IntPtr FindExecutable(string lpFile, string lpDirectory, [Out] StringBuilder lpResult);
	}
}