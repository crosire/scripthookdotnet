#include "Pickup.hpp"
#include "Native.hpp"

namespace GTA
{
	Pickup::Pickup(int handle) : _handle(handle)
	{
	}

	int Pickup::Handle::get()
	{
		return _handle;
	}
	bool Pickup::IsCollected::get()
	{
		return Native::Function::Call<bool>(Native::Hash::HAS_PICKUP_BEEN_COLLECTED, Handle);
	}
	Math::Vector3 Pickup::Position::get()
	{
		return Native::Function::Call<Math::Vector3>(Native::Hash::GET_PICKUP_COORDS, Handle);
	}

	bool Pickup::Exists()
	{
		return Native::Function::Call<bool>(Native::Hash::DOES_PICKUP_EXIST, Handle);
	}
	bool Pickup::ObjectExists()
	{
		return Native::Function::Call<bool>(Native::Hash::DOES_PICKUP_OBJECT_EXIST, Handle);
	}
	void Pickup::Delete()
	{
		return Native::Function::Call(Native::Hash::REMOVE_PICKUP, Handle);
	}
}