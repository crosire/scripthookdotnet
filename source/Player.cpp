#include "Player.hpp"
#include "Ped.hpp"
#include "Vehicle.hpp"
#include "Prop.hpp"
#include "Native.hpp"

namespace GTA
{
	Player::Player(int handle) : mHandle(handle), mPed(gcnew Ped(Native::Function::Call<int>(Native::Hash::GET_PLAYER_PED, handle)))
	{
	}

	int Player::Handle::get()
	{
		return this->mHandle;
	}

	System::String ^Player::Name::get()
	{
		return Native::Function::Call<System::String ^>(Native::Hash::GET_PLAYER_NAME, this->Handle);
	}
	System::Drawing::Color Player::Color::get()
	{
		int r = 0, g = 0, b = 0;
		Native::Function::Call(Native::Hash::GET_PLAYER_RGB_COLOUR, this->Handle, &r, &g, &b);

		return System::Drawing::Color::FromArgb(r, g, b);
	}
	Ped ^Player::Character::get()
	{
		return this->mPed;
	}
	int Player::WantedLevel::get()
	{
		return Native::Function::Call<int>(Native::Hash::GET_PLAYER_WANTED_LEVEL, this->Handle);
	}
	void Player::WantedLevel::set(int value)
	{
		Native::Function::Call(Native::Hash::SET_PLAYER_WANTED_LEVEL, this->Handle, value, false);
		Native::Function::Call(Native::Hash::SET_PLAYER_WANTED_LEVEL_NOW, this->Handle, false);
	}
	int Player::RemainingSprintTime::get()
	{
		return Native::Function::Call<int>(Native::Hash::GET_PLAYER_SPRINT_TIME_REMAINING, this->Handle);
	}
	int Player::RemainingUnderwaterTime::get()
	{
		return Native::Function::Call<int>(Native::Hash::GET_PLAYER_UNDERWATER_TIME_REMAINING, this->Handle);
	}
	bool Player::IsDead::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PLAYER_DEAD, this->Handle);
	}
	bool Player::IsAlive::get()
	{
		return !IsDead;
	}
	bool Player::IsAiming::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PLAYER_FREE_AIMING, this->Handle);
	}
	bool Player::IsOnMission::get()
	{
		return !Native::Function::Call<bool>(Native::Hash::CAN_PLAYER_START_MISSION, this->Handle);
	}
	bool Player::IsPlaying::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PLAYER_PLAYING, this->Handle);
	}
	bool Player::IsPressingHorn::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PLAYER_PRESSING_HORN, this->Handle);
	}
	bool Player::IsRidingTrain::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PLAYER_RIDING_TRAIN, this->Handle);
	}
	bool Player::IsClimbing::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PLAYER_CLIMBING, this->Handle);
	}
	Vehicle ^Player::LastVehicle::get()
	{
		return gcnew Vehicle(Native::Function::Call<int>(Native::Hash::GET_PLAYERS_LAST_VEHICLE));
	}
	int Player::Money::get()
	{
		int hash = 0;

		switch (static_cast<Native::PedHash>(this->Character->Model.Hash))
		{
			case Native::PedHash::Michael:
				hash = Native::Function::Call<int>(Native::Hash::GET_HASH_KEY, "SP0_TOTAL_CASH");
				break;
			case Native::PedHash::Franklin:
				hash = Native::Function::Call<int>(Native::Hash::GET_HASH_KEY, "SP1_TOTAL_CASH");
				break;
			case Native::PedHash::Trevor:
				hash = Native::Function::Call<int>(Native::Hash::GET_HASH_KEY, "SP2_TOTAL_CASH");
				break;
			default:
				return 0;
		}

		int value = 0;
		Native::Function::Call(Native::Hash::STAT_GET_INT, hash, &value, -1);

		return value;
	}
	void Player::Money::set(int value)
	{
		int hash = 0;

		switch (static_cast<Native::PedHash>(this->Character->Model.Hash))
		{
			case Native::PedHash::Michael:
				hash = Native::Function::Call<int>(Native::Hash::GET_HASH_KEY, "SP0_TOTAL_CASH");
				break;
			case Native::PedHash::Franklin:
				hash = Native::Function::Call<int>(Native::Hash::GET_HASH_KEY, "SP1_TOTAL_CASH");
				break;
			case Native::PedHash::Trevor:
				hash = Native::Function::Call<int>(Native::Hash::GET_HASH_KEY, "SP2_TOTAL_CASH");
				break;
			default:
				return;
		}

		Native::Function::Call(Native::Hash::STAT_SET_INT, hash, value, 1);
	}
	void Player::IgnoredByEveryone::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_EVERYONE_IGNORE_PLAYER, this->Handle, value);
	}
	void Player::CanUseCover::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_PLAYER_CAN_USE_COVER, this->Handle, value);
	}
	void Player::CanControlRagdoll::set(bool value)
	{
		Native::Function::Call(Native::Hash::GIVE_PLAYER_RAGDOLL_CONTROL, this->Handle, value);
	}
	bool Player::CanControlCharacter::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PLAYER_CONTROL_ON, this->Handle);
	}
	void Player::CanControlCharacter::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_PLAYER_CONTROL, this->Handle, value, 0);
	}

	bool Player::IsTargettingAnything::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PLAYER_TARGETTING_ANYTHING, this->Handle);
	}
	bool Player::IsTargetting(Entity ^entity)
	{
		return Native::Function::Call<bool>(Native::Hash::IS_PLAYER_FREE_AIMING_AT_ENTITY, this->Handle, entity->Handle);
	}
	Entity ^Player::GetTargetedEntity()
	{
		int entity = 0;

		if (Native::Function::Call<bool>(Native::Hash::_GET_AIMED_ENTITY, this->Handle, &entity))
		{
			if (!Native::Function::Call<bool>(Native::Hash::DOES_ENTITY_EXIST, entity))
			{
				return nullptr;
			}
			else if (Native::Function::Call<bool>(Native::Hash::IS_ENTITY_A_PED, entity))
			{
				return gcnew Ped(entity);
			}
			else if (Native::Function::Call<bool>(Native::Hash::IS_ENTITY_A_VEHICLE, entity))
			{
				return gcnew Vehicle(entity);
			}
			else if (Native::Function::Call<bool>(Native::Hash::IS_ENTITY_AN_OBJECT, entity))
			{
				return gcnew Prop(entity);
			}
		}

		return nullptr;
	}
	bool Player::Equals(Player ^player)
	{
		return !System::Object::ReferenceEquals(player, nullptr) && this->Handle == player->Handle;
	}
	int Player::GetHashCode()
	{
		return this->Handle;
	}
}