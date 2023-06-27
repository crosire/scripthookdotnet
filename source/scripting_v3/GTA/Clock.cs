//
// Copyright (C) 2015 kagikn & contributors
// License: https://github.com/scripthookvdotnet/scripthookvdotnet#license
//

using GTA.Native;
using System;

namespace GTA
{
	/// <summary>
	/// Represents the global game clock.
	/// </summary>
	public static class Clock
	{
		/// <summary>
		/// Gets or sets a value that indicates whether the in-game clock is paused.
		/// </summary>
		public static bool IsPaused
		{
			get => SHVDN.NativeMemory.IsClockPaused;
			set => Function.Call(Hash.PAUSE_CLOCK, value);
		}

		/// <summary>
		/// Gets or sets the last time the clock is ticked.
		/// You can use this value to calculate pseudo milliseconds along with <see cref="Game.GameTime"/>
		/// and <see cref="MillisecondsPerGameMinute"/> when the clock is not paused.
		/// You can also set a value to this property to shift the clock minute.
		/// </summary>
		/// <remarks>
		/// If <see cref="IsPaused"/> is set to <see langword="true"/>, this value will be updated to <see cref="Game.GameTime"/> every frame.
		/// </remarks>
		public static int LastTimeTicked
		{
			get => SHVDN.NativeMemory.LastTimeClockTicked;
			set => SHVDN.NativeMemory.LastTimeClockTicked = value;
		}

		/// <summary>
		/// Gets or sets the day of month starting from 1 to 31.
		/// The max value is guaranteed to be 31 regardless of the month.
		/// </summary>
		public static int Day
		{
			get => Function.Call<int>(Hash.GET_CLOCK_DAY_OF_MONTH);
			set => SetDateZeroBasedMonth(value, MonthZero, Year);
		}

		/// <summary>
		/// Gets or sets the day of month starting from 1.
		/// </summary>
		/// <remarks>
		/// When you do not plan to use this value to draw on the screen,
		/// consider using <see cref="MonthZero"/> since the game internally uses the zero-based month representation.
		/// </remarks>
		public static int Month
		{
			get => MonthZero + 1;
			set => MonthZero = value - 1;
		}

		/// <summary>
		/// Gets or sets the day of month starting from 0.
		/// The representation is the same as the game uses for the month.
		/// </summary>
		public static int MonthZero
		{
			get => Function.Call<int>(Hash.GET_CLOCK_MONTH);
			set => SetDateZeroBasedMonth(Day, value, Year);
		}
		/// <summary>
		/// Gets or sets the year number from 1 to 9999.
		/// </summary>
		/// <value>
		/// The current year number.
		/// </value>
		public static int Year
		{
			get => Function.Call<int>(Hash.GET_CLOCK_YEAR);
			set => SetDateZeroBasedMonth(Day, MonthZero, value);
		}

		/// <param name="day">
		/// The day number from 1 to 31.
		/// The max value is the same regardless of <paramref name="month"/>.
		/// </param>
		/// <param name="month">
		/// The month number from 1 to 12.
		/// </param>
		/// <param name="year">
		/// The year number from 1 to 9999.
		/// </param>
		/// <inheritdoc cref="SetDateZeroBasedMonth(int, int, int)"/>
		public static void SetDate(int day, int month, int year) => SetDateZeroBasedMonth(day, month - 1, year);

		/// <summary>
		/// Sets the current date in the GTA world.
		/// </summary>
		/// <param name="day">
		/// The day number from 1 to 31.
		/// The max value is the same regardless of <paramref name="month"/>.
		/// </param>
		/// <param name="month">
		/// The month number from 0 to 11.
		/// </param>
		/// <param name="year">
		/// The year number from 1 to 9999.
		/// </param>
		/// <remarks>
		/// If you set to a date that the game cannot handle properly, the <see cref="Day"/> will be set to 1985.
		/// </remarks>
		public static void SetDateZeroBasedMonth(int day, int month, int year) => Function.Call(Hash.SET_CLOCK_DATE, day, month, year);

		/// <summary>
		/// Gets the day of the week.
		/// </summary>
		/// <remarks>
		/// Returns the cached value, not the value calculated by <see cref="Day"/>, <see cref="Month"/>, and <see cref="Year"/>.
		/// If some of them is modified without updating the cached value for the day of week by direct memory editing,
		/// this property will return an incorrect value.
		/// </remarks>
		public static DayOfWeek DayOfWeek => Function.Call<DayOfWeek>(Hash.GET_CLOCK_DAY_OF_WEEK);

		/// <summary>
		/// Gets or sets the hour number from 0 to 23.
		/// </summary>
		/// <value>
		/// The current hour number.
		/// </value>
		public static int Hour
		{
			get => Function.Call<int>(Hash.GET_CLOCK_HOURS);
			set => Function.Call(Hash.SET_CLOCK_TIME, value, Minute, Second);
		}
		/// <summary>
		/// Gets or sets the minute number from 0 to 59.
		/// </summary>
		/// <value>
		/// The current minute number.
		/// </value>
		public static int Minute
		{
			get => Function.Call<int>(Hash.GET_CLOCK_MINUTES);
			set => Function.Call(Hash.SET_CLOCK_TIME, Hour, value, Second);
		}
		/// <summary>
		/// Gets or sets the second number from 0 to 59.
		/// </summary>
		/// <value>
		/// The current second number.
		/// </value>
		public static int Second
		{
			get => Function.Call<int>(Hash.GET_CLOCK_SECONDS);
			set => Function.Call(Hash.SET_CLOCK_TIME, Hour, Minute, value);
		}

		/// <summary>
		/// Gets or sets the current time of day in the GTA world.
		/// </summary>
		/// <value>
		/// The current time of day.
		/// </value>
		/// <remarks>
		/// The resolution of the value is 1 second.
		/// </remarks>
		public static TimeSpan TimeOfDay
		{
			get => new (Hour, Minute, Second);
			set => Function.Call(Hash.SET_CLOCK_TIME, value.Hours, value.Minutes, value.Seconds);
		}

		/// <summary>
		/// Gets or sets how many milliseconds in the real world one game minute takes.
		/// </summary>
		/// <value>
		/// The milliseconds one game minute takes in the real world.
		/// </value>
		public static int MillisecondsPerGameMinute
		{
			get => Function.Call<int>(Hash.GET_MILLISECONDS_PER_GAME_MINUTE);
			set => SHVDN.NativeMemory.MillisecondsPerGameMinute = value;
		}

		// these 2 arrays was taken from the exe (embedded as 4-byte arrays)
		private static int[] s_firstDaysOfWeekForNonLeapYear = new int[12] { 0, 3, 3, 6, 1, 4, 6, 2, 5, 0, 3, 5 };
		private static int[] s_firstDaysOfWeekForLeapYear = new int[12] { 6, 2, 3, 6, 1, 4, 6, 2, 5, 0, 3, 5 };

		/// <summary>
		/// Returns an indication whether the specified year is a leap year.
		/// Calculates in the same way as the game does.
		/// </summary>
		public static bool IsLeapYear(int year)
		{
			return (year % 4) == 0 && year != 100 * (year / 100) || year == 400 * (year / 400);
		}

		/// <param name="day">
		/// The day number from 1 to 31.
		/// The max value is the same regardless of <paramref name="month"/>.
		/// </param>
		/// <param name="month">
		/// The month number from 1 to 12.
		/// </param>
		/// <param name="year">
		/// The year number from 1 to 9999.
		/// </param>
		/// <inheritdoc cref="GetDayOfWeekZeroBasedMonth(int, int, int)"/>
		public static DayOfWeek GetDayOfWeek(int day, int month, int year) => GetDayOfWeekZeroBasedMonth(day, month - 1, year);

		/// <summary>
		/// Gets the day of the week.
		/// Calculates in the same way as the game does.
		/// </summary>
		/// <param name="day">
		/// The day number from 1 to 31.
		/// The max value is the same regardless of <paramref name="month"/>.
		/// </param>
		/// <param name="month">
		/// The month number from 0 to 11.
		/// </param>
		/// <param name="year">
		/// The year number from 1 to 9999.
		/// </param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Throws when one of the arguments is out of the range.
		/// </exception>
		public static DayOfWeek GetDayOfWeekZeroBasedMonth(int day, int month, int year)
		{
			if (day < 1 || day > 31)
			{
				throw new ArgumentOutOfRangeException(nameof(day));
			}
			if (month < 0 || month > 11)
			{
				throw new ArgumentOutOfRangeException(nameof(month));
			}
			if (year < 1 || year > 9999)
			{
				throw new ArgumentOutOfRangeException(nameof(year));
			}

			int century = year % 100;
			int firstDayOfWeek = s_firstDaysOfWeekForNonLeapYear[month];
			int unk1 = 2 * (3 - (year - century) / 100 % 4);
			int unk2 = century + century / 4;
			if (IsLeapYear(year))
			{
				firstDayOfWeek = s_firstDaysOfWeekForLeapYear[month];
			}

			return (DayOfWeek)((day + unk1 + firstDayOfWeek + unk2) % 7);
		}
	}
}
