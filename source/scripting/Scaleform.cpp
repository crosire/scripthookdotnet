#include "Scaleform.hpp"
#include "UI.hpp"
#include "Native.hpp"

namespace GTA
{
	using namespace System;

	extern void Log(String ^logLevel, ... array<String ^> ^message);

	Scaleform::Scaleform()
	{
	}
	Scaleform::Scaleform(int handle) : _handle(handle)
	{
	}

	int Scaleform::Handle::get()
	{
		return _handle;
	}

	bool Scaleform::Load(String ^scaleformID)
	{
		const int handle = Native::Function::Call<int>(Native::Hash::REQUEST_SCALEFORM_MOVIE, scaleformID);

		if (handle == 0)
		{
			return false;
		}

		_handle = handle;
		_scaleformID = scaleformID;

		return true;
	}

	void Scaleform::CallFunction(String ^function, ... array<Object ^> ^arguments)
	{
		Native::Function::Call(Native::Hash::_PUSH_SCALEFORM_MOVIE_FUNCTION, Handle, function);

		for each (Object ^argument in arguments)
		{
			if (argument->GetType() == Int32::typeid)
			{
				Native::Function::Call(Native::Hash::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, static_cast<int>(argument));
			}
			else if (argument->GetType() == String::typeid)
			{
				Native::Function::Call(Native::Hash::_BEGIN_TEXT_COMPONENT, "STRING");
				Native::Function::Call(Native::Hash::ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, static_cast<String ^>(argument));
				Native::Function::Call(Native::Hash::_END_TEXT_COMPONENT);
			}
			else if (argument->GetType() == Char::typeid)
			{
				Native::Function::Call(Native::Hash::_BEGIN_TEXT_COMPONENT, "STRING");
				Native::Function::Call(Native::Hash::ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, static_cast<char>(argument).ToString());
				Native::Function::Call(Native::Hash::_END_TEXT_COMPONENT);
			}
			else if (argument->GetType() == Single::typeid)
			{
				Native::Function::Call(Native::Hash::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_FLOAT, static_cast<float>(argument));
			}
			else if (argument->GetType() == Double::typeid)
			{
				Native::Function::Call(Native::Hash::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_FLOAT, static_cast<float>(static_cast<double>(argument)));
			}
			else if (argument->GetType() == Boolean::typeid)
			{
				Native::Function::Call(Native::Hash::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_BOOL, static_cast<bool>(argument));
			}
			else if (argument->GetType() == ScaleformArgumentTXD::typeid)
			{
				Native::Function::Call(Native::Hash::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_STRING, static_cast<ScaleformArgumentTXD ^>(argument)->_txd);
			}
			else
			{
				Log("[ERROR]", String::Format("Unknown argument type {0} passed to scaleform with handle {1}.", argument->GetType()->Name, Handle));
			}
		}

		Native::Function::Call(Native::Hash::_POP_SCALEFORM_MOVIE_FUNCTION_VOID);
	}

	void Scaleform::Render2D()
	{
		Native::Function::Call(Native::Hash::DRAW_SCALEFORM_MOVIE_FULLSCREEN, Handle, 255, 255, 255, 255, 0);
	}
	void Scaleform::Render2DScreenSpace(Drawing::PointF location, Drawing::PointF size)
	{
		float x = location.X / UI::WIDTH;
		float y = location.Y / UI::HEIGHT;
		float width = size.X / UI::WIDTH;
		float height = size.Y / UI::HEIGHT;

		Native::Function::Call(Native::Hash::DRAW_SCALEFORM_MOVIE, Handle, x + (width / 2.0f), y + (height / 2.0f), width, height, 255, 255, 255, 255);
	}
	void Scaleform::Render3D(Math::Vector3 position, Math::Vector3 rotation, Math::Vector3 scale)
	{
		Native::Function::Call(Native::Hash::_DRAW_SCALEFORM_MOVIE_3D_NON_ADDITIVE, Handle, position.X, position.Y, position.Z, rotation.X, rotation.Y, rotation.Z, 2.0f, 2.0f, 1.0f, scale.X, scale.Y, scale.Z, 2);
	}
	void Scaleform::Render3DAdditive(Math::Vector3 position, Math::Vector3 rotation, Math::Vector3 scale)
	{
		Native::Function::Call(Native::Hash::DRAW_SCALEFORM_MOVIE_3D, Handle, position.X, position.Y, position.Z, rotation.X, rotation.Y, rotation.Z, 2.0f, 2.0f, 1.0f, scale.X, scale.Y, scale.Z, 2);
	}
}
