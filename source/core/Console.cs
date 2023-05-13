//
// Copyright (C) 2015 crosire & contributors
// License: https://github.com/crosire/scripthookvdotnet#license
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SHVDN
{
	public class Console : MarshalByRefObject
	{
		int cursorPos = 0;
		int commandPos = -1;
		int currentPage = 1;
		bool isOpen = false;
		string input = string.Empty;
		List<string> lineHistory = new();
		List<string> commandHistory; // This must be set via CommandHistory property
		ConcurrentQueue<string[]> outputQueue = new();
		Dictionary<string, List<ConsoleCommand>> commands = new();
		int lastClosedTickCount;
		bool shouldBlockControls;
		Task<MethodInfo> compilerTask;
		const int BASE_WIDTH = 1280;
		const int BASE_HEIGHT = 720;
		const int CONSOLE_WIDTH = BASE_WIDTH;
		const int CONSOLE_HEIGHT = BASE_HEIGHT / 3;
		const int INPUT_HEIGHT = 20;
		const int LINES_PER_PAGE = 16;

		static readonly Color InputColor = Color.White;
		static readonly Color InputColorBusy = Color.DarkGray;
		static readonly Color OutputColor = Color.White;
		static readonly Color PrefixColor = Color.FromArgb(255, 52, 152, 219);
		static readonly Color BackgroundColor = Color.FromArgb(200, Color.Black);
		static readonly Color AltBackgroundColor = Color.FromArgb(200, 52, 73, 94);

		[DllImport("user32.dll")]
		static extern int ToUnicode(
			uint virtualKeyCode, uint scanCode, byte[] keyboardState,
			[Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder receivingBuffer, int bufferSize, uint flags);

		/// <summary>
		/// Gets or sets whether the console is open.
		/// </summary>
		public bool IsOpen
		{
			get => isOpen;
			set
			{
				isOpen = value;
				DisableControlsThisFrame();
				if (isOpen) return;

				lastClosedTickCount = Environment.TickCount + 200; // Hack so the input gets blocked long enough
				shouldBlockControls = true;
			}
		}

		/// <summary>
		/// Gets or sets the command history. This is used to avoid losing the command history on SHVDN reloading.
		/// </summary>
		public List<string> CommandHistory
		{
			get => commandHistory;
			set => commandHistory = value;
		}

		/// <summary>
		/// Register the specified method as a console command.
		/// </summary>
		/// <param name="command">The command attribute of the method.</param>
		/// <param name="methodInfo">The method information.</param>
		public void RegisterCommand(ConsoleCommand command, MethodInfo methodInfo)
		{
			command.MethodInfo = methodInfo;

			if (!commands.ContainsKey(command.Namespace))
				commands[command.Namespace] = new List<ConsoleCommand>();
			commands[command.Namespace].Add(command);
		}
		/// <summary>
		/// Register all methods with a <see cref="ConsoleCommand"/> attribute in the specified type as console commands.
		/// </summary>
		/// <param name="type">The type to search for console command methods.</param>
		public void RegisterCommands(Type type)
		{
			foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
			{
				try
				{
					foreach (var attribute in method.GetCustomAttributes<ConsoleCommand>(true))
					{
						RegisterCommand(attribute, method);
					}
				}
				catch (Exception ex)
				{
					Log.Message(Log.Level.Error, "Failed to search for console commands in ", type.FullName, ".", method.Name, ": ", ex.ToString());
				}
			}
		}
		/// <summary>
		/// Unregister all methods with a <see cref="ConsoleCommand"/> attribute that were previously registered.
		/// </summary>
		/// <param name="type">The type to search for console command methods.</param>
		public void UnregisterCommands(Type type)
		{
			foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
			{
				var space = method.DeclaringType.FullName;

				if (!commands.TryGetValue(space, out var command)) continue;

				command.RemoveAll(x => x.MethodInfo == method);

				if (command.Count == 0)
					commands.Remove(space);
			}
		}

		/// <summary>
		/// Add text lines to the console. This call is thread-safe.
		/// </summary>
		/// <param name="prefix">The prefix for each line.</param>
		/// <param name="messages">The lines to add to the console.</param>
		void AddLines(string prefix, string[] messages)
		{
			AddLines(prefix, messages, "~w~");
		}
		/// <summary>
		/// Add colored text lines to the console. This call is thread-safe.
		/// </summary>
		/// <param name="prefix">The prefix for each line.</param>
		/// <param name="messages">The lines to add to the console.</param>
		/// <param name="color">The color of those lines.</param>
		void AddLines(string prefix, string[] messages, string color)
		{
			for (var i = 0; i < messages.Length; i++) // Add proper styling
				messages[i] = $"~c~[{DateTime.Now.ToString("HH:mm:ss")}] ~w~{prefix} {color}{messages[i]}";

			outputQueue.Enqueue(messages);
		}
		/// <summary>
		/// Add text to the console input line.
		/// </summary>
		/// <param name="text">The text to add.</param>
		void AddToInput(string text)
		{
			if (string.IsNullOrEmpty(text))
				return;

			input = input.Insert(cursorPos, text);
			cursorPos += text.Length;
		}
		/// <summary>
		/// Paste clipboard content into the console input line.
		/// </summary>
		void AddClipboardContent()
		{
			var text = Clipboard.GetText();
			text = text.Replace("\n", string.Empty); // TODO Keep this?

			AddToInput(text);
		}

		/// <summary>
		/// Clear the console input line.
		/// </summary>
		void ClearInput()
		{
			input = string.Empty;
			cursorPos = 0;
		}
		/// <summary>
		/// Clears the console output.
		/// </summary>
		public void Clear()
		{
			lineHistory.Clear();
			currentPage = 1;
		}

		/// <summary>
		/// Writes an info message to the console.
		/// </summary>
		/// <param name="msg">The composite format string.</param>
		/// <param name="args">The formatting arguments.</param>
		public void PrintInfo(string msg, params object[] args)
		{
			if (args.Length > 0)
				msg = String.Format(msg, args);
			AddLines("[~b~INFO~w~] ", msg.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
		}
		/// <summary>
		/// Writes an error message to the console.
		/// </summary>
		/// <param name="msg">The composite format string.</param>
		/// <param name="args">The formatting arguments.</param>
		public void PrintError(string msg, params object[] args)
		{
			if (args.Length > 0)
				msg = String.Format(msg, args);
			AddLines("[~r~ERROR~w~] ", msg.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
		}
		/// <summary>
		/// Writes a warning message to the console.
		/// </summary>
		/// <param name="msg">The composite format string.</param>
		/// <param name="args">The formatting arguments.</param>
		public void PrintWarning(string msg, params object[] args)
		{
			if (args.Length > 0)
				msg = String.Format(msg, args);
			AddLines("[~o~WARNING~w~] ", msg.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
		}

		/// <summary>
		/// Writes the help text for all commands to the console.
		/// </summary>
		internal void PrintHelpText()
		{
			var help = new StringBuilder();
			foreach (var space in commands.Keys)
			{
				help.AppendLine($"[{space}]");
				foreach (var command in commands[space])
				{
					help.Append("    ~h~" + command.Name + "(");
					foreach (var arg in command.MethodInfo.GetParameters())
						help.Append(arg.ParameterType.Name + " " + arg.Name + ",");
					if (command.MethodInfo.GetParameters().Length > 0)
						help.Length--; // Remove trailing comma
					if (command.Help.Length > 0)
						help.AppendLine(")~h~: " + command.Help);
					else
						help.AppendLine(")~h~");
				}
			}

			PrintInfo(help.ToString());
		}
		/// <summary>
		/// Writes the help text for the specified command to the console.
		/// </summary>
		/// <param name="commandName">The command name to check.</param>
		internal void PrintHelpText(string commandName)
		{
			foreach (var space in commands.Keys)
			{
				foreach (var command in commands[space])
				{
					if (command.Name != commandName) continue;
					PrintInfo(command.Name + ": " + command.Help);
					return;
				}
			}
		}

		/// <summary>
		/// Main execution logic of the console.
		/// </summary>
		internal void DoTick()
		{
			var nowTickCount = Environment.TickCount;

			// Execute compiled input line script
			if (compilerTask != null && compilerTask.IsCompleted)
			{
				if (compilerTask.Result != null)
				{
					try
					{
						var result = compilerTask.Result.Invoke(null, null);
						if (result != null)
							PrintInfo($"[Return Value]: {result}");
					}
					catch (TargetInvocationException ex)
					{
						PrintError($"[Exception]: {ex.InnerException.ToString()}");
					}
				}

				ClearInput();

				// Reset compiler task
				compilerTask = null;
			}

			// Add lines from concurrent queue to history
			if (outputQueue.TryDequeue(out var lines))
				foreach (var line in lines)
					lineHistory.Add(line);

			if (!IsOpen)
			{
				// Hack so the input gets blocked long enough
				if ((lastClosedTickCount - nowTickCount) > 0)
				{
					if (shouldBlockControls)
					{
						DisableControlsThisFrame();
					}
				}
				// The console is not open for more than about 24.9 days, calculating the elapsed time with 2 int tick count vars doesn't do the job
				else if (shouldBlockControls)
				{
					shouldBlockControls = false;
				}
				return; // Nothing more to do here when the console is not open
			}

			// Disable controls while the console is open
			DisableControlsThisFrame();

			// Draw background
			DrawRect(0, 0, CONSOLE_WIDTH, CONSOLE_HEIGHT, BackgroundColor);
			// Draw input field
			DrawRect(0, CONSOLE_HEIGHT, CONSOLE_WIDTH, INPUT_HEIGHT, AltBackgroundColor);
			DrawRect(0, CONSOLE_HEIGHT + INPUT_HEIGHT, 80, INPUT_HEIGHT, AltBackgroundColor);
			// Draw input prefix
			DrawText(0, CONSOLE_HEIGHT, "$>", PrefixColor);
			// Draw input text
			DrawText(25, CONSOLE_HEIGHT, input, compilerTask == null ? InputColor : InputColorBusy);
			// Draw page information
			DrawText(5, CONSOLE_HEIGHT + INPUT_HEIGHT, "Page " + currentPage + "/" + System.Math.Max(1, ((lineHistory.Count + (LINES_PER_PAGE - 1)) / LINES_PER_PAGE)), InputColor);

			// Draw blinking cursor
			if (nowTickCount % 1000 < 500)
			{
				var lengthBetweenInputStartAndCursor = GetTextLength(input.Substring(0, cursorPos)) - GetMarginLength();
				DrawRect(26 + (lengthBetweenInputStartAndCursor * CONSOLE_WIDTH), CONSOLE_HEIGHT + 2, 2, INPUT_HEIGHT - 4, Color.White);
			}

			// Draw console history text
			var historyOffset = lineHistory.Count - (LINES_PER_PAGE * currentPage);
			var historyLength = historyOffset + LINES_PER_PAGE;
			for (var i = System.Math.Max(0, historyOffset); i < historyLength; ++i)
			{
				DrawText(2, (float)((i - historyOffset) * 14), lineHistory[i], OutputColor);
			}
		}
		/// <summary>
		/// Keyboard handling logic of the console.
		/// </summary>
		/// <param name="keys">The key that was originated this event and its modifiers.</param>
		/// <param name="status"><see langword="true" /> on a key down, <see langword="false" /> on a key up event.</param>
		internal void DoKeyEvent(Keys keys, bool status)
		{
			if (!status || !IsOpen)
				return; // Only interested in key down events and do not need to handle events when the console is not open

			var e = new KeyEventArgs(keys);

			if (e.KeyCode == Keys.PageUp)
			{
				PageUp();
				return;
			}
			if (e.KeyCode == Keys.PageDown)
			{
				PageDown();
				return;
			}

			switch (e.KeyCode)
			{
				case Keys.Back:
					if (e.Alt)
						BackwardKillWord();
					else
						BackwardDeleteChar();
					break;
				case Keys.Delete:
					ForwardDeleteChar();
					break;
				case Keys.Left:
					if (e.Control)
						BackwardWord();
					else
						MoveCursorLeft();
					break;
				case Keys.Right:
					if (e.Control)
						ForwardWord();
					else
						MoveCursorRight();
					break;
				case Keys.Insert:
					if (e.Shift)
						AddClipboardContent();
					break;
				case Keys.Home:
					MoveCursorToBegOfLine();
					break;
				case Keys.End:
					MoveCursorToEndOfLine();
					break;
				case Keys.Up:
					GoUpCommandList();
					break;
				case Keys.Down:
					GoDownCommandList();
					break;
				case Keys.Enter:
					CompileExpression();
					break;
				case Keys.Escape:
					IsOpen = false;
					break;
				case Keys.B:
					if (e.Control)
						MoveCursorLeft();
					else if (e.Alt)
						BackwardWord();
					else
						goto default;
					break;
				case Keys.D:
					if (e.Alt)
						KillWord();
					else if (e.Control)
						ForwardDeleteChar();
					else
						goto default;
					break;
				case Keys.F:
					if (e.Control)
						MoveCursorRight();
					else if (e.Alt)
						ForwardWord();
					else
						goto default;
					break;
				case Keys.H:
					if (e.Control)
						BackwardDeleteChar();
					else
						goto default;
					break;
				case Keys.A:
					if (e.Control)
						MoveCursorToBegOfLine();
					else
						goto default;
					break;
				case Keys.E:
					if (e.Control)
						MoveCursorToEndOfLine();
					else
						goto default;
					break;
				case Keys.P:
					if (e.Control)
						GoUpCommandList();
					else
						goto default;
					break;
				case Keys.K:
					if (e.Control)
						BackwardKillLine();
					else
						goto default;
					break;
				case Keys.M:
					if (e.Control)
						CompileExpression();
					else
						goto default;
					break;
				case Keys.N:
					if (e.Control)
						GoDownCommandList();
					else
						goto default;
					break;
				case Keys.L:
					if (e.Control)
						Clear();
					else
						goto default;
					break;
				case Keys.T:
					if (e.Alt)
						TransposeTwoWords();
					else if (e.Control)
						TransposeTwoChars();
					else
						goto default;
					break;
				case Keys.U:
					if (e.Control)
						KillLine();
					else
						goto default;
					break;
				case Keys.V:
					if (e.Control)
						AddClipboardContent();
					else
						goto default;
					break;
				case Keys.W:
					if (e.Control)
						UnixWordRubout();
					else
						goto default;
					break;
				default:
					var buf = new StringBuilder(256);
					var keyboardState = new byte[256];
					keyboardState[(int)Keys.Menu] = e.Alt ? (byte)0xff : (byte)0;
					keyboardState[(int)Keys.ShiftKey] = e.Shift ? (byte)0xff : (byte)0;
					keyboardState[(int)Keys.ControlKey] = e.Control ? (byte)0xff : (byte)0;

					// Translate key event to character for text input
					ToUnicode((uint)e.KeyCode, 0, keyboardState, buf, 256, 0);
					AddToInput(buf.ToString());
					break;
			}
		}

		void PageUp()
		{
			if (currentPage < ((lineHistory.Count + LINES_PER_PAGE - 1) / LINES_PER_PAGE))
				currentPage++;
		}
		void PageDown()
		{
			if (currentPage > 1)
				currentPage--;
		}
		void GoUpCommandList()
		{
			if (commandHistory.Count == 0 || commandPos >= commandHistory.Count - 1)
				return;

			commandPos++;
			input = commandHistory[commandHistory.Count - commandPos - 1];
			// Reset cursor position to end of input text
			cursorPos = input.Length;
		}
		void GoDownCommandList()
		{
			if (commandHistory.Count == 0 || commandPos <= 0)
				return;

			commandPos--;
			input = commandHistory[commandHistory.Count - commandPos - 1];
			cursorPos = input.Length;
		}

		/// <summary>
		/// Moves to the end of the next word, just like emacs and GNU readline (does not move to the beginning of the next word like zsh does for forward-word).
		/// Words are composed of letters and digits.
		/// </summary>
		void ForwardWord()
		{
			if (cursorPos >= input.Length)
			{
				return;
			}

			// Note: Char.IsLetterOrDigit returns true for most characters where iswalnum returns true in Windows (exactly same result in the ASCII range), but does not apply for all of them
			// bash (GNU readline) and zsh use iswalnum (zsh uses iswalnum only if tested char is a non-ASCII one) to detect if characters can be used as words for your information
			if (!char.IsLetterOrDigit(input[cursorPos]))
			{
				cursorPos++;
				for (; cursorPos < input.Length; cursorPos++)
				{
					if (char.IsLetterOrDigit(input[cursorPos]))
					{
						break;
					}
				}
			}

			for (; cursorPos < input.Length; cursorPos++)
			{
				if (!char.IsLetterOrDigit(input[cursorPos]))
				{
					break;
				}
			}
		}
		/// <summary>
		/// Moves back to the start of the current or previous word.
		/// Words are composed of letters and digits.
		/// </summary>
		void BackwardWord()
		{
			if (cursorPos == 0)
			{
				return;
			}

			var prevChar = input[cursorPos - 1];
			if (!char.IsLetterOrDigit(prevChar))
			{
				cursorPos--;
				for (; cursorPos > 0; cursorPos--)
				{
					prevChar = input[cursorPos - 1];
					if (char.IsLetterOrDigit(prevChar))
					{
						break;
					}
				}
			}

			for (; cursorPos > 0; cursorPos--)
			{
				prevChar = input[cursorPos - 1];
				if (!char.IsLetterOrDigit(prevChar))
				{
					break;
				}
			}
		}
		/// <summary>
		/// Deletes the character behind the cursor.
		/// </summary>
		void BackwardDeleteChar()
		{
			if (input.Length <= 0 || cursorPos <= 0) return;
			input = input.Remove(cursorPos - 1, 1);
			cursorPos--;
		}
		/// <summary>
		/// Deletes the character at point.
		/// </summary>
		void ForwardDeleteChar()
		{
			if (input.Length <= 0 || cursorPos >= input.Length) return;
			input = input.Remove(cursorPos, 1);
		}

		/// <summary>
		/// Kills the text from the cursor to the end of the line.
		/// </summary>
		void KillLine()
		{
			if (input.Length <= 0 || cursorPos <= 0) return;
			KillText(ref input, 0, cursorPos);
			cursorPos = 0;
		}
		/// <summary>
		/// Kills backward from the cursor to the beginning of the current line.
		/// </summary>
		void BackwardKillLine()
		{
			if (input.Length <= 0 || cursorPos >= input.Length) return;
			KillText(ref input, cursorPos, input.Length - cursorPos);
		}
		/// <summary>
		/// Kills from point to the end of the current word, or if between words, to the end of the next word.
		/// Word boundaries are the same as <see cref="ForwardWord"/>.
		/// </summary>
		void KillWord()
		{
			var origCursorPos = cursorPos;
			ForwardWord();

			if (cursorPos == origCursorPos) return;
			KillText(ref input, origCursorPos, cursorPos - origCursorPos);
			cursorPos = origCursorPos;
		}
		/// <summary>
		/// Kill the word behind the cursor.
		/// Word boundaries are the same as <see cref="BackwardWord"/>.
		/// </summary>
		void BackwardKillWord()
		{
			var origCursorPos = cursorPos;
			BackwardWord();

			if (cursorPos == origCursorPos) return;
			KillText(ref input, cursorPos, origCursorPos - cursorPos);
		}
		/// <summary>
		/// Kills the word behind the cursor, using white space as a word boundary.
		/// </summary>
		void UnixWordRubout()
		{
			if (cursorPos == 0)
			{
				return;
			}

			var origCursorPos = cursorPos;

			while (cursorPos > 0 && IsRegularWhiteSpaceOrTab(input[cursorPos - 1]))
			{
				cursorPos--;
			}


			while (cursorPos > 0 && !IsRegularWhiteSpaceOrTab(input[cursorPos - 1]))
			{
				cursorPos--;
			}


			KillText(ref input, cursorPos, origCursorPos - cursorPos);

			// yields exactly the same result as a internal "whitespace" function in bash
			static bool IsRegularWhiteSpaceOrTab(char ch) => ch == ' ' || ch == '\t';
		}

		/// <summary>
		/// Drags the character before the cursor forward over the character at the cursor, moving the cursor forward as well.
		/// If the insertion point is at the end of the line, then this transposes the last two characters of the line.
		/// </summary>
		void TransposeTwoChars()
		{
			var inputLength = input.Length;
			if (inputLength < 2)
			{
				return;
			}

			if (cursorPos == 0)
			{
				SwapTwoCharacters(input, 0);
				cursorPos = 2;
			}
			else if (cursorPos < inputLength)
			{
				SwapTwoCharacters(input, cursorPos - 1);
				cursorPos += 1;
			}
			else
			{
				SwapTwoCharacters(input, cursorPos - 2);
			}

			void SwapTwoCharacters(string str, int index)
			{
				unsafe
				{
					fixed (char* stringPtr = str)
					{
						var tmp = stringPtr[index];
						stringPtr[index] = stringPtr[index + 1];
						stringPtr[index + 1] = tmp;
					}
				}
			}
		}
		/// <summary>
		/// Drags the word before point past the word after point, moving point past that word as well.
		/// If the insertion point is at the end of the line, this transposes the last two words on the line.
		/// </summary>
		void TransposeTwoWords()
		{
			if (input.Length < 3)
			{
				return;
			}

			var origCursorPos = cursorPos;

			ForwardWord();
			var word2End = cursorPos;
			BackwardWord();
			var word2Beg = cursorPos;
			BackwardWord();
			var word1Beg = cursorPos;
			ForwardWord();
			var word1End = cursorPos;

			if ((word1Beg == word2Beg) || (word2Beg < word1End))
			{
				cursorPos = origCursorPos;
				return;
			}

			var word1 = input.Substring(word1Beg, word1End - word1Beg);
			var word2 = input.Substring(word2Beg, word2End - word2Beg);

			var stringBuilder = new StringBuilder(input.Length + Math.Max((word1.Length - word2.Length), 0)); // Prevent reallocation of internal array
			stringBuilder.Append(input);

			stringBuilder.Remove(word2Beg, word2.Length);
			stringBuilder.Insert(word2Beg, word1);

			stringBuilder.Remove(word1Beg, word1.Length);
			stringBuilder.Insert(word1Beg, word2);

			input = stringBuilder.ToString();
			cursorPos = word2End;
		}

		void KillText(ref string str, int startIndex, int length)
		{
			Clipboard.SetText(str.Substring(startIndex, length));
			str = str.Remove(startIndex, length);
		}

		void MoveCursorLeft()
		{
			if (cursorPos > 0)
				cursorPos--;
		}
		void MoveCursorRight()
		{
			if (cursorPos < input.Length)
				cursorPos++;
		}
		void MoveCursorToBegOfLine()
		{
			cursorPos = 0;
		}
		void MoveCursorToEndOfLine()
		{
			cursorPos = input.Length;
		}

		void CompileExpression()
		{
			if (string.IsNullOrEmpty(input) || compilerTask != null)
				return;

			commandPos = -1;
			if (commandHistory.LastOrDefault() != input)
				commandHistory.Add(input);

			compilerTask = Task.Factory.StartNew(() =>
			{
				var compiler = new Microsoft.CSharp.CSharpCodeProvider();
				var compilerOptions = new System.CodeDom.Compiler.CompilerParameters();
				compilerOptions.GenerateInMemory = true;
				compilerOptions.IncludeDebugInformation = true;
				compilerOptions.ReferencedAssemblies.Add("System.dll");
				compilerOptions.ReferencedAssemblies.Add("System.Core.dll");
				compilerOptions.ReferencedAssemblies.Add("System.Drawing.dll");
				compilerOptions.ReferencedAssemblies.Add("System.Windows.Forms.dll");
				// Reference the newest scripting API
				compilerOptions.ReferencedAssemblies.Add("ScriptHookVDotNet3.dll");
				compilerOptions.ReferencedAssemblies.Add(typeof(ScriptDomain).Assembly.Location);

				foreach (var script in ScriptDomain.CurrentDomain.RunningScripts.Where(x => x.IsRunning))
					if (System.IO.File.Exists(script.Filename) && System.IO.Path.GetExtension(script.Filename) == ".dll")
						compilerOptions.ReferencedAssemblies.Add(script.Filename);

				const string template =
					"using System; using System.Linq; using System.Drawing; using System.Windows.Forms; using GTA; using GTA.Math; using GTA.Native; " +
					// Define some shortcut variables to simplify commands
					"public class ConsoleInput : ScriptHookVDotNet {{ public static object Execute() {{ var P = Game.Player.Character; var V = P.CurrentVehicle; {0}; return null; }} }}";

				var compilerResult = compiler.CompileAssemblyFromSource(compilerOptions, string.Format(template, input));

				if (!compilerResult.Errors.HasErrors)
				{
					return compilerResult.CompiledAssembly.GetType("ConsoleInput").GetMethod("Execute");
				}

				PrintError($"Couldn't compile input expression: {input}");

				var errors = new StringBuilder();

				for (var i = 0; i < compilerResult.Errors.Count; ++i)
				{
					errors.Append("   at line ");
					errors.Append(compilerResult.Errors[i].Line);
					errors.Append(": ");
					errors.Append(compilerResult.Errors[i].ErrorText);

					if (i < compilerResult.Errors.Count - 1)
						errors.AppendLine();
				}

				PrintError(errors.ToString());
				return null;
			});
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

		static unsafe void DrawRect(float x, float y, int width, int height, Color color)
		{
			var w = (float)(width) / BASE_WIDTH;
			var h = (float)(height) / BASE_HEIGHT;

			NativeFunc.InvokeInternal(0x3A618A217E5154F0ul /* DRAW_RECT */,
				(x / BASE_WIDTH) + w * 0.5f,
				(y / BASE_HEIGHT) + h * 0.5f,
				w, h,
				color.R, color.G, color.B, color.A);
		}
		static unsafe void DrawText(float x, float y, string text, Color color)
		{
			NativeFunc.InvokeInternal(0x66E0276CC5F6B9DA /* SET_TEXT_FONT */, 0); // Chalet London :>
			NativeFunc.InvokeInternal(0x07C837F9A01C34C9 /* SET_TEXT_SCALE */, 0.35f, 0.35f);
			NativeFunc.InvokeInternal(0xBE6B23FFA53FB442 /* SET_TEXT_COLOUR */, color.R, color.G, color.B, color.A);
			NativeFunc.InvokeInternal(0x25FBB336DF1804CB /* BEGIN_TEXT_COMMAND_DISPLAY_TEXT */, NativeMemory.CellEmailBcon);
			NativeFunc.PushLongString(text, 99);
			NativeFunc.InvokeInternal(0xCD015E5BB0D96A57 /* END_TEXT_COMMAND_DISPLAY_TEXT */, (x / BASE_WIDTH), (y / BASE_HEIGHT));
		}

		static unsafe void DisableControlsThisFrame()
		{
			NativeFunc.InvokeInternal(0x5F4B6931816E599B /* DISABLE_ALL_CONTROL_ACTIONS */, 0);

			// LookLeftRight .. LookRightOnly
			for (ulong i = 1; i <= 6; i++)
				NativeFunc.InvokeInternal(0x351220255D64C155 /* ENABLE_CONTROL_ACTION */, 0, i, 0);
		}

		static unsafe float GetTextLength(string text)
		{
			NativeFunc.InvokeInternal(0x66E0276CC5F6B9DA /* SET_TEXT_FONT */, 0);
			NativeFunc.InvokeInternal(0x07C837F9A01C34C9 /* SET_TEXT_SCALE */, 0.35f, 0.35f);
			NativeFunc.InvokeInternal(0x54CE8AC98E120CAB /* BEGIN_TEXT_COMMAND_GET_SCREEN_WIDTH_OF_DISPLAY_TEXT */, NativeMemory.CellEmailBcon);
			NativeFunc.PushLongString(text, 98); // 99 byte string chunks don't process properly in END_TEXT_COMMAND_GET_SCREEN_WIDTH_OF_DISPLAY_TEXT
			return *(float*)NativeFunc.InvokeInternal(0x85F061DA64ED2F67 /* END_TEXT_COMMAND_GET_SCREEN_WIDTH_OF_DISPLAY_TEXT */, true);
		}

		static float GetMarginLength()
		{
			var len1 = GetTextLength("A");
			var len2 = GetTextLength("AA");
			return len1 - (len2 - len1); // [Margin][A] - [A] = [Margin]
		}
	}

	public class ConsoleCommand : Attribute
	{
		public ConsoleCommand() : this(string.Empty)
		{
		}
		public ConsoleCommand(string help)
		{
			Help = help;
		}

		public string Help { get; }

		internal string Name => MethodInfo.Name;
		internal string Namespace => MethodInfo.DeclaringType.FullName;
		internal MethodInfo MethodInfo { get; set; }
	}
}
