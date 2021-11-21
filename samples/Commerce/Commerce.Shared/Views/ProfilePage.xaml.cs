﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Toolkit.UI.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Commerce
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ProfilePage : Page
	{
		public ProfilePage()
		{
			this.InitializeComponent();

			this.Loaded += (s, e) =>
			{
				// Initialize the toggle to the current theme.
				darkModeToggle.IsEnabled = false;
				darkModeToggle.IsOn = SystemThemeHelper.IsAppInDarkMode();
				darkModeToggle.IsEnabled = true;
			};
		}

		private void ToggleDarkMode()
		{
			if (darkModeToggle.IsEnabled)
			{
				SystemThemeHelper.SetApplicationTheme(darkModeToggle.IsOn);
			}
		}
	}
}