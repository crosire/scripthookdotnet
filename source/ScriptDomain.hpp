/**
 * Copyright (C) 2015 Crosire
 *
 * This software is  provided 'as-is', without any express  or implied  warranty. In no event will the
 * authors be held liable for any damages arising from the use of this software.
 * Permission  is granted  to anyone  to use  this software  for  any  purpose,  including  commercial
 * applications, and to alter it and redistribute it freely, subject to the following restrictions:
 *
 *   1. The origin of this software must not be misrepresented; you must not claim that you  wrote the
 *      original  software. If you use this  software  in a product, an  acknowledgment in the product
 *      documentation would be appreciated but is not required.
 *   2. Altered source versions must  be plainly  marked as such, and  must not be  misrepresented  as
 *      being the original software.
 *   3. This notice may not be removed or altered from any source distribution.
 */

#pragma once

#include "Script.hpp"

namespace GTA
{
	private interface class IScriptTask
	{
		void Run();
	};

	private ref class ScriptDomain sealed : public System::MarshalByRefObject
	{
	public:
		ScriptDomain();
		~ScriptDomain();

		static property Script ^ExecutingScript
		{
			Script ^get()
			{
				if (System::Object::ReferenceEquals(sCurrentDomain, nullptr))
				{
					return nullptr;
				}

				return sCurrentDomain->mExecutingScript;
			}
		}
		static property ScriptDomain ^CurrentDomain
		{
			inline ScriptDomain ^get()
			{
				return sCurrentDomain;
			}
		}

		static ScriptDomain ^Load(System::String ^path);
		static void Unload(ScriptDomain ^%domain);

		property System::String ^Name
		{
			inline System::String ^get()
			{
				return this->mAppDomain->FriendlyName;
			}
		}
		property System::AppDomain ^AppDomain
		{
			inline System::AppDomain ^get()
			{
				return this->mAppDomain;
			}
		}

		void Start();
		void Abort();
		static void AbortScript(Script ^script);
		void DoTick();
		void DoKeyboardMessage(System::Windows::Forms::Keys key, bool status, bool statusCtrl, bool statusShift, bool statusAlt);

		void ExecuteTask(IScriptTask ^task);
		System::IntPtr PinString(System::String ^string);
		inline bool IsKeyPressed(System::Windows::Forms::Keys key)
		{
			return this->mKeyboardState[static_cast<int>(key)];
		}
		System::Object ^InitializeLifetimeService() override;

	private:
		bool LoadScript(System::String ^filename);
		bool LoadAssembly(System::String ^filename);
		bool LoadAssembly(System::String ^filename, System::Reflection::Assembly ^assembly);
		Script ^InstantiateScript(System::Type ^scripttype);
		void CleanupStrings();

		static ScriptDomain ^sCurrentDomain;
		static String ^mScriptPath;
		System::AppDomain ^mAppDomain;
		int mExecutingThreadId;
		Script ^mExecutingScript;
		System::Collections::Generic::List<Script ^> ^mRunningScripts;
		System::Collections::Generic::Queue<IScriptTask ^> ^mTaskQueue;
		System::Collections::Generic::List<System::IntPtr> ^mPinnedStrings;
		System::Collections::Generic::Dictionary<String ^, System::Collections::Generic::List<Type^>^> ^mScriptTypes;
		System::Collections::Generic::Dictionary<String ^, System::Collections::Generic::List<String^>^> ^mScriptTypeFiles;
		array<bool> ^mKeyboardState;
	};
}