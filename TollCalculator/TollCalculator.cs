using System;
using System.Collections.Generic;
using System.Linq;
using TollCalculations.Vehicles;

namespace TollCalculations;

/// <summary>
/// Calculate vehicle tolls.
/// </summary>
public class TollCalculator
{
	private readonly IEnumerable<VehicleTypes> _tollFreeTypes;
	private readonly IHolidayProvider _holidayProvider;
	private readonly List<FeeInterval> _feeIntervals;

	public TollCalculator(
        IEnumerable<VehicleTypes> tollFreeTypes,
        IHolidayProvider holidayProvider)
	{
        _tollFreeTypes = tollFreeTypes;
		_holidayProvider = holidayProvider;

		_feeIntervals = new List<FeeInterval>()
		{
			new FeeInterval(0, 0, 5, 59, 0),
			new FeeInterval(6, 0, 6, 29, 8),
			new FeeInterval(6, 30, 6, 59, 13),
			new FeeInterval(7, 0, 7, 59, 18),
			new FeeInterval(8, 0, 8, 29, 13),
			new FeeInterval(8, 30, 14, 59, 8),
			new FeeInterval(15, 0, 15, 29, 13),
			new FeeInterval(15, 30, 16, 59, 18),
			new FeeInterval(17, 0, 17, 59, 13),
			new FeeInterval(18, 0, 18, 29, 8),
			new FeeInterval(18, 30, 24, 0, 0)
		};
	}

	/// <summary>
	/// Calculate the total toll fee for one day
	/// </summary>
	/// <param name="vehicle">the vehicle.</param>
	/// <param name="day">record of all passes on one day.</param>
	/// <returns>the total toll fee for that day.</returns>
	/// <exception cref="ArgumentNullException"></exception>
	public int GetTollFee(IVehicle vehicle, TollPassages day)
    {
        if (vehicle == null)
        {
            throw new ArgumentNullException(nameof(vehicle));
        }

		if (IsTollFreeDate(day.Day) || IsTollFreeVehicle(vehicle) || !day.Passages.Any())
        {
            return 0;
        }

        var passages = new Queue<TimeSpan>(day.Passages.OrderBy(x => x));

        int accumulatedFee = 0;

		var intervalStart = passages.Dequeue();
        int maxFee = GetFeeAtTime(day.Day + intervalStart);

		while (passages.Count != 0)
        {
            var next = passages.Dequeue();
			var fee = GetFeeAtTime(day.Day + next);
            if (next - intervalStart < TimeSpan.FromHours(1))
            {
                // Still in the same interval, check if maxFee needs to be updated.
                maxFee = fee > maxFee ? fee : maxFee;
			}
            else
            {
                // next element in q is a new interval.
                accumulatedFee += maxFee;
                intervalStart = next;
                maxFee = fee;
			}
        }
        accumulatedFee += maxFee;

        return accumulatedFee;
	}

    private int GetFeeAtTime(DateTime date)
	{
        int hour = date.Hour;
        int minute = date.Minute;

        var time = date.TimeOfDay;

        var intervals = _feeIntervals.Where(x => x.Contains(TimeOnly.FromTimeSpan(time)));

        var fee = intervals.First().Fee;
        return fee;
    }

    private bool IsTollFreeVehicle(IVehicle vehicle)
    {
        var isTollFree = _tollFreeTypes.Contains(vehicle.VehicleType);
        return isTollFree;
    }

    private bool IsTollFreeDate(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday
            || date.DayOfWeek == DayOfWeek.Sunday)
        {
            return true;
        }

        return _holidayProvider.IsHoliday(date);
    }
}
