//
// Copyright (C) 2015 crosire & contributors
// License: https://github.com/crosire/scripthookvdotnet#license
//

using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using SHVDN;
using static SHVDN.NativeMemory;

namespace GTA.NaturalMotion
{
	/// <summary>
	/// A base class for manually building a <see cref="GTA.NaturalMotion.Message"/>.
	/// </summary>
	public class Message
	{
		#region Fields
		private string _message;
		private Dictionary<string, (int value, Type type)> _boolIntFloatArguments;
		private Dictionary<string, object> _stringVector3ArrayArguments;
		private static readonly Dictionary<string, (int value, Type type)> _stopArgument = new Dictionary<string, (int value, Type type)>() { { "start", (0, typeof(bool)) } };
		#endregion

		/// <summary>
		/// Creates a class to manually build <see cref="Message"/>s that can be sent to any <see cref="Ped"/>.
		/// </summary>
		/// <param name="message">The name of the natural motion message.</param>
		public Message(string message)
		{
			_message = message;
			_boolIntFloatArguments = new Dictionary<string, (int value, Type type)>();
			_stringVector3ArrayArguments = new Dictionary<string, object>();
		}
		private Message(string message, Dictionary<string, (int value, Type type)> boolIntFloatArgs, Dictionary<string, object> stringVector3ArrayArgs)
		{
			_message = message;
			_boolIntFloatArguments = boolIntFloatArgs;
			_stringVector3ArrayArguments = stringVector3ArrayArgs;
		}

		/// <summary>
		/// Stops this NaturalMotion behavior on the given <see cref="Ped"/>.
		/// </summary>
		/// <param name="target">The <see cref="Ped"/> to send the Abort <see cref="Message"/> to.</param>
		public void Abort(Ped target)
		{
			if (target == null || !target.Exists())
				return;

			var message = new SHVDN.NativeMemory.NmMessageParameterCollection(_message, _stopArgument, null);
			SHVDN.NativeMemory.SendNmMessage(target.Handle, message);
		}

		/// <summary>
		/// Starts this NaturalMotion behavior on the <see cref="Ped"/> that will loop until manually aborted.
		/// Starts a <c>CTaskNMControl</c> task if the <see cref="Ped"/> has no such task.
		/// </summary>
		/// <param name="target">The <see cref="Ped"/> to send the <see cref="Message"/> to.</param>
		public void SendTo(Ped target)
		{
			if (target == null || !target.Exists())
				return;

			if (!target.IsRagdoll)
			{
				if (!target.CanRagdoll)
				{
					target.CanRagdoll = true;
				}
			}

			if (!SHVDN.NativeMemory.IsTaskNMScriptControlOrEventSwitch2NMActive(target.MemoryAddress))
			{
				// Does not call when a CTaskNMControl task is active or the CEvent (which usually causes some task) related to CTaskNMControl occured for calling SET_PED_TO_RAGDOLL just like in legacy scripts.
				// Otherwise, the ragdoll duration will be overridden.
				Function.Call(Hash.SET_PED_TO_RAGDOLL, target.Handle, 10000, -1, 1, 1, 1, 0);
			}

			SendMessagesWithStartParamEnabledToInternal(target);
		}
		/// <summary>
		///	Starts this NaturalMotion behavior on the <see cref="Ped"/> for a specified duration.
		///	Always starts a new ragdoll task, making impossible to stack multiple Natural Motion behaviors on this <see cref="Ped"/>.
		/// </summary>
		/// <param name="target">The <see cref="Ped"/> to send the <see cref="Message"/> to.</param>
		/// <param name="duration">How long to apply the behavior for (-1 for looped).</param>
		public void SendTo(Ped target, int duration)
		{
			if (target == null || !target.Exists())
				return;

			if (!target.CanRagdoll)
			{
				target.CanRagdoll = true;
			}

			// Always call to specify the new duration
			Function.Call(Hash.SET_PED_TO_RAGDOLL, target.Handle, 10000, duration, 1, 1, 1, 0);
			SendTo(target);
		}

		/// <summary>
		/// Sends message this NaturalMotion behavior on the <see cref="Ped"/>, but don't start a new ragdoll task so the <see cref="Ped"/> can have multiple NM behaviors.
		/// You'll need to start a ragdoll task with <see cref="Ped.Ragdoll(int, RagdollType)"/> before the <see cref="Ped"/> can recieve this <see cref="Message"/>.
		/// </summary>
		/// <remarks>
		/// Although <see cref="SendTo(Ped)"/> wouldn't create a new ragdoll task if the <see cref="Ped"/> runs a script ragdoll tasks in most cases,
		/// this method guarantees it sends the message if the <see cref="Ped"/> can receive NM messages but doesn't create a new ragdoll task.
		/// </remarks>
		/// <param name="target">The <see cref="Ped"/> to send the <see cref="Message"/> to.</param>
		/// <param name="startBehavior">Specifies whether this message should start at the beginning.</param>
		/// <param name="ifNMScriptControlRunning">
		/// <para>Specifies whether the messages should be sent to the only if they <see cref="Ped"/> has a <c>CTaskNMScriptControl</c> running, which can be started with <see cref="Ped.Ragdoll(int, RagdollType)"/>.</para>
		/// <para>
		/// If you set this parameter to <see langword="false"/> and you need to set the <see cref="Ped"/> from a ragdoll state to an animated state after a certain timeout (in game time),
		/// you should call <see cref="Ped.CancelRagdoll()"/> on them after a certain timeout if they are alive and <see cref="Ped.IsRagdoll"/> returns <see langword="true"/>.
		/// </para>
		/// </param>
		public void SendToNoNewRagdollTask(Ped target, bool startBehavior = true, bool ifNMScriptControlRunning = true)
		{
			if (startBehavior)
			{
				SendMessagesWithStartParamEnabledToInternal(target, ifNMScriptControlRunning);
			}
			else
			{
				SendMessagesWithoutExplicitStartParamToInternal(target, ifNMScriptControlRunning);
			}
		}

		public void SendMessagesWithStartParamEnabledToInternal(Ped target, bool ifNMScriptControlRunning = true)
		{
			// Use a cloned dictionary so this message instance can keep the original parameters
			// Shallow copying should be enough
			var newBoolIntFloatArguments = GetClonedDictWithStartParamEnabled();
			var constructedMessageAndParameters = new NmMessageParameterCollection(_message, newBoolIntFloatArguments, _stringVector3ArrayArguments);
			SHVDN.NativeMemory.SendNmMessage(target.Handle, constructedMessageAndParameters, ifNMScriptControlRunning);
		}
		public void SendMessagesWithoutExplicitStartParamToInternal(Ped target, bool ifNMScriptControlRunning = true)
		{
			// Use a cloned dictionary so this message instance can keep the original parameters
			// Shallow copying should be enough
			var newBoolIntFloatArguments = GetClonedDictWithoutExplicitStartParam();
			var constructedMessageAndParameters = new NmMessageParameterCollection(_message, newBoolIntFloatArguments, _stringVector3ArrayArguments);
			SHVDN.NativeMemory.SendNmMessage(target.Handle, constructedMessageAndParameters, ifNMScriptControlRunning);
		}

		private Dictionary<string, (int value, Type type)> GetClonedDictWithStartParamEnabled()
		{
			Dictionary<string, (int value, Type type)> clonedDict;
			if (_boolIntFloatArguments == null)
			{
				clonedDict = new Dictionary<string, (int value, Type type)>();
			}
			else
			{
				clonedDict = new Dictionary<string, (int value, Type type)>(_boolIntFloatArguments);
			}
			clonedDict["start"] = (1, typeof(bool));

			return clonedDict;
		}
		private Dictionary<string, (int value, Type type)> GetClonedDictWithoutExplicitStartParam()
		{
			Dictionary<string, (int value, Type type)> clonedDict;
			if (_boolIntFloatArguments == null)
			{
				clonedDict = new Dictionary<string, (int value, Type type)>();
			}
			else
			{
				clonedDict = new Dictionary<string, (int value, Type type)>(_boolIntFloatArguments);
			}
			clonedDict.Remove("start");

			return clonedDict;
		}

		/// <summary>
		/// Sets a <see cref="Message"/> argument to a <see cref="bool"/> value.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <param name="value">The value to set the argument to.</param>
		public void SetArgument(string argName, bool value)
		{
			CreateBoolIntFloatArgDictIfNotCreated();

			int valueConverted = value ? 1 : 0;
			_boolIntFloatArguments[argName] = (valueConverted, typeof(bool));
		}
		/// <summary>
		/// Sets a <see cref="Message"/> argument to a <see cref="int"/> value.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <param name="value">The value to set the argument to.</param>
		public void SetArgument(string argName, int value)
		{
			CreateBoolIntFloatArgDictIfNotCreated();

			_boolIntFloatArguments[argName] = (value, typeof(int));
		}
		/// <summary>
		/// Sets a <see cref="Message"/> argument to a <see cref="float"/> value.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <param name="value">The value to set the argument to.</param>
		public void SetArgument(string argName, float value)
		{
			CreateBoolIntFloatArgDictIfNotCreated();

			unsafe
			{
				int valueConverted = *(int*)&value;
				_boolIntFloatArguments[argName] = (valueConverted, typeof(float));
			}
		}
		/// <summary>
		/// Sets a <see cref="Message"/> argument to a <see cref="string"/> value.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <param name="value">The value to set the argument to.</param>
		public void SetArgument(string argName, string value)
		{
			CreateStringVector3ArrayArgDictIfNotCreated();

			_stringVector3ArrayArguments[argName] = value;
		}
		/// <summary>
		/// Sets a <see cref="Message"/> argument to a <see cref="Vector3"/> value.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <param name="value">The value to set the argument to.</param>
		public void SetArgument(string argName, Vector3 value)
		{
			CreateStringVector3ArrayArgDictIfNotCreated();

			_stringVector3ArrayArguments[argName] = value.ToArray();
		}

		/// <summary>
		/// Resets all arguments to their default values.
		/// </summary>
		public void ResetArguments()
		{
			_boolIntFloatArguments?.Clear();
			_stringVector3ArrayArguments?.Clear();
		}

		public void CreateBoolIntFloatArgDictIfNotCreated()
		{
			if (_boolIntFloatArguments == null)
			{
				_boolIntFloatArguments = new Dictionary<string, (int value, Type type)>();
			}
		}

		public void CreateStringVector3ArrayArgDictIfNotCreated()
		{
			if (_stringVector3ArrayArguments == null)
			{
				_stringVector3ArrayArguments = new Dictionary<string, object>();
			}
		}

		/// <summary>
		/// Returns the internal message name.
		/// </summary>
		public override string ToString()
		{
			return _message;
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance by performing a shallow copy.
		/// </summary>
		public Message Clone()
		{
			var newBoolIntFloatArgumentsShallowCopied = new Dictionary<string, (int, Type)>(_boolIntFloatArguments);
			var newStringVector3ArrayArgumentsShallowCopied = new Dictionary<string, object>(_stringVector3ArrayArguments);
			return new Message(_message, newBoolIntFloatArgumentsShallowCopied, newStringVector3ArrayArgumentsShallowCopied);
		}
	}

	/// <summary>
	/// A helper class for building a <seealso cref="GTA.NaturalMotion.Message" /> and sending it to a given <see cref="Ped"/>.
	/// </summary>
	public abstract class CustomHelper
	{
		#region Fields
		private readonly Ped _ped;
		private readonly Message _message;
		#endregion

		/// <summary>
		/// Creates a helper class for building Natural Motion messages to send to a given <see cref="Ped"/>.
		/// </summary>
		/// <param name="target">The <see cref="Ped"/> that the message will be applied to.</param>
		/// <param name="message">The name of the natural motion message.</param>
		protected CustomHelper(Ped target, string message)
		{
			_ped = target;
			_message = new Message(message);
		}

		/// <summary>
		/// Starts this Natural Motion behavior on the <see cref="Ped"/> that will loop until manually aborted.
		/// </summary>
		public void Start()
		{
			_message.SendTo(_ped);
		}
		/// <summary>
		/// Starts this Natural Motion behavior on the <see cref="Ped"/> for a specified duration.
		/// </summary>
		/// <param name="duration">How long to apply the behavior for (-1 for looped).</param>
		public void Start(int duration)
		{
			_message.SendTo(_ped, duration);
		}
		/// <summary>
		/// Starts this Natural Motion behavior on the <see cref="Ped"/> for a specified duration.
		/// </summary>
		/// <param name="ifNMScriptControlRunning">
		/// <para>Specifies whether the messages should be sent to the only if they <see cref="Ped"/> has a <c>CTaskNMScriptControl</c> running, which can be started with <see cref="Ped.Ragdoll(int, RagdollType)"/>.</para>
		/// <para>
		/// If you set this parameter to <see langword="false"/> and you need to set the <see cref="Ped"/> from a ragdoll state to an animated state after a certain timeout (in game time),
		/// you should call <see cref="Ped.CancelRagdoll()"/> on them after a certain timeout if they are alive and <see cref="Ped.IsRagdoll"/> returns <see langword="true"/>.
		/// </para>
		/// </param>
		public void Update(bool ifNMScriptControlRunning = true)
		{
			_message.SendToNoNewRagdollTask(_ped, false, ifNMScriptControlRunning);
		}
		/// <summary>
		/// Stops this Natural Motion behavior on the <see cref="Ped"/>.
		/// </summary>
		public void Stop()
		{
			_message.Abort(_ped);
		}

		/// <summary>
		/// Sets a <see cref="Message"/> argument to a <see cref="bool"/> value.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <param name="value">The value to set the argument to.</param>
		public void SetArgument(string argName, bool value)
		{
			_message.SetArgument(argName, value);
		}
		/// <summary>
		/// Sets a <see cref="Message"/> argument to a <see cref="int"/> value.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <param name="value">The value to set the argument to.</param>
		public void SetArgument(string argName, int value)
		{
			_message.SetArgument(argName, value);
		}
		/// <summary>
		/// Sets a <see cref="Message"/> argument to a <see cref="float"/> value.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <param name="value">The value to set the argument to.</param>
		public void SetArgument(string argName, float value)
		{
			_message.SetArgument(argName, value);
		}
		/// <summary>
		/// Sets a <see cref="Message"/> argument to a <see cref="string"/> value.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <param name="value">The value to set the argument to.</param>
		public void SetArgument(string argName, string value)
		{
			_message.SetArgument(argName, value);
		}
		/// <summary>
		/// Sets a <see cref="Message"/> argument to a <see cref="Vector3"/> value.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <param name="value">The value to set the argument to.</param>
		public void SetArgument(string argName, Vector3 value)
		{
			_message.SetArgument(argName, value);
		}

		/// <summary>
		/// Resets all arguments to their default values.
		/// </summary>
		public void ResetArguments()
		{
			_message.ResetArguments();
		}

		/// <summary>
		/// Returns the internal message name.
		/// </summary>
		public override string ToString()
		{
			return _message.ToString();
		}
	}
}
