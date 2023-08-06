//
// Copyright (C) 2023 kagikn & contributors
// License: https://github.com/scripthookvdotnet/scripthookvdotnet#license
//

using System;

namespace GTA.Graphics
{
	/// <summary>
	/// Represents a struct that contains a <see cref="Txd"/> and a texture name <see cref="string"/>.
	/// </summary>
	/// <remarks>
	/// You should not use the default constructor. The fallback behavior can be changed from filling in the 2 values
	/// with <see langword="null"/> after the codebase of SHVDN starts to use C# 10 or later C# version.
	/// </remarks>
	public readonly struct TxdAndTextureNamePair : IEquatable<TxdAndTextureNamePair>
	{
		public TxdAndTextureNamePair(string txdName, string texName) : this(new Txd(txdName), texName)
		{
		}
		public TxdAndTextureNamePair(Txd txd, string texName)
		{
			Txd = txd;
			TextureName = texName;
		}

		/// <summary>
		/// Gets the <see cref="Txd"/> struct of texture dictionary name.
		/// </summary>
		public Txd Txd
		{
			get; init;
		}
		/// <summary>
		/// Gets the texture name.
		/// </summary>
		public string TextureName
		{
			get; init;
		}

		public bool Equals(TxdAndTextureNamePair other)
			=> Txd == other.Txd && TextureName == other.TextureName;
		public override bool Equals(object obj)
		{
			if (obj is TxdAndTextureNamePair txdAndTexNamePair)
			{
				return Equals(txdAndTexNamePair);
			}

			return false;
		}

		public static bool operator ==(TxdAndTextureNamePair left, TxdAndTextureNamePair right)
			=> left.Equals(right);
		public static bool operator !=(TxdAndTextureNamePair left, TxdAndTextureNamePair right)
			=> !left.Equals(right);

		public override int GetHashCode() => Txd.GetHashCode() * 17 + TextureName.GetHashCode();

		public void Deconstruct(out Txd txd, out string texName)
		{
			txd = Txd;
			texName = TextureName;
		}
	}
}
