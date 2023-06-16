﻿using Microsoft.Maui.Devices;
using Uno.Extensions;

namespace MauiEmbedding.Presentation;
partial class MauiEssentialsViewModel : ObservableObject
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BatteryLevelText))]
	double _batteryLevel;

	public string BatteryLevelText => $"Battery level: {BatteryLevel * 100}%";

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(DisplayInfo))]
	(double width, double height) _displaySize;

	public string DisplayInfo => $"Display info: {DisplaySize.width}x{DisplaySize.height}";

	public MauiEssentialsViewModel(IDispatcher dispatcher)
	{
		BatteryLevel = Battery.ChargeLevel;
		DisplaySize = (DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);

		_ = dispatcher.ExecuteAsync(() =>
		{
			Battery.BatteryInfoChanged += OnBatteryBatteryInfoChanged;
			DeviceDisplay.MainDisplayInfoChanged += OnDeviceDisplayChanged;
		});
	}

	private void OnDeviceDisplayChanged(object? sender, DisplayInfoChangedEventArgs e) =>
		DisplaySize = (e.DisplayInfo.Width, e.DisplayInfo.Height);
	private void OnBatteryBatteryInfoChanged(object? sender, BatteryInfoChangedEventArgs e) =>
		BatteryLevel = e.ChargeLevel;
}
