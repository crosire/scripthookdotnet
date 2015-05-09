#include "Ped.hpp"
#include "Vehicle.hpp"
#include "Tasks.hpp"
#include "Native.hpp"

namespace GTA
{
	Ped::Ped(int id) : Entity(id), mTasks(gcnew Tasks(this))
	{
	}

	Ped ^Ped::Any::get()
	{
		return gcnew Ped(0);
	}
	int Ped::Accuracy::get()
	{
		return Native::Function::Call<int>(Native::Hash::GET_PED_ACCURACY, this->ID);
	}
	void Ped::Accuracy::set(int value)
	{
		Native::Function::Call(Native::Hash::SET_PED_ACCURACY, this->ID, value);
	}
	Tasks ^Ped::Task::get()
	{
		return this->mTasks;
	}
	GTA::Gender Ped::Gender::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_MALE, this->ID) ? GTA::Gender::Male : GTA::Gender::Female;
	}
	int Ped::Armor::get()
	{
		return Native::Function::Call<int>(Native::Hash::GET_PED_ARMOUR, this->ID);
	}
	void Ped::Armor::set(int value)
	{
		Native::Function::Call(Native::Hash::SET_PED_ARMOUR, this->ID, value);
	}
	int Ped::Money::get()
	{
		return Native::Function::Call<int>(Native::Hash::GET_PED_MONEY, this->ID);
	}
	void Ped::Money::set(int value)
	{
		Native::Function::Call(Native::Hash::SET_PED_MONEY, this->ID, value);
	}
	bool Ped::IsPlayer::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_A_PLAYER, this->ID);
	}
	bool Ped::IsHuman::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_HUMAN, this->ID);
	}
	bool Ped::IsIdle::get()
	{
		return !IsInjured && !IsRagdoll && !IsInAir && !IsOnFire && !IsDucking && !IsGettingIntoAVehicle && !IsInCombat && !IsInMeleeCombat && !(IsInVehicle() && !IsSittingInVehicle());
	}
	bool Ped::IsProne::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_PRONE, this->ID);
	}
	bool Ped::IsDucking::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_DUCKING, this->ID);
	}
	void Ped::IsDucking::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_PED_DUCKING, this->ID, value);
	}
	bool Ped::IsGettingUp::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_GETTING_UP, this->ID);
	}
	bool Ped::IsGettingIntoAVehicle::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_GETTING_INTO_A_VEHICLE, this->ID);
	}
	bool Ped::IsRagdoll::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_RAGDOLL, this->ID);
	}
	bool Ped::IsInjured::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_INJURED, this->ID);
	}
	bool Ped::IsShooting::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_SHOOTING, this->ID);
	}
	bool Ped::IsInCombat::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_IN_COMBAT, this->ID);
	}
	bool Ped::IsInMeleeCombat::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_IN_MELEE_COMBAT, this->ID);
	}
	bool Ped::IsSwimming::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_SWIMMING, this->ID);
	}
	bool Ped::IsSwimmingUnderWater::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_SWIMMING_UNDER_WATER, this->ID);
	}
	Vehicle ^Ped::CurrentVehicle::get()
	{
		if (!IsInVehicle())
		{
			return nullptr;
		}
		return gcnew Vehicle(Native::Function::Call<int>(Native::Hash::GET_VEHICLE_PED_IS_IN, this->ID, false));
	}
	void Ped::IsEnemy::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_PED_AS_ENEMY, this->ID, value);
	}
	void Ped::IsPriorityTargetForEnemies::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_ENTITY_IS_TARGET_PRIORITY, this->ID, value, 0);
	}
	void Ped::AlwaysDiesOnLowHealth::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_PED_DIES_WHEN_INJURED, this->ID, value);
	}
	void Ped::BlockPermanentEvents::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, this->ID, value);
	}
	void Ped::CanRagdoll::set(bool value)
	{
		Native::Function::Call(Native::Hash::CAN_PED_RAGDOLL, this->ID, value);
	}
	void Ped::CanSwitchWeapons::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_PED_CAN_SWITCH_WEAPON, this->ID, value);
	}
	void Ped::CanBeKnockedOffBike::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_PED_CAN_BE_KNOCKED_OFF_VEHICLE, this->ID, value);
	}
	void Ped::CanBeDraggedOutOfVehicle::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_PED_CAN_BE_DRAGGED_OUT, this->ID, value);
	}
	void Ped::CanPlayGestures::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_PED_CAN_PLAY_GESTURE_ANIMS, this->ID, value);
	}
	bool Ped::IsInVehicle()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_IN_ANY_VEHICLE, this->ID, 0);
	}
	bool Ped::IsInVehicle(Vehicle ^vehicle)
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_IN_VEHICLE, this->ID, vehicle->ID, 0);
	}
	bool Ped::IsSittingInVehicle()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_SITTING_IN_ANY_VEHICLE, this->ID);
	}
	bool Ped::IsSittingInVehicle(Vehicle ^vehicle)
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PED_SITTING_IN_VEHICLE, this->ID, vehicle->ID);
	}
	void Ped::Kill()
	{
		Health = -1;
	}
	float Ped::GetDistanceTo(Math::Vector3 coordinate)
	{
		return Native::Function::Call<float>(Native::Hash::GET_DISTANCE_BETWEEN_COORDS, this->Position.X, this->Position.Y, this->Position.Z, coordinate.X, coordinate.Y, coordinate.Z, 1);
	}
}