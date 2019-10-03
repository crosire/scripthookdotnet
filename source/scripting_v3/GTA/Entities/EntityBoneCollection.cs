using System.Collections;
using System.Collections.Generic;
using GTA.Native;

namespace GTA
{
	public class EntityBoneCollection : IEnumerable<EntityBone>
	{
		public class Enumerator : IEnumerator<EntityBone>
		{
			#region Fields
			private readonly EntityBoneCollection _collection;
			private int currentIndex = -1;// skip the CORE bone index(-1)
			#endregion

			public Enumerator(EntityBoneCollection collection)
			{
				_collection = collection;
			}
			public EntityBone Current => _collection[currentIndex];

			object IEnumerator.Current => _collection[currentIndex];

			public void Dispose()
			{

			}

			public bool MoveNext()
			{
				return ++currentIndex < _collection.Count;
			}

			public void Reset()
			{
				currentIndex = -1;
			}
		}

		#region Fields
		protected readonly Entity _owner;
		#endregion

		internal EntityBoneCollection(Entity owner)
		{
			_owner = owner;
		}

		/// <summary>
		/// Gets the <see cref="EntityBone"/> with the specified bone name.
		/// </summary>
		/// <param name="boneName">Name of the bone.</param>
		public EntityBone this[string boneName]
		{
			get { return new EntityBone(_owner, boneName); }
		}

		/// <summary>
		/// Gets the <see cref="EntityBone"/> at the specified bone index.
		/// </summary>
		/// <param name="boneIndex">The bone index.</param>
		public EntityBone this[int boneIndex]
		{
			get { return new EntityBone(_owner, boneIndex); }
		}

		/// <summary>
		/// Determines whether this <see cref="Entity"/> has a bone with the specified bone name
		/// </summary>
		/// <param name="boneName">Name of the bone.</param>
		/// <returns>
		///   <c>true</c> if this <see cref="Entity"/> has a bone with the specified bone name; otherwise, <c>false</c>.
		/// </returns>
		public bool HasBone(string boneName)
		{
			return Function.Call<int>(Hash.GET_ENTITY_BONE_INDEX_BY_NAME, _owner.Handle, boneName) !=  -1;
		}

		/// <summary>
		/// Gets the number of bones that this <see cref="Entity"/> has.
		/// </summary>
		public int Count => SHVDN.NativeMemory.GetEntityBoneCount(_owner.Handle);

		public EntityBone Core => new EntityBone(_owner, -1);

		public IEnumerator<EntityBone> GetEnumerator()
		{
			return new Enumerator(this);
		}

		public override int GetHashCode()
		{
			return _owner.GetHashCode() ^ Count.GetHashCode();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}