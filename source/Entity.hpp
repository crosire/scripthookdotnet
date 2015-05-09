#pragma once

#include "Model.hpp"
#include "Vector3.hpp"

namespace GTA
{
	public ref class Entity abstract
	{
	public:
		Entity(int id);

		property int ID
		{
			int get();
		}
		property Math::Vector3 Position
		{
			Math::Vector3 get();
			void set(Math::Vector3 value);
		}
		property float HeightAboveGround
		{
			float get();
		}
		property float Heading
		{
			float get();
			void set(float value);
		}
		property Math::Vector3 Rotation
		{
			Math::Vector3 get();
		}
		property Math::Vector3 ForwardVector
		{
			Math::Vector3 get();
		}
		property Math::Vector3 Velocity
		{
			Math::Vector3 get();
			void set(Math::Vector3 value);
		}
		property bool FreezePosition
		{
			void set(bool value);
		}
		property int Health
		{
			int get();
			void set(int value);
		}
		property int MaxHealth
		{
			int get();
			void set(int value);
		}
		property GTA::Model Model
		{
			GTA::Model get();
		}
		property bool IsDead
		{
			bool get();
		}
		property bool IsAlive
		{
			bool get();
		}
		property bool IsInvincible
		{
			void set(bool value);
		}
		property bool IsVisible
		{
			bool get();
			void set(bool value);
		}
		property bool IsOccluded
		{
			bool get();
		}
		property bool IsOnScreen
		{
			bool get();
		}
		property bool IsUpright
		{
			bool get();
		}
		property bool IsUpsideDown
		{
			bool get();
		}
		property bool IsInAir
		{
			bool get();
		}
		property bool IsInWater
		{
			bool get();
		}
		property bool IsOnFire
		{
			bool get();
		}
		property bool IsPersistent
		{
			bool get();
			void set(bool value);
		}

		void ApplyForce(Math::Vector3 direction);
		void ApplyForce(Math::Vector3 direction, Math::Vector3 rotation);
		void ApplyForceRelative(Math::Vector3 direction);
		void ApplyForceRelative(Math::Vector3 direction, Math::Vector3 rotation);

		float GetDistanceTo(Math::Vector3 coordinate);
		

		bool Exists();
		static bool Exists(Entity ^entity);
		void MarkAsNoLongerNeeded();
		virtual bool Equals(Entity ^entity);

		virtual int GetHashCode() override;
		static inline bool operator ==(Entity ^left, Entity ^right)
		{
			if (Object::ReferenceEquals(left, nullptr))
			{
				return Object::ReferenceEquals(right, nullptr);
			}

			return left->Equals(right);
		}
		static inline bool operator !=(Entity ^left, Entity ^right)
		{
			return !operator ==(left, right);
		}

	private:
		int mID;
	};
}