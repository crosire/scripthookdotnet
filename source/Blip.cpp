#include "Blip.hpp"
#include "Native.hpp"

namespace GTA
{
	Blip::Blip(int handle) : mHandle(handle)
	{
	}

	int Blip::Handle::get()
	{
		return this->mHandle;
	}
	Math::Vector3 Blip::Position::get()
	{
		return Native::Function::Call<Math::Vector3>(Native::Hash::GET_BLIP_INFO_ID_COORD, this->Handle);
	}
	void Blip::Position::set(Math::Vector3 value)
	{
		Native::Function::Call(Native::Hash::SET_BLIP_COORDS, this->Handle, value.X, value.Y, value.Z);
	}
	void Blip::Scale::set(float scale)
	{
		Native::Function::Call(Native::Hash::SET_BLIP_SCALE, this->Handle, scale);
	}
	bool Blip::IsFlashing::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_BLIP_FLASHING, this->Handle);
	}
	void Blip::IsFlashing::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_BLIP_FLASHES, this->Handle, value);
	}
	BlipColor Blip::Color::get()
	{
		return BlipColor(Native::Function::Call<int>(Native::Hash::GET_BLIP_COLOUR, this->Handle));
	}
	void Blip::Color::set(BlipColor color)
	{
		Native::Function::Call(Native::Hash::SET_BLIP_COLOUR, this->Handle, (int)color);
	}
	int Blip::Alpha::get()
	{
		return Native::Function::Call<int>(Native::Hash::GET_BLIP_ALPHA, this->Handle);
	}
	void Blip::Alpha::set(int alpha)
	{
		Native::Function::Call(Native::Hash::SET_BLIP_ALPHA, this->Handle, alpha);
	}
	void Blip::ShowRoute::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_BLIP_ROUTE, this->Handle, value);
	}
	BlipSprite Blip::Sprite::get()
	{
		return BlipSprite(Native::Function::Call<int>(Native::Hash::GET_BLIP_SPRITE, this->Handle));
	}
	void Blip::Sprite::set(BlipSprite sprite)
	{
		Native::Function::Call(Native::Hash::SET_BLIP_SPRITE, this->Handle, (int)sprite);
	}
	int Blip::Type::get()
	{
		return Native::Function::Call<int>(Native::Hash::GET_BLIP_INFO_ID_TYPE, this->Handle);
	}

	bool Blip::IsOnMinimap::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_BLIP_ON_MINIMAP, this->Handle);
	}
	bool Blip::IsShortRange::get()
	{
		return Native::Function::Call<bool>(Native::Hash::IS_BLIP_SHORT_RANGE, this->Handle);
	}
	void Blip::IsShortRange::set(bool value)
	{
		Native::Function::Call(Native::Hash::SET_BLIP_AS_SHORT_RANGE, this->Handle, value);
	}

	void Blip::ShowNumber(int number)
	{
		Native::Function::Call(Native::Hash::SHOW_NUMBER_ON_BLIP, this->Handle, number);
	}
	void Blip::HideNumber()
	{
		Native::Function::Call(Native::Hash::HIDE_NUMBER_ON_BLIP, this->Handle);
	}
	bool Blip::Exists()
	{
		return Native::Function::Call<bool>(Native::Hash::DOES_BLIP_EXIST, this->Handle);
	}
	void Blip::Remove()
	{
		int id = this->Handle;
		Native::Function::Call(Native::Hash::REMOVE_BLIP, &id);
	}
	void Blip::SetAsFriendly()
	{
		Native::Function::Call(Native::Hash::SET_BLIP_AS_FRIENDLY, this->Handle, 1);
	}
	void Blip::SetAsHostile()
	{
		Native::Function::Call(Native::Hash::SET_BLIP_AS_FRIENDLY, this->Handle, 0);
	}
}