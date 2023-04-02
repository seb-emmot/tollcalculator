using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using TollCalculations;
using TollCalculations.Vehicles;

namespace TollCalculations.Tests;

[TestClass]
public class TollCalculatorTests
{
	private TollCalculator _calc;
	private List<VehicleTypes> _tollFree;

	[TestInitialize]
	public void Initialize()
	{
		_tollFree = new List<VehicleTypes>() {
			VehicleTypes.Motorbike,
			VehicleTypes.Tractor,
			VehicleTypes.Emergency,
			VehicleTypes.Diplomat,
			VehicleTypes.Foreign,
			VehicleTypes.Military
		};

		var hProv = new SweHolidayProvider();
		_calc = new TollCalculator(_tollFree, hProv);
	}

	[TestMethod]
	public void GetTollFee_IsHoliday_ReturnZero()
	{
		var vehicle = new Car();
		var passages = new TollPassages(
			new DateTime(2013, 01, 01),
			new List<TimeSpan>()
			{
				new TimeSpan(6, 0, 0), // 8
			}
		);

		var actFee = _calc.GetTollFee(vehicle, passages);
		var expFee = 0;

		Assert.AreEqual(expFee, actFee);
	}

	[TestMethod]
	public void GetTollFee_VehicleIsNonTollable_ReturnZero()
	{
		foreach (var typ in _tollFree)
		{
			var vehicle = new Vehicle(typ);
			var passages = new TollPassages(
				new DateTime(2013, 01, 02),
				new List<TimeSpan>()
				{
					new TimeSpan(6, 0, 0), // 8
				}
			);

			var actFee = _calc.GetTollFee(vehicle, passages);
			var expFee = 0;

			Assert.AreEqual(expFee, actFee);
		}
	}

	[TestMethod]
	public void GetTollFee_MultiplePasses_ReturnsFee()
	{
		var vehicle = new Car();
		var passages = new TollPassages(
			new DateTime(2013, 01, 02),
			new List<TimeSpan>()
			{
				new TimeSpan(0, 0, 0), // 0
				new TimeSpan(6, 0, 0), // 8
				new TimeSpan(7, 30, 0), // 18
				new TimeSpan(9, 0, 0), // 8
				new TimeSpan(10, 0, 0), // 8
				new TimeSpan(19, 0, 0), // 0
				new TimeSpan(24, 0, 0) // 0
			}
		);

		var actFee = _calc.GetTollFee(vehicle, passages);
		var expFee = 42;

		Assert.AreEqual(expFee, actFee);
	}

	[TestMethod]
	public void GetTollFee_GracePeriodHasLowerFee_DiscardsFee()
	{
		var vehicle = new Car();
		var passages = new TollPassages(
			new DateTime(2013, 01, 02),
			new List<TimeSpan>()
			{
				new TimeSpan(7, 30, 0), // 18
				new TimeSpan(8, 00, 0), // should be discarded
				new TimeSpan(8, 45, 0), // 8
			}
		);

		var actFee = _calc.GetTollFee(vehicle, passages);
		var expFee = 26;

		Assert.AreEqual(expFee, actFee);
	}

	[TestMethod]
	public void GetTollFee_GracePeriodHasHigherFee_IncreasesFee()
	{
		var vehicle = new Car();
		var passages = new TollPassages(
			new DateTime(2013, 01, 02),
			new List<TimeSpan>()
			{
				new TimeSpan(6, 50, 0), // 13
				new TimeSpan(7, 30, 0), // 18 - should increase
			}
		);

		var actFee = _calc.GetTollFee(vehicle, passages);
		var expFee = 18;

		Assert.AreEqual(expFee, actFee);
	}

	[TestMethod]
	public void GetTollFee_SinglePass_ReturnsFee()
	{
		var vehicle = new Car();
		var passages = new TollPassages(
			new DateTime(2013, 01, 02),
			new List<TimeSpan>()
			{
				new TimeSpan(7, 30, 0), // 18
			}
		);

		var actFee = _calc.GetTollFee(vehicle, passages);
		var expFee = 18;

		Assert.AreEqual(expFee, actFee);
	}

	[DataRow(0)]
	[DataRow(24)]
	[TestMethod]
	public void GetTollFee_SinglePassMidnight_ReturnsSame(int hour)
	{
		var vehicle = new Car();
		var passages = new TollPassages(
			new DateTime(2013, 01, 02),
			new List<TimeSpan>()
			{
				new TimeSpan(hour, 0, 0), // 0
			}
		);

		var actFee = _calc.GetTollFee(vehicle, passages);
		var expFee = 0;

		Assert.AreEqual(expFee, actFee);
	}
}