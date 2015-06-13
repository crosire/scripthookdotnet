#pragma once

#include "Player.hpp"
#include "MemoryAccess.hpp"
#include "Controls.hpp"

namespace GTA
{
	public enum class RadioStation
	{
		LosSantosRockRadio,
		NonStopPopFM,
		RadioLosSantos,
		ChannelX,
		WestCoastTalkRadio,
		RebelRadio,
		SoulwaxFM,
		EastLosFM,
		WestCoastClassics,
		TheBlueArk,
		WorldWideFM,
		FlyloFM,
		TheLowdown,
		TheLab,
		RadioMirrorPark,
		Space,
		VinewoodBoulevardRadio,
	};

	public ref class Game sealed abstract
	{
	private:
		static GTA::GameVersion gVersion; // maybe come up with a better name
	public:
		static property float FPS
		{
			float get();
		}
		static property GTA::GameVersion Version
		{
			GTA::GameVersion get();
		}
		static property int GameTime
		{
			int get();
		}
		static property bool IsPaused
		{
			bool get();
			void set(bool value);
		}
		static property float LastFrameTime
		{
			float get();
		}
		static property bool MissionFlag
		{
			bool get();
			void set(bool value);
		}
		static property bool Nightvision
		{
			bool get();
			void set(bool value);
		}
		static property GTA::Player ^Player
		{
			GTA::Player ^get();
		}
		static property int RadarZoom
		{
			void set(int value);
		}
		static property GTA::RadioStation RadioStation
		{
			GTA::RadioStation get();
			void set(GTA::RadioStation value);
		}
		static property float TimeScale
		{
			void set(float value);
		}
		static property float WantedMultiplier
		{
			void set(float value);
		}

		static bool IsKeyPressed(System::Windows::Forms::Keys key);
		static bool IsControlPressed(int index, Control control);
		static bool IsControlJustPressed(int index, Control control);
		static bool IsControlJustReleased(int index, Control control);

		static void Pause();
		static void Unpause();
		static void DoAutoSave();
		static void ShowSaveMenu();
		static void FadeScreenIn(int time);
		static void FadeScreenOut(int time);
		static System::String ^GetGXTEntry(System::String ^entry);

		static void PlaySound(System::String ^soundFile, System::String ^soundSet);
		static void PlayMusic(System::String ^musicFile);
		static void StopMusic(System::String ^musicFile);

		static System::String ^GetUserInput(int maxLength);
		static System::String ^GetUserInput(System::String ^startText, int maxLength);
	};
}