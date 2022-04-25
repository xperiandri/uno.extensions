﻿namespace Uno.Extensions.Navigation;

public static class RouteMapExtensions
{

	internal static IEnumerable<InternalRouteMap> Flatten(this InternalRouteMap route)
	{
		if (route is null)
		{
			yield break;
		}

		yield return route;

		foreach (var subMap in route.Nested.Flatten())
		{
			yield return subMap;
		}
	}

	internal static IEnumerable<InternalRouteMap> Flatten(this IEnumerable<InternalRouteMap> routes)
	{
		if (routes is null)
		{
			yield break;
		}

		foreach (var routeMap in routes)
		{
			yield return routeMap;

			foreach (var subMap in routeMap.Nested.Flatten())
			{
				yield return subMap;
			}
		}
	}
}
