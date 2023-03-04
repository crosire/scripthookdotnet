// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "hosting.h"
#include "main.h"
#include "exports.h"
bool GameReloaded;

void SetTls(LPVOID tls)
{
	__writegsqword(0x58, (DWORD64)tls);
}
LPVOID GetTls()
{
	return (LPVOID)__readgsqword(0x58);
}

void Startup() {
	// Find asi path
	assert(GetModuleFileName(AsiModule, AsiFilePath, sizeof(AsiFilePath) / sizeof(TCHAR)) != ERROR_INSUFFICIENT_BUFFER);

	// Set up well-known properties
	SetEnvironmentVariable(L"SHVDN_ASI_PATH", AsiFilePath);
	SetPtr("SHVDN.ASI.GetTlsFuncAddr", GetTls);
	SetPtr("SHVDN.ASI.SetTlsFuncAddr", SetTls);
	SetPtr("SHVDN.ASI.ModuleHandle", AsiModule);
	SetPtr("SHVDN.ASI.PtrGameReloaded", &GameReloaded);
	CLR_Startup();
}
void OnKeyboard(DWORD key, WORD repeats, BYTE scanCode, BOOL isExtended, BOOL isWithAlt, BOOL wasDownBefore, BOOL isUpNow) {
	CLR_DoKeyboard(
		key,
		!isUpNow,
		(GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0,
		(GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0,
		isWithAlt != FALSE);
}
LPVOID sGameFiber;
void ScriptMain() {

	// ScriptHookV already turned the current thread into a fiber, so can safely retrieve it
	sGameFiber = GetCurrentFiber();

	while (true)
	{
		GameReloaded = false;
		CLR_DoInit();

		while (!GameReloaded)
		{
			const PVOID currentFiber = GetCurrentFiber();
			if (currentFiber != sGameFiber)
			{
				// SHV switches the fiber when the game loads a save, so trigger a reload here
				sGameFiber = currentFiber;
				GameReloaded = true;
				break;
			}
			CLR_DoTick();
			scriptWait(0);
		}
	}
}
BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		AsiModule = hModule;
		DisableThreadLibraryCalls(hModule);
		scriptRegister(hModule, ScriptMain);
		keyboardHandlerRegister(OnKeyboard);
		Startup();
		break;
	case DLL_THREAD_ATTACH:
		break;
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		// This does not allow proper cleanup of ScriptDomain, but we'll unload leftover domains during next init
		CLR_Shutdown();
		keyboardHandlerUnregister(OnKeyboard);
		scriptUnregister(AsiModule);
		break;
	}
	return TRUE;
}

