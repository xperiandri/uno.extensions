﻿namespace Uno.Extensions.Navigation;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
public record ViewMap(
	Type? View = null,
	Func<Type?>? DynamicView = null,
	Type? ViewModel = null,
	DataMap? Data = null,
	Type? ResultData = null,
	object? ViewAttributes = null
)
{
	public virtual void RegisterTypes(IServiceCollection services)
	{
		if (ViewModel is not null)
		{
			services.AddTransient(ViewModel);
		}

		Data?.RegisterTypes(services);
	}
}

public record ViewMap<TView>(
	Type? ViewModel = null,
	DataMap? Data = null,
	Type? ResultData = null,
	object? ViewAttributes = null
) : ViewMap(View: typeof(TView), ViewModel: ViewModel, Data: Data, ResultData: ResultData, ViewAttributes: ViewAttributes)
{
}

public record ViewMap<TView, TViewModel>(
	DataMap? Data = null,
	Type? ResultData = null,
	object? ViewAttributes = null
) : ViewMap(View: typeof(TView), ViewModel: typeof(TViewModel), Data: Data, ResultData: ResultData, ViewAttributes: ViewAttributes)
{
}

public record DialogAction(string? Label = "", Action? Action = null, object? Id = null) { }

#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

