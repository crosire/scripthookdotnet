//
// Copyright (C) 2015 crosire & kagikn & contributors
// License: https://github.com/scripthookvdotnet/scripthookvdotnet#license
//

using GTA.Math;
using GTA.Native;
using System;

namespace GTA
{
	/// <summary>
	/// Represents the gameplay camera director.
	/// The gameplay director is responsible for the follow and aim cameras.
	/// </summary>
	public static class GameplayCamera
	{
		/// <summary>
		/// Gets the memory address of the gameplay camera director.
		/// </summary>
		public static IntPtr MemoryAddress => SHVDN.NativeMemory.GetGameplayCameraAddress();

		/// <summary>
		/// Gets the matrix of the <see cref="GameplayCamera"/> director.
		/// </summary>
		public static Matrix Matrix => new(SHVDN.NativeMemory.ReadMatrix(MemoryAddress + 0x1F0));

		/// <summary>
		/// Gets the position of the <see cref="GameplayCamera"/> director.
		/// </summary>
		public static Vector3 Position => Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_COORD);

		/// <summary>
		/// Gets the rotation of the <see cref="GameplayCamera"/> director.
		/// </summary>
		/// <value>
		/// The yaw, pitch and roll rotations measured in degrees.
		/// </value>
		public static Vector3 Rotation => Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT, 2);

		/// <summary>
		/// Gets the direction of the <see cref="GameplayCamera"/> director is pointing in.
		/// </summary>
		public static Vector3 Direction => ForwardVector;

		/// <summary>
		/// Gets the up vector of the <see cref="GameplayCamera"/> director.
		/// </summary>
		public static Vector3 UpVector => new(SHVDN.NativeMemory.ReadVector3(MemoryAddress + 0x210));

		/// <summary>
		/// Gets the right vector of the <see cref="GameplayCamera"/> director.
		/// </summary>
		public static Vector3 RightVector => new(SHVDN.NativeMemory.ReadVector3(MemoryAddress + 0x1F0));

		/// <summary>
		/// Gets the forward vector of the <see cref="GameplayCamera"/> director, see also <seealso cref="Direction"/>.
		/// </summary>
		public static Vector3 ForwardVector => new(SHVDN.NativeMemory.ReadVector3(MemoryAddress + 0x200));

		/// <summary>
		/// Gets the position in world coordinates of an offset relative to the <see cref="GameplayCamera"/> director.
		/// </summary>
		/// <param name="offset">The offset from the <see cref="GameplayCamera"/>.</param>
		public static Vector3 GetOffsetPosition(Vector3 offset)
		{
			return Matrix.TransformPoint(offset);
		}

		/// <summary>
		/// Gets the relative offset of the <see cref="GameplayCamera"/> director from a world coordinates position.
		/// </summary>
		/// <param name="worldCoords">The world coordinates.</param>
		public static Vector3 GetPositionOffset(Vector3 worldCoords)
		{
			return Matrix.InverseTransformPoint(worldCoords);
		}

#region Third Person Camera

		public static void FollowCameraIgnoreAttachParentMovementThisUpdate()
			=> Function.Call(Hash.SET_FOLLOW_CAM_IGNORE_ATTACH_PARENT_MOVEMENT_THIS_UPDATE);

		/// <summary>
		/// Forces the active third person camera using the specified heading limits only for this update.
		/// </summary>
		/// <param name="minRelativeHeading">The minimum yaw value.</param>
		/// <param name="maxRelativeHeading">The maximum yaw value.</param>
		public static void SetThirdPersonCameraRelativeHeadingLimitsThisUpdate(float minRelativeHeading, float maxRelativeHeading)
			=> Function.Call(Hash.SET_THIRD_PERSON_CAM_RELATIVE_HEADING_LIMITS_THIS_UPDATE, minRelativeHeading, maxRelativeHeading);
		/// <summary>
		/// Forces the active third person camera using the specified pitch limits only for this update.
		/// </summary>
		/// <param name="minRelativePitch">The minimum pitch value.</param>
		/// <param name="maxRelativePitch">The maximum pitch value.</param>
		public static void SetThirdPersonCameraRelativePitchLimitsThisUpdate(float minRelativePitch, float maxRelativePitch)
			=> Function.Call(Hash.SET_THIRD_PERSON_CAM_RELATIVE_PITCH_LIMITS_THIS_UPDATE, minRelativePitch, maxRelativePitch);
		/// <summary>
		/// Forces the active third person camera using the specified pitch limits only for this update.
		/// </summary>
		/// <param name="minDistance">The minimum distance in meters.</param>
		/// <param name="maxDistance">The maximum distance in meters.</param>
		public static void SetThirdPersonCameraOrbitDistanceLimitsThisUpdate(float minDistance, float maxDistance)
			=> Function.Call(Hash.SET_THIRD_PERSON_CAM_ORBIT_DISTANCE_LIMITS_THIS_UPDATE, minDistance, maxDistance);

#endregion

		/// <summary>
		/// Gets the <see cref="GameplayCamera"/>'s pitch relative to the target entity (ped or vehicle) in degrees.
		/// </summary>
		public static float RelativePitch
		{
			get => Function.Call<float>(Hash.GET_GAMEPLAY_CAM_RELATIVE_PITCH);
			set => Function.Call(Hash.SET_GAMEPLAY_CAM_RELATIVE_PITCH, value, 1f);
		}

		/// <summary>
		/// Sets the gameplay camera's pitch relative to the target entity (<see cref="Ped"/> or <see cref="Vehicle"/>).
		/// </summary>
		/// <param name="pitch">The relative pitch to set in degrees.</param>
		/// <param name="smoothRate">
		/// The rate at which the relative pitch should be attained. 1.0f is instant, 0.0f is infinite.
		/// </param>
		public static void SetRelativePitch(float pitch, float smoothRate)
			=> Function.Call(Hash.SET_GAMEPLAY_CAM_RELATIVE_PITCH, pitch, smoothRate);

		/// <summary>
		/// Gets the <see cref="GameplayCamera"/>'s heading relative to the target entity (ped or vehicle) in degrees.
		/// </summary>
		public static float RelativeHeading
		{
			get => Function.Call<float>(Hash.GET_GAMEPLAY_CAM_RELATIVE_HEADING);
			set => Function.Call(Hash.SET_GAMEPLAY_CAM_RELATIVE_HEADING, value);
		}

		/// <summary>
		/// Sets the gameplay camera's pitch relative to the target entity (<see cref="Ped"/> or <see cref="Vehicle"/>).
		/// </summary>
		/// <param name="heading">The relative heading to set in degrees.</param>
		/// <param name="pitch">The relative pitch to set in degrees.</param>
		/// <param name="smoothRate">
		/// The rate at which the relative pitch should be attained. 1.0f is instant, 0.0f is infinite.
		/// </param>
		public static void ForceRelativeHeadingAndPitch(float heading, float pitch, float smoothRate)
			=> Function.Call(Hash.SET_GAMEPLAY_CAM_RELATIVE_PITCH, heading, pitch, smoothRate);

		/// <summary>
		/// Gets the value that indicates a follow-ped camera is active.
		/// </summary>
		public static bool IsFollowPedCameraActive => Function.Call<bool>(Hash.IS_FOLLOW_PED_CAM_ACTIVE);

		/// <summary>
		/// Gets the global view mode used by all follow-ped cameras.
		/// </summary>
		public static CameraViewMode FollowPedCameraViewMode
		{
			get => Function.Call<CameraViewMode>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE);
			set => Function.Call(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, (int)value);
		}

		/// <summary>
		/// Gets the value that indicates a follow-vehicle camera is active.
		/// </summary>
		public static bool IsFollowVehicleCameraActive => Function.Call<bool>(Hash.IS_FOLLOW_VEHICLE_CAM_ACTIVE);

		/// <summary>
		/// Gets or sets the view mode used by the follow-vehicle and vehicle-aim cameras associated with classes of vehicles
		/// that are not handled specially, such as cars.
		/// Use <see cref="GetCameraViewModeForContext"/> or <see cref="SetCameraViewModeForContext"/>
		/// to query the view mode applied for other classes of vehicle.
		/// </summary>
		public static CameraViewMode FollowVehicleCameraViewMode
		{
			get => Function.Call<CameraViewMode>(Hash.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE);
			set => Function.Call(Hash.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE, (int)value);
		}

		/// <summary>
		///  Gets the camera view mode for the specified context.
		/// </summary>
		public static CameraViewMode GetCameraViewModeForContext(CameraViewModeContext context)
			=> Function.Call<CameraViewMode>(Hash.GET_CAM_VIEW_MODE_FOR_CONTEXT, (int)context);
		/// <summary>
		///	Sets the camera view mode for the specified context.
		/// </summary>
		public static CameraViewMode SetCameraViewModeForContext(CameraViewModeContext context, CameraViewMode viewMode)
			=> Function.Call<CameraViewMode>(Hash.SET_CAM_VIEW_MODE_FOR_CONTEXT, (int)context, (int)viewMode);

		/// <summary>
		/// Returns the view mode context for the active gameplay camera.
		/// </summary>
		public static CameraViewModeContext ActiveViewModeContext
			=> Function.Call<CameraViewModeContext>(Hash.GET_CAM_ACTIVE_VIEW_MODE_CONTEXT);

		/// <summary>
		/// Gets the first-person ped aim zoom factor associated with equipped sniper scoped weapon,
		/// or the mobile phone camera, if active.
		/// </summary>
		public static float Zoom => Function.Call<float>(Hash.GET_FIRST_PERSON_AIM_CAM_ZOOM_FACTOR);

		/// <summary>
		/// Gets the field of view of the <see cref="GameplayCamera"/>.
		/// </summary>
		public static float FieldOfView => Function.Call<float>(Hash.GET_GAMEPLAY_CAM_FOV);

		/// <summary>
		/// Gets a value indicating whether the gameplay director is the dominant rendering director.
		/// The gameplay director is responsible for the follow and aim cameras.
		/// </summary>
		/// <value>
		/// <see langword="true" /> if the <see cref="GameplayCamera"/> is rendering; otherwise, <see langword="false" />.
		/// </value>
		public static bool IsRendering => Function.Call<bool>(Hash.IS_GAMEPLAY_CAM_RENDERING);

		/// <summary>
		/// Gets a value indicating whether an aim camera is active.
		/// </summary>
		/// <value>
		/// <see langword="true" /> if an aim camera is rendering; otherwise, <see langword="false" />.
		/// </value>
		public static bool IsAimCamActive => Function.Call<bool>(Hash.IS_AIM_CAM_ACTIVE);

		/// <summary>
		/// Gets a value indicating whether a first person ped aim camera is active,
		/// which can be activated by using a sniper rifle scope or mobile phone camera (strictly checks if the gameplay
		/// director use a <c>camFirstPersonPedAimCamera</c> instance).
		/// Do not confuse with a first person shooter camera (<c>camFirstPersonShooterCamera</c>),
		/// which can be activated by switching the camera mode.
		/// </summary>
		/// <value>
		/// <see langword="true" /> if a first person ped aim camera is active; otherwise, <see langword="false" />.
		/// </value>
		public static bool IsFirstPersonAimCamActive => Function.Call<bool>(Hash.IS_FIRST_PERSON_AIM_CAM_ACTIVE);

		/// <summary>
		/// Gets a value indicating whether the active gameplay camera is looking behind.
		/// </summary>
		/// <value>
		/// <see langword="true" /> if the <see cref="GameplayCamera"/> director is looking behind; otherwise, <see langword="false" />.
		/// </value>
		public static bool IsLookingBehind => Function.Call<bool>(Hash.IS_GAMEPLAY_CAM_LOOKING_BEHIND);

		/// <summary>
		/// Shakes the <see cref="GameplayCamera"/>.
		/// </summary>
		/// <param name="shakeType">Type of the shake to apply.</param>
		/// <param name="amplitude">The amplitude of the shaking.</param>
		public static void Shake(CameraShake shakeType, float amplitude)
		{
			Function.Call(Hash.SHAKE_GAMEPLAY_CAM, Camera.shakeNames[(int)shakeType], amplitude);
		}

		/// <summary>
		/// Stops shaking the <see cref="GameplayCamera"/>.
		/// </summary>
		public static void StopShaking()
		{
			Function.Call(Hash.STOP_GAMEPLAY_CAM_SHAKING, true);
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="GameplayCamera"/> is shaking.
		/// </summary>
		/// <value>
		/// <see langword="true" /> if the <see cref="GameplayCamera"/> is shaking; otherwise, <see langword="false" />.
		/// </value>
		public static bool IsShaking => Function.Call<bool>(Hash.IS_GAMEPLAY_CAM_SHAKING);

		/// <summary>
		/// Sets the overall amplitude scaling for an active gameplay camera shake.
		/// </summary>
		public static float ShakeAmplitude
		{
			set => Function.Call(Hash.SET_GAMEPLAY_CAM_SHAKE_AMPLITUDE, value);
		}
	}
}
