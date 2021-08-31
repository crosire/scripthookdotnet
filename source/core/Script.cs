//
// Copyright (C) 2015 crosire & contributors
// License: https://github.com/crosire/scripthookvdotnet#license
//

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;

namespace SHVDN
{
	public class Script
	{
		DateTime resumeTime = DateTime.MinValue;
		bool willInvokeAbortEvent;
		internal ConcurrentQueue<Tuple<bool, KeyEventArgs>> keyboardEvents = new ConcurrentQueue<Tuple<bool, KeyEventArgs>>();

		/// <summary>
		/// Gets or sets the interval in ms between each <see cref="Tick"/>.
		/// </summary>
		public int Interval { get; set; }

		/// <summary>
		/// Gets whether executing of this script is paused or not.
		/// </summary>
		public bool IsPaused { get; private set; }

		/// <summary>
		/// Gets the status of this script.
		/// So <c>true</c> if it is running and <c>false</c> if it was aborted.
		/// </summary>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// Gets whether this is the currently executing script.
		/// </summary>
		public bool IsExecuting => ScriptDomain.ExecutingScript == this;

		/// <summary>
		/// An event that is raised every tick of the script.
		/// </summary>
		public event EventHandler Tick;
		/// <summary>
		/// An event that is raised when this script gets aborted for any reason.
		/// </summary>
		public event EventHandler Aborted;

		/// <summary>
		/// An event that is raised when a key is lifted.
		/// </summary>
		public event KeyEventHandler KeyUp;
		/// <summary>
		/// An event that is raised when a key is first pressed.
		/// </summary>
		public event KeyEventHandler KeyDown;

		/// <summary>
		/// Gets the instance name of this script.
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		/// Gets the path to the file that contains this script.
		/// </summary>
		public string Filename { get; internal set; }

		/// <summary>
		/// Gets the instance of the script.
		/// </summary>
		public object ScriptInstance { get; internal set; }

		/// <summary>
		/// The main execution logic of all scripts.
		/// </summary>
		internal void MainLoop()
		{
			if (!IsRunning || IsPaused || DateTime.UtcNow < resumeTime)
			{
				return;
			}

			// Process keyboard events
			while (keyboardEvents.TryDequeue(out Tuple<bool, KeyEventArgs> ev))
			{
				try
				{
					if (!ev.Item1)
						KeyUp?.Invoke(this, ev.Item2);
					else
						KeyDown?.Invoke(this, ev.Item2);
				}
				catch (Exception ex)
				{
					ScriptDomain.HandleUnhandledException(this, new UnhandledExceptionEventArgs(ex, false));
					break; // Break out of key event loop, but continue to run script
				}
			}

			try
			{
				Tick?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				ScriptDomain.HandleUnhandledException(this, new UnhandledExceptionEventArgs(ex, true));

				// An exception during tick is fatal, so abort the script and stop main loop
				Abort();
			}

			if (willInvokeAbortEvent)
			{
				try
				{
					Aborted?.Invoke(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					ScriptDomain.HandleUnhandledException(this, new UnhandledExceptionEventArgs(ex, true));
				}

				Log.Message(Log.Level.Warning, "Aborted script ", Name, ".");
			}
			else
			{
				// Yield execution to next tick
				Wait(Interval);
			}
		}

		/// <summary>
		/// Starts execution of this script.
		/// </summary>
		public void Start()
		{
			IsRunning = true;

			Log.Message(Log.Level.Info, "Started script ", Name, ".");
		}
		/// <summary>
		/// Aborts execution of this script.
		/// </summary>
		public void Abort()
		{
			IsRunning = false;
			willInvokeAbortEvent = true;
		}

		/// <summary>
		/// Pauses execution of this script.
		/// </summary>
		public void Pause()
		{
			if (IsPaused)
				return; // Pause status has not changed, so nothing to do

			IsPaused = true;

			Log.Message(Log.Level.Info, "Paused script ", Name, ".");
		}
		/// <summary>
		/// Resumes execution of this script.
		/// </summary>
		public void Resume()
		{
			if (!IsPaused)
				return;

			IsPaused = false;

			Log.Message(Log.Level.Info, "Resumed script ", Name, ".");
		}

		/// <summary>
		/// Pause execution of this script for the specified time.
		/// </summary>
		/// <param name="ms">The time in milliseconds to pause.</param>
		public void Wait(int ms)
		{
			resumeTime = DateTime.UtcNow + TimeSpan.FromMilliseconds(ms);
		}
	}
}
