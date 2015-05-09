#pragma once

#include "Entity.hpp"

namespace GTA
{
	ref class Tasks;
	ref class Vehicle;

	public enum class Gender
	{
		Male,
		Female
	};

	public ref class Ped sealed : public Entity
	{
	public:
		Ped(int id);

		static property Ped ^Any
		{
			Ped ^get();
		}

		property int Accuracy
		{
			int get();
			void set(int value);
		}
		property Tasks ^Task
		{
			Tasks ^get();
		}
		property GTA::Gender Gender
		{
			GTA::Gender get();
		}
		property int Armor
		{
			int get();
			void set(int value);
		}
		property int Money
		{
			int get();
			void set(int value);
		}
		property bool IsPlayer
		{
			bool get();
		}
		property bool IsHuman
		{
			bool get();
		}
		property bool IsIdle
		{
			bool get();
		}
		property bool IsProne
		{
			bool get();
		}
		property bool IsDucking
		{
			bool get();
			void set(bool value);
		}
		property bool IsGettingUp
		{
			bool get();
		}
		property bool IsGettingIntoAVehicle
		{
			bool get();
		}
		property bool IsRagdoll
		{
			bool get();
		}
		property bool IsInjured
		{
			bool get();
		}
		property bool IsShooting
		{
			bool get();
		}
		property bool IsInCombat
		{
			bool get();
		}
		property bool IsInMeleeCombat
		{
			bool get();
		}
		property bool IsSwimming
		{
			bool get();
		}
		property bool IsSwimmingUnderWater
		{
			bool get();
		}
		property Vehicle ^CurrentVehicle
		{
			Vehicle ^get();
		}
		property bool IsEnemy
		{
			void set(bool value);
		}
		property bool IsPriorityTargetForEnemies
		{
			void set(bool value);
		}
		property bool AlwaysDiesOnLowHealth
		{
			void set(bool value);
		}
		property bool BlockPermanentEvents
		{
			void set(bool value);
		}
		property bool CanRagdoll
		{
			void set(bool value);
		}
		property bool CanSwitchWeapons
		{
			void set(bool value);
		}
		property bool CanBeKnockedOffBike
		{
			void set(bool value);
		}
		property bool CanBeDraggedOutOfVehicle
		{
			void set(bool value);
		}
		property bool CanPlayGestures
		{
			void set(bool value);
		}

		bool IsInVehicle();
		bool IsInVehicle(Vehicle ^vehicle);
		bool IsSittingInVehicle();
		bool IsSittingInVehicle(Vehicle ^vehicle);

		float GetDistanceTo(Math::Vector3 coordinate);
		void Kill();

	private:
		Tasks ^mTasks;
	};
}