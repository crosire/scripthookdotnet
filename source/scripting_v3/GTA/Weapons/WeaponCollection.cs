//
// Copyright (C) 2015 crosire & contributors
// License: https://github.com/crosire/scripthookvdotnet#license
//

using GTA.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GTA
{
	public sealed class WeaponCollection : IEnumerable<Weapon>, IEnumerable
	{
		[StructLayout(LayoutKind.Explicit, Size = 0xC)]
		unsafe internal struct RageAtArrayPtr
		{
			[FieldOffset(0x0)]
			internal ulong* data;
			[FieldOffset(0x8)]
			internal ushort size;
			[FieldOffset(0xA)]
			internal ushort capacity;

			internal ulong GetElementAddress(int i)
			{
				return data[i];
			}
		}

		[StructLayout(LayoutKind.Explicit, Size = 0x14)]
		struct ItemInfo
		{
			[FieldOffset(0x10)]
			internal uint nameHash;
		}

		#region Fields
		readonly Ped owner;
		readonly Dictionary<WeaponHash, Weapon> weapons = new();
		#endregion

		internal WeaponCollection(Ped owner)
		{
			this.owner = owner;
		}

		/// <summary>
		/// Gets the <see cref="Weapon"/> associated with the specified <see cref="WeaponHash"/>.
		/// </summary>
		/// <param name="hash">The <see cref="WeaponHash"/> of the <see cref="Weapon"/> to get</param>
		/// <value>
		/// The value associated with the specified <see cref="WeaponHash"/>.
		/// If the specified <see cref="WeaponHash"/> is not found, this property will return <see langword="null"/>.
		/// </value>
		public Weapon this[WeaponHash hash]
		{
			get
			{
				if (weapons.TryGetValue(hash, out var weapon)) return weapon;
				if (!Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, owner.Handle, (uint)hash, 0))
				{
					return null;
				}

				weapon = new Weapon(owner, hash);
				weapons.Add(hash, weapon);

				return weapon;
			}
		}

		public IEnumerator<Weapon> GetEnumerator()
		{
			var currentIndex = 0;
			while (true)
			{
				var currentWeaponInstance = GetWeaponInstanceByIndexOfPedInventory(currentIndex++);
				if (currentWeaponInstance == null)
				{
					yield break;
				}

				yield return currentWeaponInstance;
			}
		}

		private Weapon GetWeaponInstanceByIndexOfPedInventory(int index)
		{
			unsafe
			{
				var pedInventoryAddr = SHVDN.NativeMemory.GetCPedInventoryAddressFromPedHandle(owner.Handle);
				if (pedInventoryAddr == IntPtr.Zero)
				{
					return null;
				}

				var weaponInventoryArray = (RageAtArrayPtr*)(pedInventoryAddr + 0x18);
				if (index >= weaponInventoryArray->size)
				{
					return null;
				}

				var itemAddress = weaponInventoryArray->GetElementAddress(index);
				var weaponInfo = *(ItemInfo**)(itemAddress + 0x8);
				if (weaponInfo == null)
				{
					return null;
				}

				var weaponHash = (WeaponHash)weaponInfo->nameHash;
				if (weapons.TryGetValue(weaponHash, out var weapon)) return weapon;

				weapon = new Weapon(owner, weaponHash);
				weapons.Add(weaponHash, weapon);

				return weapon;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Gets an <c>array</c> of all <see cref="WeaponHash"/>es this <see cref="WeaponCollection"/> has.
		/// </summary>
		public WeaponHash[] GetAllWeaponHashes()
		{
			return Array.ConvertAll(SHVDN.NativeMemory.GetAllWeaponHashesOfPedInventory(owner.Handle), x => (WeaponHash)x);
		}

		/// <summary>
		/// Gets the <see cref="WeaponHash"/> associated with the slot hash in the weapon inventory of the owner <see cref="Ped"/>.
		/// Can fetch the weapon hash faster than <see cref="this[WeaponHash]"/> since the internal weapon inventory array is sorted in acsending order by slot hashes.
		/// </summary>
		/// <param name="slotHash">The slot hash key of the value to get.</param>
		/// <param name="weaponHash">
		/// When this method returns, contains the <see cref="WeaponHash"/> associated with the slot hash,
		/// if the weapon inventory of the owner <see cref="Ped"/> has a weapon for the slot hash; otherwise, the zero <see cref="WeaponHash"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns><see langword="true"/> if the <see cref="WeaponCollection"/> contains a <see cref="WeaponHash"/> with the specified slot hash; otherwise, <see langword="false"/>.</returns>
		public bool TryGetWeaponHashBySlotHash(int slotHash, out WeaponHash weaponHash)
		{
			var foundWeaponHash = SHVDN.NativeMemory.TryGetWeaponHashInPedInventoryBySlotHash(owner.Handle, (uint)slotHash, out uint weaponHashUInt);

			weaponHash = (WeaponHash)weaponHashUInt;
			return foundWeaponHash;
		}
		/// <summary>
		/// Gets the <see cref="Weapon"/> associated with the slot hash in the weapon inventory of the owner <see cref="Ped"/>.
		/// Can fetch the weapon hash faster than <see cref="this[WeaponHash]"/> since the internal weapon inventory array is sorted in acsending order by slot hashes.
		/// </summary>
		/// <param name="slotHash">The slot hash key of the value to get.</param>
		/// <param name="weapon">
		/// When this method returns, contains the <see cref="Weapon"/> associated with the slot hash,
		/// if the weapon inventory of the owner <see cref="Ped"/> has a weapon for the slot hash; otherwise, <see langword="null"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns><see langword="true"/> if the <see cref="WeaponCollection"/> contains a <see cref="Weapon"/> with the specified slot hash; otherwise, <see langword="false"/>.</returns>
		public bool TryGetWeaponBySlotHash(int slotHash, out Weapon weapon)
		{
			if (!TryGetWeaponHashBySlotHash(slotHash, out var weaponHash))
			{
				weapon = null;
				return false;
			}

			if (weapons.TryGetValue(weaponHash, out weapon)) return true;

			weapon = new Weapon(owner, weaponHash);
			weapons.Add(weaponHash, weapon);

			return true;
		}

		/// <summary>
		/// Gets the current <see cref="Weapon"/>.
		/// </summary>
		/// <remarks>
		/// Returns the target <see cref="Weapon"/> if the <c>CWeaponInventory</c> of the owner <see cref="Ped"/> is trying to switch the current weapon to another
		/// but not finished doing (e.g. calls <see cref="Select(WeaponHash)"/> on the <see cref="Ped"/> but they are ragdolling at that time).
		/// </remarks>
		public Weapon Current
		{
			get
			{
				int currentWeapon;
				unsafe
				{
					Function.Call(Hash.GET_CURRENT_PED_WEAPON, owner.Handle, &currentWeapon, true);
				}

				var hash = (WeaponHash)currentWeapon;

				if (weapons.TryGetValue(hash, out var current))
				{
					return current;
				}

				var weapon = new Weapon(owner, hash);
				weapons.Add(hash, weapon);

				return weapon;
			}
		}

		public Weapon BestWeapon
		{
			get
			{
				var hash = Function.Call<WeaponHash>(Hash.GET_BEST_PED_WEAPON, owner.Handle, 0);

				if (weapons.TryGetValue(hash, out var bestWeapon))
				{
					return bestWeapon;
				}

				var weapon = new Weapon(owner, (WeaponHash)hash);
				weapons.Add(hash, weapon);

				return weapon;
			}
		}

		/// <summary>
		/// Gets the value that indicates whether the owner <see cref="Ped"/> has the weapon for <paramref name="weaponHash"/>.
		/// </summary>
		/// <remarks>
		/// Returns <see langword="true"/> for <see cref="WeaponHash.Unarmed"/> unless the item for the hash is removed from <c>CWeaponInventory</c> of the owner <see cref="Ped"/>.
		/// </remarks>
		public bool HasWeapon(WeaponHash weaponHash)
		{
			return Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, owner.Handle, (uint)weaponHash);
		}

		public bool IsWeaponValid(WeaponHash hash)
		{
			return Function.Call<bool>(Hash.IS_WEAPON_VALID, (uint)hash);
		}

		/// <summary>
		/// Gets the current weapon <see cref="Prop"/>.
		/// </summary>
		/// <remarks>
		/// Always check if the returned value is valid with the null check and <see cref="Entity.Exists"/>.
		/// This method returns <see langword="null"/> if the current weapon is <see cref="WeaponHash.Unarmed"/>, but always returns a <see cref="Prop"/> instance otherwise
		/// even if the owner <see cref="Ped"/> is not using the weapon <see cref="Prop"/> (For example, when the <see cref="Ped"/> is ragdolling and the current weapon cannot hold with one hand),
		/// which is kept for compatibility as calling methods on a invalid <see cref="Prop"/> will not cause serious issues in general (just do nothing or return zero values in most cases).
		/// </remarks>
		public Prop CurrentWeaponObject
		{
			get
			{
				if (Current.Hash == WeaponHash.Unarmed)
				{
					return null;
				}

				return new Prop(Function.Call<int>(Hash.GET_CURRENT_PED_WEAPON_ENTITY_INDEX, owner.Handle));
			}
		}

		public bool Select(Weapon weapon)
		{
			if (!weapon.IsPresent)
			{
				return false;
			}

			Function.Call(Hash.SET_CURRENT_PED_WEAPON, owner.Handle, (uint)weapon.Hash, true);

			return true;
		}
		public bool Select(WeaponHash weaponHash)
		{
			return Select(weaponHash, true);
		}
		/// <summary>
		/// Selects the specified weapon.
		/// </summary>
		/// <param name="weaponHash">The weapon hash.</param>
		/// <param name="equipNow">Specifies if the owner ped will equip in hands immediately.</param>
		/// <returns></returns>
		public bool Select(WeaponHash weaponHash, bool equipNow)
		{
			if (!Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, owner.Handle, (uint)weaponHash))
			{
				return false;
			}

			Function.Call(Hash.SET_CURRENT_PED_WEAPON, owner.Handle, (uint)weaponHash, equipNow);

			return true;
		}


		/// <summary>
		/// Gives the specified weapon if the owner <see cref="Ped"/> does not have one, or selects the weapon if they have one and <paramref name="equipNow"/> is set to <see langword="true" />.
		/// </summary>
		/// <param name="weaponHash">The weapon hash.</param>
		/// <param name="ammoCount">The ammo count to be added to the weapon inventory of the owner <see cref="Ped"/>.</param>
		/// <param name="equipNow">If set to <see langword="true" />, the owner <see cref="Ped"/> will switch their weapon to the weapon of <paramref name="weaponHash"/> as soon as they can (not instantly).</param>
		/// <param name="isAmmoLoaded">
		/// Does not work since the ammo in clip is always full if not selected unless the game code related to auto-reload is modified.
		/// This was supposed to determine if the ammo will be loaded after the weapon is given to the owner <see cref="Ped"/>.
		/// </param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
		public Weapon Give(WeaponHash weaponHash, int ammoCount, bool equipNow, bool isAmmoLoaded)
		{
			if (!weapons.TryGetValue(weaponHash, out var weapon))
			{
				weapon = new Weapon(owner, weaponHash);
				weapons.Add(weaponHash, weapon);
			}

			if (weapon.IsPresent)
			{
				if (equipNow)
				{
					Select(weapon);
				}
			}
			else
			{
				// Set the 4th argument to false for consistency. If 4th argument is set to true when 5th one is set to true, the ped will instantly select the added weapon in any case.
				Function.Call(Hash.GIVE_WEAPON_TO_PED, owner.Handle, (uint)weapon.Hash, ammoCount, false, equipNow);
			}

			return weapon;
		}

		public Weapon Give(string name, int ammoCount, bool equipNow, bool isAmmoLoaded)
		{
			return Give((WeaponHash)Game.GenerateHash(name), ammoCount, equipNow, isAmmoLoaded);
		}

		/// <summary>
		/// Drops the current weapon and creates a pickup <see cref="Prop"/> with the owner address set to that of the owner <see cref="Ped"/>.
		/// </summary>
		public void Drop()
		{
			Function.Call(Hash.SET_PED_DROPS_WEAPON, owner.Handle);
		}

		/// <summary>
		/// Removes the specified weapon.
		/// </summary>
		/// <remarks>
		/// This method can remove <see cref="WeaponHash.Unarmed"/> from the weapon inventory.
		/// </remarks>
		public void Remove(Weapon weapon)
		{
			var hash = weapon.Hash;

			if (weapons.ContainsKey(hash))
			{
				weapons.Remove(hash);
			}

			Remove(weapon.Hash);
		}
		/// <inheritdoc cref="Remove(Weapon)"/>
		public void Remove(WeaponHash weaponHash)
		{
			Function.Call(Hash.REMOVE_WEAPON_FROM_PED, owner.Handle, (uint)weaponHash);
		}

		/// <summary>
		/// Removes all weapons from the weapon inventory except for <see cref="WeaponHash.Unarmed"/>.
		/// </summary>
		public void RemoveAll()
		{
			Function.Call(Hash.REMOVE_ALL_PED_WEAPONS, owner.Handle, true);

			weapons.Clear();
		}
	}
}
