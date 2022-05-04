﻿// Marking as auto-generated to prevent StyleCop including this file - Taken from Microsoft.Extensions.Hosting
// <auto-generated />

namespace Uno.Extensions.Hosting;

public class HostBuilder : IHostBuilder
{
	private List<Action<IConfigurationBuilder>> _configureHostConfigActions = new List<Action<IConfigurationBuilder>>();
	private List<Action<HostBuilderContext, IConfigurationBuilder>> _configureAppConfigActions = new List<Action<HostBuilderContext, IConfigurationBuilder>>();
	private List<Action<HostBuilderContext, IServiceCollection>> _configureServicesActions = new List<Action<HostBuilderContext, IServiceCollection>>();
	private List<IConfigureContainerAdapter> _configureContainerActions = new List<IConfigureContainerAdapter>();
	private IServiceFactoryAdapter _serviceProviderFactory = new ServiceFactoryAdapter<IServiceCollection>(new DefaultServiceProviderFactory());
	private bool _hostBuilt;
	private IConfiguration? _hostConfiguration;
	private IConfiguration? _appConfiguration;
	private HostBuilderContext? _hostBuilderContext;
	private HostingEnvironment? _hostingEnvironment;
	private IServiceProvider? _appServices;
	private PhysicalFileProvider? _defaultProvider;

	/// <summary>
	/// A central location for sharing state between components during the host building process.
	/// </summary>
	public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

	/// <summary>
	/// Set up the configuration for the builder itself. This will be used to initialize the <see cref="IHostEnvironment"/>
	/// for use later in the build process. This can be called multiple times and the results will be additive.
	/// </summary>
	/// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder"/> that will be used
	/// to construct the <see cref="IConfiguration"/> for the host.</param>
	/// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
	public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
	{
		_configureHostConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
		return this;
	}

	/// <summary>
	/// Sets up the configuration for the remainder of the build process and application. This can be called multiple times and
	/// the results will be additive. The results will be available at <see cref="HostBuilderContext.Configuration"/> for
	/// subsequent operations, as well as in <see cref="IHost.Services"/>.
	/// </summary>
	/// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder"/> that will be used
	/// to construct the <see cref="IConfiguration"/> for the host.</param>
	/// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
	public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
	{
		_configureAppConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
		return this;
	}

	/// <summary>
	/// Adds services to the container. This can be called multiple times and the results will be additive.
	/// </summary>
	/// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder"/> that will be used
	/// to construct the <see cref="IConfiguration"/> for the host.</param>
	/// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
	public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
	{
		_configureServicesActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
		return this;
	}

	/// <summary>
	/// Overrides the factory used to create the service provider.
	/// </summary>
	/// <typeparam name="TContainerBuilder">The type of the builder to create.</typeparam>
	/// <param name="factory">A factory used for creating service providers.</param>
	/// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
	public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
	{
		_serviceProviderFactory = new ServiceFactoryAdapter<TContainerBuilder>(factory ?? throw new ArgumentNullException(nameof(factory)));
		return this;
	}

	/// <summary>
	/// Overrides the factory used to create the service provider.
	/// </summary>
	/// <param name="factory">A factory used for creating service providers.</param>
	/// <typeparam name="TContainerBuilder">The type of the builder to create.</typeparam>
	/// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
	public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) where TContainerBuilder : notnull
	{
		_serviceProviderFactory = new ServiceFactoryAdapter<TContainerBuilder>(() => _hostBuilderContext, factory ?? throw new ArgumentNullException(nameof(factory)));
		return this;
	}

	/// <summary>
	/// Enables configuring the instantiated dependency container. This can be called multiple times and
	/// the results will be additive.
	/// </summary>
	/// <typeparam name="TContainerBuilder">The type of the builder to create.</typeparam>
	/// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder"/> that will be used
	/// to construct the <see cref="IConfiguration"/> for the host.</param>
	/// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
	public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
	{
		_configureContainerActions.Add(new ConfigureContainerAdapter<TContainerBuilder>(configureDelegate
			?? throw new ArgumentNullException(nameof(configureDelegate))));
		return this;
	}

	/// <summary>
	/// Run the given actions to initialize the host. This can only be called once.
	/// </summary>
	/// <returns>An initialized <see cref="IHost"/></returns>
	public IHost? Build()
	{
		if (_hostBuilt)
		{
			throw new InvalidOperationException("BuildCalled"); // SR.BuildCalled);
		}
		_hostBuilt = true;


		BuildHostConfiguration();
		CreateHostingEnvironment();
		CreateHostBuilderContext();
		BuildAppConfiguration();
		CreateServiceProvider();

		var host = _appServices?.GetRequiredService<IHost>();
		return host;
	}

	private void BuildHostConfiguration()
	{
		IConfigurationBuilder configBuilder = new ConfigurationBuilder()
			.AddInMemoryCollection(); // Make sure there's some default storage since there are no default providers

		foreach (Action<IConfigurationBuilder> buildAction in _configureHostConfigActions)
		{
			buildAction(configBuilder);
		}
		_hostConfiguration = configBuilder.Build();
	}

	private void CreateHostingEnvironment()
	{
		_hostingEnvironment = new HostingEnvironment()
		{
			ApplicationName = _hostConfiguration?[HostDefaults.ApplicationKey],
			EnvironmentName = _hostConfiguration?[HostDefaults.EnvironmentKey] ?? Environments.Production,
			ContentRootPath = ResolveContentRootPath(_hostConfiguration?[HostDefaults.ContentRootKey], AppContext.BaseDirectory),
		};

		if (string.IsNullOrEmpty(_hostingEnvironment.ApplicationName))
		{
			_hostingEnvironment.ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name;
		}

		_hostingEnvironment.ContentRootFileProvider = _defaultProvider = new PhysicalFileProvider(_hostingEnvironment.ContentRootPath);
	}

	private string? ResolveContentRootPath(string? contentRootPath, string? basePath)
	{
		if (string.IsNullOrEmpty(contentRootPath))
		{
			return basePath;
		}
		if (Path.IsPathRooted(contentRootPath))
		{
			return contentRootPath;
		}
		return Path.Combine(Path.GetFullPath(basePath ?? string.Empty), contentRootPath);
	}

	private void CreateHostBuilderContext()
	{
		_hostBuilderContext = new HostBuilderContext(Properties)
		{
			HostingEnvironment = _hostingEnvironment,
			Configuration = _hostConfiguration
		};
	}

	private void BuildAppConfiguration()
	{
		IConfigurationBuilder configBuilder = new ConfigurationBuilder()
			.SetBasePath(_hostingEnvironment?.ContentRootPath)
			.AddConfiguration(_hostConfiguration, shouldDisposeConfiguration: true);

		if (_hostBuilderContext is not null)
		{
			foreach (Action<HostBuilderContext, IConfigurationBuilder> buildAction in _configureAppConfigActions)
			{
				buildAction(_hostBuilderContext, configBuilder);
			}
			_appConfiguration = configBuilder.Build();
			_hostBuilderContext.Configuration = _appConfiguration;
		}
	}

	private void CreateServiceProvider()
	{
		var services = new ServiceCollection();
		if (_hostingEnvironment is not null)
		{
			services.AddSingleton<IHostEnvironment>(_hostingEnvironment);
		}
		if (_hostBuilderContext is not null)
		{
			services.AddSingleton(_hostBuilderContext);
		}
		// register configuration as factory to make it dispose with the service provider
		services.AddSingleton(_ => _appConfiguration!);
		services.AddSingleton<IHostApplicationLifetime, ApplicationLifetime>();
		services.AddSingleton((Func<IServiceProvider, IHost>)(_ =>
		{
			return (IHost)new Internal.Host(_appServices,
				(IHostEnvironment?)_hostingEnvironment,
				_defaultProvider,
				_appServices?.GetRequiredService<IHostApplicationLifetime>(),
				(ILogger<Internal.Host>?)(_appServices?.GetRequiredService<ILogger<Internal.Host>>()),
				_appServices?.GetService<IHostLifetime>(),
				_appServices?.GetService<IOptions<HostOptions>>());
		}));
		//services.AddOptions().Configure<HostOptions>(options => { options.Initialize(_hostConfiguration); });
		services.AddLogging();

		foreach (Action<HostBuilderContext, IServiceCollection> configureServicesAction in _configureServicesActions)
		{
			configureServicesAction(_hostBuilderContext!, services);
		}

		object containerBuilder = _serviceProviderFactory.CreateBuilder(services);

		foreach (IConfigureContainerAdapter containerAction in _configureContainerActions)
		{
			containerAction.ConfigureContainer(_hostBuilderContext, containerBuilder);
		}

		_appServices = _serviceProviderFactory.CreateServiceProvider(containerBuilder);

		if (_appServices == null)
		{
			throw new InvalidOperationException("NullIServiceProvider");// SR.NullIServiceProvider);
		}

		//// resolve configuration explicitly once to mark it as resolved within the
		//// service provider, ensuring it will be properly disposed with the provider
		//_ = _appServices.GetService<IConfiguration>();
	}
}