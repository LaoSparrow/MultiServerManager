using System;
using Microsoft.Extensions.DependencyInjection;
using MultiServerManager.Common;

namespace MultiServerManager.Utils.Extensions;

// ReSharper disable once InconsistentNaming
public static class IServiceCollectionExt
{
    public static IServiceCollection AddViewModel(this IServiceCollection services, ViewModelBase vm) =>
        services.AddSingleton(vm);

    public static IServiceCollection AddViewModel(this IServiceCollection services,
        Func<IServiceProvider, ViewModelBase> implementationFactory) =>
        services.AddSingleton(implementationFactory);
}