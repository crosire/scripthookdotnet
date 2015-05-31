#pragma once

#include "Entity.hpp"
#include "World.hpp"
#include "Vehicle.hpp"

namespace GTA
{
	ref class Tasks;
	ref class Vehicle;
	ref class WeaponCollection;

	public enum class Gender
	{
		Male,
		Female
	};

	public enum class DrivingStyle
	{
		Normal = 0xC00AB,
		IgnoreLights = 0x2C0025,
		SometimesOvertakeTraffic = 5,
		Rushed = 0x400C0025,
		AvoidTraffic = 0xC0024,
		AvoidTrafficExtremely = 6,
	};

	public ref class Ped sealed : public Entity
	{
	public:
		Ped(int handle);

		property int Accuracy
		{
			int get();
			void set(int value);
		}
		property Tasks ^Task
		{
			Tasks ^get();
		}
		property int TaskSequenceProgress
		{
			int get();
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
		property bool AlwaysKeepTask
		{
			void set(bool value);
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
			bool get();
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
		property bool IsWalking
		{
			bool get();
		}
		property bool IsRunning
		{
			bool get();
		}
		property bool IsSprinting
		{
			bool get();
		}
		property int RelationshipGroup
		{
			int get();
			void set(int group);
		}
		property float DrivingSpeed 
		{
			void set(float value);
		}
		property float MaxDrivingSpeed
		{
			void set(float value);
		}
		property DrivingStyle DrivingStyle
		{
			void set(GTA::DrivingStyle value);
		}
		property float WetnessHeight
		{
			void set(float value);
		}

		property WeaponCollection ^Weapons
		{
			WeaponCollection ^get();
		}

		bool IsInVehicle();
		bool IsInVehicle(Vehicle ^vehicle);
		bool IsSittingInVehicle();
		bool IsSittingInVehicle(Vehicle ^vehicle);
		Relationship GetRelationshipWithPed(Ped ^ped);
		void SetIntoVehicle(Vehicle ^vehicle, VehicleSeat seat);

		void Kill();
		void ResetVisibleDamage();
		void ClearBloodDamage();

	private:
		Tasks ^mTasks;
		WeaponCollection ^pWeapons;
	};
}