#pragma once

#include "Vector3.hpp"
#include "ScriptDomain.hpp"

namespace GTA
{
	namespace Native
	{
		private ref class MemoryAccess abstract sealed
		{
		internal:
			static MemoryAccess();

			static System::UInt64(*GetAddressOfEntity)(int handle);
			static System::UInt64(*GetAddressOfPlayer)(int handle);

			static array<int> ^GetVehicleHandles();
			static array<int> ^GetVehicleHandles(array<int> ^modelhashes);
			static array<int> ^GetVehicleHandles(Math::Vector3 position, float radius);
			static array<int> ^GetVehicleHandles(Math::Vector3 position, float radius, array<int> ^modelhashes);
			static array<int> ^GetPedHandles();
			static array<int> ^GetPedHandles(array<int> ^modelhashes);
			static array<int> ^GetPedHandles(Math::Vector3 position, float radius);
			static array<int> ^GetPedHandles(Math::Vector3 position, float radius, array<int> ^modelhashes);
			static array<int> ^GetPropHandles();
			static array<int> ^GetPropHandles(array<int> ^modelhashes);
			static array<int> ^GetPropHandles(Math::Vector3 position, float radius);
			static array<int> ^GetPropHandles(Math::Vector3 position, float radius, array<int> ^modelhashes);
			static array<int> ^GetEntityHandles();
			static array<int> ^GetEntityHandles(Math::Vector3 position, float radius);

			static int(*AddEntityToPool)(System::UInt64 address);
			static System::UInt64(*GetEntityPos)(System::UInt64 address, float *position);
			static System::UInt64(*GetEntityModel1)(System::UInt64 address), (*GetEntityModel2)(System::UInt64 address);
			static System::UInt64 *EntityPoolAddress, *VehiclePoolAddress, *PedPoolAddress, *ObjectPoolAddress;
			static System::UInt64 SetNmIntAddress, SetNmFloatAddress, SetNmBoolAddress, SetNmStringAddress, SetNmVec3Address, CreateNmMessageFunc, GiveNmMessageFunc;

		private:
			static System::UInt64 FindPattern(const char *pattern, const char *mask);
		};
	}
}
