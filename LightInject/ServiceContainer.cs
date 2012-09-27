/*****************************************************************************   
   Copyright 2012 bernhard.richter@gmail.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
******************************************************************************
   LightInject version 2.0.0.1 
   https://github.com/seesharper/LightInject/wiki/Getting-started
******************************************************************************/
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:PrefixLocalCallsWithThis", Justification = "No inheritance")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Single source file deployment.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:FileMustHaveHeader", Justification = "Custom header.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "All public members are documented.")]

namespace LightInject
{
    using System;
#if NET
    using System.Collections.Concurrent;
#endif
#if SILVERLIGHT
    using System.Collections;
#endif
    using System.Collections.Generic;
#if NET    
    using System.IO;
#endif
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;    
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Specifies the lifecycle type of a registered service.
    /// </summary>
    internal enum LifeCycleType
    {
        /// <summary>
        /// Specifies that a new instance is created for each request.
        /// </summary>
        Transient,

        /// <summary>
        /// Specifies that the same instance is returned across multiple requests.
        /// </summary>
        Singleton,

        /// <summary>
        /// Specifies that the same instance is returned throughout the dependency graph.
        /// </summary>
        Request
    }

    /// <summary>
    /// Defines a set of methods used to register services into the service container.
    /// </summary>
    internal interface IServiceRegistry
    {
        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        void Register(Type serviceType, Type implementingType);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        void Register(Type serviceType, Type implementingType, LifeCycleType lifeCycle);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="serviceName">The name of the service.</param>
        void Register(Type serviceType, Type implementingType, string serviceName);

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        void Register(Type serviceType, Type implementingType, string serviceName, LifeCycleType lifeCycle);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        void Register<TService, TImplementation>() where TImplementation : TService;

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        void Register<TService, TImplementation>(LifeCycleType lifeCycle) where TImplementation : TService;

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="serviceName">The name of the service.</param>
        void Register<TService, TImplementation>(string serviceName) where TImplementation : TService;

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="serviceName">The name of the service.</param>
        /// /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        void Register<TService, TImplementation>(string serviceName, LifeCycleType lifeCycle) where TImplementation : TService;
       
        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the given <paramref name="instance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="instance">The instance returned when this service is requested.</param>
        void Register<TService>(TService instance);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the given <paramref name="instance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="instance">The instance returned when this service is requested.</param>
        /// <param name="serviceName">The name of the service.</param>
        void Register<TService>(TService instance, string serviceName);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="expression"/> that 
        /// describes the dependencies of the service. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="expression">The lambdaExpression that describes the dependencies of the service.</param>
        /// <example>
        /// The following example shows how to register a new IFoo service.
        /// <code>
        /// <![CDATA[
        /// container.Register<IFoo>(r => new FooWithDependency(r.GetInstance<IBar>()))
        /// ]]>
        /// </code>
        /// </example>
        void Register<TService>(Expression<Func<IServiceFactory, TService>> expression);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="expression"/> that 
        /// describes the dependencies of the service. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="expression">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        void Register<TService>(Expression<Func<IServiceFactory, TService>> expression, LifeCycleType lifeCycle);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="expression"/> that 
        /// describes the dependencies of the service. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="expression">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="serviceName">The name of the service.</param>        
        void Register<TService>(Expression<Func<IServiceFactory, TService>> expression, string serviceName);

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="expression"/> that 
        /// describes the dependencies of the service. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="expression">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="serviceName">The name of the service.</param>        
        /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        void Register<TService>(Expression<Func<IServiceFactory, TService>> expression, string serviceName, LifeCycleType lifeCycle);
    }

    /// <summary>
    /// Defines a set of methods used to retrieve service instances.
    /// </summary>
    internal interface IServiceFactory
    {
        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <returns>The requested service instance.</returns>
        object GetInstance(Type serviceType);

        /// <summary>
        /// Gets a named instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance.</returns>
        object GetInstance(Type serviceType, string serviceName);

        /// <summary>
        /// Gets an instance of the given <typeparamref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <returns>The requested service instance.</returns>
        TService GetInstance<TService>();

        /// <summary>
        /// Gets a named instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance.</returns>    
        TService GetInstance<TService>(string serviceName);

        /// <summary>
        /// Gets all instances of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of services to resolve.</param>
        /// <returns>A list that contains all implementations of the <paramref name="serviceType"/>.</returns>
        IEnumerable<object> GetAllInstances(Type serviceType);

        /// <summary>
        /// Gets all instances of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of services to resolve.</typeparam>
        /// <returns>A list that contains all implementations of the <typeparamref name="TService"/> type.</returns>
        IEnumerable<TService> GetAllInstances<TService>();
    }

    /// <summary>
    /// Represents a factory class that is capable of returning an object instance.
    /// </summary>    
    internal interface IFactory
    {
        /// <summary>
        /// Returns an instance of the given type indicated by the <paramref name="serviceRequest"/>. 
        /// </summary>        
        /// <param name="serviceRequest">The <see cref="ServiceRequest"/> instance that contains information about the service request.</param>
        /// <returns>An object instance corresponding to the <paramref name="serviceRequest"/>.</returns>
        object GetInstance(ServiceRequest serviceRequest);

        /// <summary>
        /// Determines if this factory can return an instance of the given <paramref name="serviceType"/> and <paramref name="serviceName"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns><b>true</b>, if the instance can be created, otherwise <b>false</b>.</returns>
        bool CanGetInstance(Type serviceType, string serviceName);
    }

    /// <summary>
    /// Represents a class that acts as a composition root for an <see cref="IServiceRegistry"/> instance.
    /// </summary>
    internal interface ICompositionRoot
    {
        /// <summary>
        /// Composes services by adding services to the <paramref name="serviceRegistry"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        void Compose(IServiceRegistry serviceRegistry);
    }

    /// <summary>
    /// Represents a class that is responsible for selecting properties that represents a dependency to the target <see cref="Type"/>.
    /// </summary>
    internal interface IPropertySelector
    {
        /// <summary>
        /// Selects properties that represents a dependency from the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which to select the properties.</param>
        /// <returns>A list of properties that represents a dependency to the target <paramref name="type"/></returns>
        IEnumerable<PropertyInfo> Select(Type type);
    }

    /// <summary>
    /// Represents a class that is responsible loading a set of assemblies based on the given search pattern.
    /// </summary>
    internal interface IAssemblyLoader
    {
        /// <summary>
        /// Loads a set of assemblies based on the given <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">The search pattern to use.</param>
        /// <returns>A list of assemblies based on the given <paramref name="searchPattern"/>.</returns>
        IEnumerable<Assembly> Load(string searchPattern);
    }

    /// <summary>
    /// Represents a class that is capable of scanning an assembly and register services into an <see cref="IServiceContainer"/> instance.
    /// </summary>
    internal interface IAssemblyScanner
    {
        /// <summary>
        /// Scans the target <paramref name="assembly"/> and registers services found within the assembly.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to scan.</param>        
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/> instance.</param>
        /// <param name="lifeCycleType">The <see cref="LifeCycleType"/> used to register the services found within the assembly.</param>
        /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        void Scan(Assembly assembly, IServiceRegistry serviceRegistry, LifeCycleType lifeCycleType, Func<Type, bool> shouldRegister);
    }

    /// <summary>
    /// Represents an inversion of control container.
    /// </summary>
    internal interface IServiceContainer : IServiceRegistry, IServiceFactory
    {
        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>        
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        /// will be used to configure the container.
        /// </remarks>     
        void RegisterAssembly(Assembly assembly);

        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        /// will be used to configure the container.
        /// </remarks>     
        void RegisterAssembly(Assembly assembly, Func<Type, bool> shouldRegister);

        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <param name="lifeCycleType">The <see cref="LifeCycleType"/> used to register the services found within the assembly.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        /// will be used to configure the container.
        /// </remarks>     
        void RegisterAssembly(Assembly assembly, LifeCycleType lifeCycleType);

        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <param name="lifeCycleType">The <see cref="LifeCycleType"/> used to register the services found within the assembly.</param>
        /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        /// will be used to configure the container.
        /// </remarks>     
        void RegisterAssembly(Assembly assembly, LifeCycleType lifeCycleType, Func<Type, bool> shouldRegister);

#if NET
        
        /// <summary>
        /// Registers services from assemblies in the base directory that matches the <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">The search pattern used to filter the assembly files.</param>
        void RegisterAssembly(string searchPattern);
#endif
    }

    /// <summary>
    /// An ultra lightweight service container.
    /// </summary>
    internal class ServiceContainer : IServiceContainer
    {        
        private const string UnresolvedDependencyError = "Unresolved dependency {0}";        
        private static readonly MethodInfo GetInstanceMethod;
        private readonly ServiceRegistry<Action<DynamicMethodInfo>> emitters = new ServiceRegistry<Action<DynamicMethodInfo>>();
        private readonly ServiceRegistry<OpenGenericEmitter> openGenericEmitters = new ServiceRegistry<OpenGenericEmitter>();
        private readonly DelegateRegistry<Type> delegates = new DelegateRegistry<Type>();
        private readonly DelegateRegistry<Tuple<Type, string>> namedDelegates = new DelegateRegistry<Tuple<Type, string>>();
        private readonly ThreadSafeDictionary<ServiceInfo, ConstructionInfo> implementations = new ThreadSafeDictionary<ServiceInfo, ConstructionInfo>();        
        private readonly ThreadSafeDictionary<ServiceInfo, Lazy<object>> singletons = new ThreadSafeDictionary<ServiceInfo, Lazy<object>>();
        private readonly Storage<object> constants = new Storage<object>();
        private readonly Stack<Action<DynamicMethodInfo>> dependencyStack = new Stack<Action<DynamicMethodInfo>>(); 
        private Storage<IFactory> factories;
        private bool firstServiceRequest = true;

        static ServiceContainer()
        {
            GetInstanceMethod = typeof(IFactory).GetMethod("GetInstance");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContainer"/> class.
        /// </summary>
        public ServiceContainer()
        {
            AssemblyScanner = new AssemblyScanner();
            PropertySelector = new PropertySelector();
#if NET
            AssemblyLoader = new AssemblyLoader();
#endif
        }

        /// <summary>
        /// Gets or sets the <see cref="IAssemblyScanner"/> instance that is responsible for scanning assemblies.
        /// </summary>
        public IAssemblyScanner AssemblyScanner { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IPropertySelector"/> instance that is responsible selecting the properties
        /// that represents a dependency for a given <see cref="Type"/>.
        /// </summary>
        public IPropertySelector PropertySelector { get; set; }
#if NET
        
        /// <summary>
        /// Gets or sets the <see cref="IAssemblyLoader"/> instance that is responsible for loading assemblies during assembly scanning. 
        /// </summary>
        public IAssemblyLoader AssemblyLoader { get; set; }
#endif
        
        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>        
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        /// will be used to configure the container.
        /// </remarks>             
        public void RegisterAssembly(Assembly assembly)
        {
            RegisterAssembly(assembly, LifeCycleType.Transient);
        }

        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        /// will be used to configure the container.
        /// </remarks>     
        public void RegisterAssembly(Assembly assembly, Func<Type, bool> shouldRegister)
        {
            AssemblyScanner.Scan(assembly, this, LifeCycleType.Transient, shouldRegister);
        }

        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <param name="lifeCycleType">The <see cref="LifeCycleType"/> used to register the services found within the assembly.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        /// will be used to configure the container.
        /// </remarks>     
        public void RegisterAssembly(Assembly assembly, LifeCycleType lifeCycleType)
        {
            AssemblyScanner.Scan(assembly, this, lifeCycleType, t => true);
        }

        /// <summary>
        /// Registers services from the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to be scanned for services.</param>
        /// <param name="lifeCycleType">The <see cref="LifeCycleType"/> used to register the services found within the assembly.</param>
        /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        /// <remarks>
        /// If the target <paramref name="assembly"/> contains an implementation of the <see cref="ICompositionRoot"/> interface, this 
        /// will be used to configure the container.
        /// </remarks>     
        public void RegisterAssembly(Assembly assembly, LifeCycleType lifeCycleType, Func<Type, bool> shouldRegister)
        {
            AssemblyScanner.Scan(assembly, this, lifeCycleType, shouldRegister);
        }

#if NET
       
        /// <summary>
        /// Registers services from assemblies in the base directory that matches the <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">The search pattern used to filter the assembly files.</param>
        public void RegisterAssembly(string searchPattern)
        {
            foreach (Assembly assembly in AssemblyLoader.Load(searchPattern))
            {
                this.RegisterAssembly(assembly);
            }
        }
#endif        
        
        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        public void Register(Type serviceType, Type implementingType, LifeCycleType lifeCycle)
        {
            Register(serviceType, implementingType, string.Empty, lifeCycle);
        }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        public void Register(Type serviceType, Type implementingType, string serviceName, LifeCycleType lifeCycle)
        {
            RegisterService(serviceType, implementingType, lifeCycle, serviceName);
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        public void Register<TService, TImplementation>() where TImplementation : TService
        {
            Register(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        public void Register<TService, TImplementation>(LifeCycleType lifeCycle) where TImplementation : TService
        {
            Register(typeof(TService), typeof(TImplementation), lifeCycle);
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="serviceName">The name of the service.</param>
        public void Register<TService, TImplementation>(string serviceName) where TImplementation : TService
        {
            Register<TService, TImplementation>(serviceName, LifeCycleType.Transient);
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <typeparam name="TImplementation">The implementing type.</typeparam>
        /// <param name="serviceName">The name of the service.</param>
        /// /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        public void Register<TService, TImplementation>(string serviceName, LifeCycleType lifeCycle) where TImplementation : TService
        {
            Register(typeof(TService), typeof(TImplementation), serviceName, lifeCycle);
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        /// describes the dependencies of the service. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        public void Register<TService>(Expression<Func<IServiceFactory, TService>> factory, LifeCycleType lifeCycle)
        {
            RegisterServiceFromLambdaExpression(factory, lifeCycle, string.Empty);
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        /// describes the dependencies of the service. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="serviceName">The name of the service.</param>        
        public void Register<TService>(Expression<Func<IServiceFactory, TService>> factory, string serviceName)
        {
            RegisterServiceFromLambdaExpression(factory, LifeCycleType.Transient, serviceName);
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        /// describes the dependencies of the service. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        /// <param name="serviceName">The name of the service.</param>        
        /// <param name="lifeCycle">The <see cref="LifeCycleType"/> that specifies the life cycle of the service.</param>
        public void Register<TService>(Expression<Func<IServiceFactory, TService>> factory, string serviceName, LifeCycleType lifeCycle)
        {
            RegisterServiceFromLambdaExpression(factory, lifeCycle, serviceName);
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the given <paramref name="instance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="instance">The instance returned when this service is requested.</param>
        public void Register<TService>(TService instance)
        {
            Register(instance, string.Empty);
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the given <paramref name="instance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="instance">The instance returned when this service is requested.</param>
        /// <param name="serviceName">The name of the service.</param>
        public void Register<TService>(TService instance, string serviceName)
        {
            RegisterValue(typeof(TService), instance, serviceName);
        }

        /// <summary>
        /// Registers the <typeparamref name="TService"/> with the <paramref name="factory"/> that 
        /// describes the dependencies of the service. 
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="factory">The lambdaExpression that describes the dependencies of the service.</param>
        /// <example>
        /// The following example shows how to register a new IFoo service.
        /// <code>
        /// <![CDATA[
        /// container.Register<IFoo>(r => new FooWithDependency(r.GetInstance<IBar>()))
        /// ]]>
        /// </code>
        /// </example>
        public void Register<TService>(Expression<Func<IServiceFactory, TService>> factory)
        {
            RegisterServiceFromLambdaExpression(factory, LifeCycleType.Transient, string.Empty);
        }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="serviceName">The name of the service.</param>
        public void Register(Type serviceType, Type implementingType, string serviceName)
        {
            RegisterService(serviceType, implementingType, LifeCycleType.Transient, serviceName);
        }

        /// <summary>
        /// Registers the <paramref name="serviceType"/> with the <paramref name="implementingType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementingType">The implementing type.</param>
        public void Register(Type serviceType, Type implementingType)
        {
            RegisterService(serviceType, implementingType, LifeCycleType.Transient, string.Empty);
        }

        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <returns>The requested service instance.</returns>
        public object GetInstance(Type serviceType)
        {
            Func<object[], object> del;

            if (!delegates.TryGetValue(serviceType, out del))
            {
                del = delegates.GetOrAdd(serviceType, t => CreateDelegate(t, string.Empty));
            }

            return del(constants.Items);                       
        }

        /// <summary>
        /// Gets an instance of the given <typeparamref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <returns>The requested service instance.</returns>
        public TService GetInstance<TService>()
        {
            return (TService)GetInstance(typeof(TService));
        }

        /// <summary>
        /// Gets a named instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance.</returns>    
        public TService GetInstance<TService>(string serviceName)
        {
            return (TService)GetInstance(typeof(TService), serviceName);
        }

        /// <summary>
        /// Gets a named instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="serviceName">The name of the requested service.</param>
        /// <returns>The requested service instance.</returns>
        public object GetInstance(Type serviceType, string serviceName)
        {
            Func<object[], object> del;

            if (!namedDelegates.TryGetValue(Tuple.Create(serviceType, serviceName), out del))
            {
                del = namedDelegates.GetOrAdd(Tuple.Create(serviceType, serviceName), t => CreateDelegate(t.Item1, serviceName));
            }

            return del(constants.Items);                              
        }

        /// <summary>
        /// Gets all instances of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of services to resolve.</param>
        /// <returns>A list that contains all implementations of the <paramref name="serviceType"/>.</returns>
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return (IEnumerable<object>)GetInstance(typeof(IEnumerable<>).MakeGenericType(serviceType));
        }

        /// <summary>
        /// Gets all instances of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of services to resolve.</typeparam>
        /// <returns>A list that contains all implementations of the <typeparamref name="TService"/> type.</returns>
        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return GetInstance<IEnumerable<TService>>();
        }

        private static Func<object[], object> CreateDynamicMethodDelegate(Action<DynamicMethodInfo> serviceEmitter, Type serviceType)
        {
            var dynamicMethodInfo = new DynamicMethodInfo();
            serviceEmitter(dynamicMethodInfo);
            if (serviceType.IsValueType)
            {
                dynamicMethodInfo.GetILGenerator().Emit(OpCodes.Box, serviceType);
            }

            return dynamicMethodInfo.CreateDelegate();
        }

        private static void EmitLoadConstant(DynamicMethodInfo dynamicMethodInfo, int index, Type type)
        {           
            var generator = dynamicMethodInfo.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, index);
            generator.Emit(OpCodes.Ldelem_Ref);
            generator.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
        }

        private static void EmitEnumerable(IList<Action<DynamicMethodInfo>> serviceEmitters, Type elementType, DynamicMethodInfo dynamicMethodInfo)
        {
            ILGenerator generator = dynamicMethodInfo.GetILGenerator();
            LocalBuilder array = generator.DeclareLocal(elementType.MakeArrayType());
            generator.Emit(OpCodes.Ldc_I4, serviceEmitters.Count);
            generator.Emit(OpCodes.Newarr, elementType);
            generator.Emit(OpCodes.Stloc, array);

            for (int index = 0; index < serviceEmitters.Count; index++)
            {
                generator.Emit(OpCodes.Ldloc, array);
                generator.Emit(OpCodes.Ldc_I4, index);
                var serviceEmitter = serviceEmitters[index];
                serviceEmitter(dynamicMethodInfo);
                generator.Emit(OpCodes.Stelem, elementType);
            }

            generator.Emit(OpCodes.Ldloc, array);
        }

        private static void EmitCallCustomFactory(DynamicMethodInfo dynamicMethodInfo, int serviceRequestConstantIndex, int factoryConstantIndex, Type serviceType)
        {
            ILGenerator generator = dynamicMethodInfo.GetILGenerator();
            EmitLoadConstant(dynamicMethodInfo, factoryConstantIndex, typeof(IFactory));
            EmitLoadConstant(dynamicMethodInfo, serviceRequestConstantIndex, typeof(ServiceRequest));
            generator.Emit(OpCodes.Callvirt, GetInstanceMethod);
            if (serviceType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, serviceType);
            }
        }

        private static bool IsEnumerableOfT(Type serviceType)
        {
            return serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        private static bool IsFunc(Type serviceType)
        {
            return serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(Func<>);
        }

        private static bool IsClosedGeneric(Type serviceType)
        {
            return serviceType.IsGenericType && !serviceType.IsGenericTypeDefinition;
        }

        private static bool IsFuncWithStringArgument(Type serviceType)
        {
            return serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(Func<,>)
                && serviceType.GetGenericArguments()[0] == typeof(string);
        }

        private static ConstructorInfo GetConstructorWithTheMostParameters(Type implementingType)
        {
            return implementingType.GetConstructors().OrderBy(c => c.GetParameters().Count()).LastOrDefault();
        }

        private static bool IsFactory(Type type)
        {
            return typeof(IFactory).IsAssignableFrom(type);
        }

        private static IEnumerable<ConstructorDependency> GetConstructorDependencies(ConstructorInfo constructorInfo)
        {
            return
                constructorInfo.GetParameters().OrderBy(p => p.Position).Select(
                    p => new ConstructorDependency { ServiceName = string.Empty, ServiceType = p.ParameterType, Parameter = p });
        }
       
        private ConstructionInfo CreateServiceInfo(ServiceInfo serviceInfo)
        {
            if (serviceInfo.FactoryExpression != null)
            {
                return this.CreateConstructionInfoFromLambdaExpression(serviceInfo.FactoryExpression);                
            }
            
            return this.CreateConstructionInfoFromImplementingType(serviceInfo);
        }

        private ConstructionInfo CreateConstructionInfoFromImplementingType(ServiceInfo serviceInfo)
        {
            var constructionInfo = new ConstructionInfo();
            ConstructorInfo constructorInfo = GetConstructorWithTheMostParameters(serviceInfo.ImplementingType);
            constructionInfo.ImplementingType = serviceInfo.ImplementingType;
            constructionInfo.Constructor = constructorInfo;
            constructionInfo.ConstructorDependencies.AddRange(GetConstructorDependencies(constructorInfo));
            constructionInfo.PropertyDependencies.AddRange(this.GetPropertyDependencies(serviceInfo.ImplementingType));
            return constructionInfo;
        }

        private IEnumerable<PropertyDependecy> GetPropertyDependencies(Type implementingType)
        {
            return GetInjectableProperties(implementingType).Select(
                p => new PropertyDependecy { Property = p, ServiceName = string.Empty, ServiceType = p.PropertyType });
        }

        private IEnumerable<PropertyInfo> GetInjectableProperties(Type implementingType)
        {
            return PropertySelector.Select(implementingType);
        }

        private ConstructionInfo CreateConstructionInfoFromLambdaExpression(LambdaExpression lambdaExpression)
        {
            var lambdaExpressionParser = new LambdaExpressionParser();
            ConstructionInfo constructionInfo = lambdaExpressionParser.Parse(lambdaExpression);
            return constructionInfo;
        }

        private Action<DynamicMethodInfo> GetEmitMethod(Type serviceType, string serviceName)
        {
            Action<DynamicMethodInfo> emitter = GetRegisteredEmitMethod(serviceType, serviceName);

            IFactory factory = GetCustomFactory(serviceType, serviceName);
            if (factory != null)
            {
                emitter = GetCustomFactoryEmitMethod(serviceType, serviceName, factory, emitter);
            }
            
            return this.CreateEmitMethodWrapper(emitter, serviceType, serviceName);
        }

        private Action<DynamicMethodInfo> CreateEmitMethodWrapper(Action<DynamicMethodInfo> emitter, Type serviceType, string serviceName)
        {
            if (emitter == null)
            {
                return null;
            }

            return dmi =>
                {
                    if (dependencyStack.Contains(emitter))
                    {
                        throw new InvalidOperationException(
                            string.Format("Recursive dependency detected: ServiceType:{0}, ServiceName:{1}]", serviceType, serviceName));
                    }

                    dependencyStack.Push(emitter);
                    emitter(dmi);
                    dependencyStack.Pop();
                };
        }

        private Action<DynamicMethodInfo> GetCustomFactoryEmitMethod(Type serviceType, string serviceName, IFactory factory, Action<DynamicMethodInfo> emitter)
        {
            if (emitter != null)
            {
                var del = CreateDynamicMethodDelegate(emitter, typeof(IFactory));
                emitter = CreateEmitMethodBasedOnCustomFactory(serviceType, serviceName, factory, () => del(constants.Items));
            }
            else
            {
                emitter = CreateEmitMethodBasedOnCustomFactory(serviceType, serviceName, factory, null);
            }

            UpdateServiceRegistration(serviceType, serviceName, emitter);
            return emitter;
        }

        private Action<DynamicMethodInfo> GetRegisteredEmitMethod(Type serviceType, string serviceName)
        {
            Action<DynamicMethodInfo> emitter;
            var registrations = this.GetServiceRegistrations(serviceType);
            registrations.TryGetValue(serviceName, out emitter);
            return emitter ?? ResolveUnknownServiceEmitter(serviceType, serviceName);
        }

        private void UpdateServiceRegistration(Type serviceType, string serviceName, Action<DynamicMethodInfo> emitter)
        {
            if (emitter != null)
            {
                GetServiceRegistrations(serviceType).AddOrUpdate(serviceName, s => emitter, (s, m) => emitter);
            }
        }

        private void EmitRequestInstance(ServiceInfo serviceInfo, DynamicMethodInfo dynamicMethodInfo)
        {
            if (!dynamicMethodInfo.ContainsLocalVariable(serviceInfo))
            {
                EmitNewInstance(serviceInfo, dynamicMethodInfo);
                dynamicMethodInfo.EmitStoreLocalVariable(serviceInfo);
            }

            dynamicMethodInfo.EmitLoadLocalVariable(serviceInfo);
        }

        private void EmitNewInstance(ServiceInfo serviceInfo, DynamicMethodInfo dynamicMethodInfo)
        {
            ConstructionInfo constructionInfo = this.GetConstructionInfo(serviceInfo);
            if (constructionInfo.FactoryDelegate != null)
            {
                this.EmitNewInstanceUsingFactoryDelegate(constructionInfo.FactoryDelegate, dynamicMethodInfo);
            }
            else
            {
                this.EmitNewInstanceUsingImplementingType(dynamicMethodInfo, constructionInfo);
            }            
        }

        private void EmitNewInstanceUsingImplementingType(DynamicMethodInfo dynamicMethodInfo, ConstructionInfo constructionInfo)
        {
            ILGenerator generator = dynamicMethodInfo.GetILGenerator();
            this.EmitConstructorDependencies(constructionInfo, dynamicMethodInfo);
            generator.Emit(OpCodes.Newobj, constructionInfo.Constructor);
            this.EmitPropertyDependencies(constructionInfo, dynamicMethodInfo);
        }

        private void EmitNewInstanceUsingFactoryDelegate(Delegate factoryDelegate, DynamicMethodInfo dynamicMethodInfo)
        {            
            var factoryDelegateIndex = constants.Add(factoryDelegate);
            var serviceFactoryIndex = constants.Add(this);
            Type funcType = factoryDelegate.GetType();            
            EmitLoadConstant(dynamicMethodInfo, factoryDelegateIndex, funcType);
            EmitLoadConstant(dynamicMethodInfo, serviceFactoryIndex, typeof(IServiceFactory));
            ILGenerator generator = dynamicMethodInfo.GetILGenerator();
            MethodInfo invokeMethod = funcType.GetMethod("Invoke");
            generator.Emit(OpCodes.Callvirt, invokeMethod);
        }

        private void EmitConstructorDependencies(ConstructionInfo constructionInfo, DynamicMethodInfo dynamicMethodInfo)
        {
            foreach (ConstructorDependency dependency in constructionInfo.ConstructorDependencies)
            {
                this.EmitDependency(dynamicMethodInfo, dependency);
            }
        }

        private void EmitDependency(DynamicMethodInfo dynamicMethodInfo, Dependency dependency)
        {
            ILGenerator generator = dynamicMethodInfo.GetILGenerator();
            if (dependency.FactoryExpression != null)
            {
                var lambda = Expression.Lambda(dependency.FactoryExpression, new ParameterExpression[] { }).Compile();
                MethodInfo methodInfo = lambda.GetType().GetMethod("Invoke");
                EmitLoadConstant(dynamicMethodInfo, constants.Add(lambda), lambda.GetType());
                generator.Emit(OpCodes.Callvirt, methodInfo);
            }
            else
            {
                Action<DynamicMethodInfo> emitter = GetEmitMethod(dependency.ServiceType, dependency.ServiceName);                                                
                if (emitter == null)
                {
                    throw new InvalidOperationException(string.Format(UnresolvedDependencyError, dependency));
                }

                try
                {
                    emitter(dynamicMethodInfo);
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException(string.Format(UnresolvedDependencyError, dependency), ex);
                }                
            }
        }

        private void EmitPropertyDependencies(ConstructionInfo constructionInfo, DynamicMethodInfo dynamicMethodInfo)
        {
            if (constructionInfo.PropertyDependencies.Count == 0)
            {
                return;
            }

            ILGenerator generator = dynamicMethodInfo.GetILGenerator();
            LocalBuilder instance = generator.DeclareLocal(constructionInfo.ImplementingType);
            generator.Emit(OpCodes.Stloc, instance);
            foreach (var propertyDependency in constructionInfo.PropertyDependencies)
            {
                generator.Emit(OpCodes.Ldloc, instance);
                EmitDependency(dynamicMethodInfo, propertyDependency);
                dynamicMethodInfo.GetILGenerator().Emit(OpCodes.Callvirt, propertyDependency.Property.GetSetMethod());
            }

            generator.Emit(OpCodes.Ldloc, instance);
        }

        private Action<DynamicMethodInfo> ResolveUnknownServiceEmitter(Type serviceType, string serviceName)
        {
            Action<DynamicMethodInfo> emitter = null;
            if (IsFunc(serviceType))
            {
                emitter = CreateServiceEmitterBasedOnFuncServiceRequest(serviceType, false);
            }
            else if (IsEnumerableOfT(serviceType))
            {
                emitter = CreateEnumerableServiceEmitter(serviceType);
            }
            else if (IsFuncWithStringArgument(serviceType))
            {
                emitter = CreateServiceEmitterBasedOnFuncServiceRequest(serviceType, true);
            }
            else if (CanRedirectRequestForDefaultServiceToSingleNamedService(serviceType, serviceName))
            {
                emitter = CreateServiceEmitterBasedOnSingleNamedInstance(serviceType);
            }
            else if (IsClosedGeneric(serviceType))
            {
                emitter = CreateServiceEmitterBasedOnClosedGenericServiceRequest(serviceType, serviceName);
            }
                        
            UpdateServiceRegistration(serviceType, serviceName, emitter);
            
            return emitter;
        }

        private Action<DynamicMethodInfo> CreateEmitMethodBasedOnCustomFactory(Type serviceType, string serviceName, IFactory factory, Func<object> proceed)
        {
            int serviceRequestConstantIndex = CreateServiceRequestConstant(serviceType, serviceName, proceed);
            var factoryConstantIndex = this.CreateFactoryConstant(factory);
            return dmi => EmitCallCustomFactory(dmi, serviceRequestConstantIndex, factoryConstantIndex, serviceType);
        }

        private int CreateFactoryConstant(IFactory factory)
        {
            int factoryConstantIndex = this.constants.Add(factory);
            return factoryConstantIndex;
        }

        private int CreateServiceRequestConstant(Type serviceType, string serviceName, Func<object> proceed)
        {
            var serviceRequest = new ServiceRequest { ServiceType = serviceType, ServiceName = serviceName, Proceed = proceed };
            return constants.Add(serviceRequest);
        }

        private IFactory GetCustomFactory(Type serviceType, string serviceName)
        {
            if (IsFactory(serviceType) ||
                (IsEnumerableOfT(serviceType) && IsFactory(serviceType.GetGenericArguments().First())))
            {
                return null;
            }

            return factories.Items.FirstOrDefault(f => f.CanGetInstance(serviceType, serviceName));            
        }

        private Action<DynamicMethodInfo> CreateEnumerableServiceEmitter(Type serviceType)
        {
            Type actualServiceType = serviceType.GetGenericArguments()[0];
            if (actualServiceType.IsGenericType)
            {
                EnsureEmitMethodsForOpenGenericTypesAreCreated(actualServiceType);
            }

            IList<Action<DynamicMethodInfo>> serviceEmitters = GetServiceRegistrations(actualServiceType).Values.ToList();
            
            if (dependencyStack.Count > 0 && serviceEmitters.Contains(dependencyStack.Peek()))
            {
                serviceEmitters.Remove(dependencyStack.Peek());
            }

            return dmi => EmitEnumerable(serviceEmitters, actualServiceType, dmi);
        }

        private void EnsureEmitMethodsForOpenGenericTypesAreCreated(Type actualServiceType)
        {
            var openGenericServiceType = actualServiceType.GetGenericTypeDefinition();
            var openGenericServiceEmitters = this.GetOpenGenericRegistrations(openGenericServiceType);
            foreach (var openGenericEmitterEntry in openGenericServiceEmitters.Keys)
            {
                GetRegisteredEmitMethod(actualServiceType, openGenericEmitterEntry);
            }
        }

        private Action<DynamicMethodInfo> CreateServiceEmitterBasedOnFuncServiceRequest(Type serviceType, bool namedService)
        {
            var actualServiceType = serviceType.GetGenericArguments().Last();
            var methodInfo = typeof(ServiceContainer).GetMethod("CreateFuncGetInstanceDelegate", BindingFlags.Instance | BindingFlags.NonPublic);
            var del = methodInfo.MakeGenericMethod(actualServiceType).Invoke(this, new object[] { namedService });
            var constantIndex = constants.Add(del);
            return dmi => EmitLoadConstant(dmi, constantIndex, serviceType);
        }

        private Delegate CreateFuncGetInstanceDelegate<TServiceType>(bool namedService)
        {
            if (namedService)
            {
                Func<string, TServiceType> func = GetInstance<TServiceType>;
                return func;
            }
            else
            {
                Func<TServiceType> func = GetInstance<TServiceType>;
                return func;
            }
        }

        private Action<DynamicMethodInfo> CreateServiceEmitterBasedOnClosedGenericServiceRequest(Type serviceType, string serviceName)
        {
            Type openGenericType = serviceType.GetGenericTypeDefinition();

            OpenGenericEmitter openGenericEmitter = GetOpenGenericTypeInfo(openGenericType, serviceName);
            if (openGenericEmitter == null)
            {
                return null;
            }

            Type closedGenericType = openGenericEmitter.ImplementingType.MakeGenericType(serviceType.GetGenericArguments());
            return dmi => openGenericEmitter.EmitMethod(dmi, serviceType, closedGenericType);
        }

        private OpenGenericEmitter GetOpenGenericTypeInfo(Type serviceType, string serviceName)
        {
            var openGenericRegistrations = GetOpenGenericRegistrations(serviceType);
            if (CanRedirectRequestForDefaultOpenGenericServiceToSingleNamedService(serviceType, serviceName))
            {
                return openGenericRegistrations.First().Value;
            }

            OpenGenericEmitter openGenericEmitter;
            openGenericRegistrations.TryGetValue(serviceName, out openGenericEmitter);
            return openGenericEmitter;
        }

        private Action<DynamicMethodInfo> CreateServiceEmitterBasedOnSingleNamedInstance(Type serviceType)
        {
            return GetEmitMethod(serviceType, GetServiceRegistrations(serviceType).First().Key);
        }

        private bool CanRedirectRequestForDefaultServiceToSingleNamedService(Type serviceType, string serviceName)
        {
            return string.IsNullOrEmpty(serviceName) && GetServiceRegistrations(serviceType).Count == 1;
        }

        private bool CanRedirectRequestForDefaultOpenGenericServiceToSingleNamedService(Type serviceType, string serviceName)
        {
            return string.IsNullOrEmpty(serviceName) && GetOpenGenericRegistrations(serviceType).Count == 1;
        }

        private ConstructionInfo GetConstructionInfo(ServiceInfo serviceInfo)
        {
            return implementations.GetOrAdd(serviceInfo, CreateServiceInfo);
        }

        private ThreadSafeDictionary<string, Action<DynamicMethodInfo>> GetServiceRegistrations(Type serviceType)
        {
            return this.emitters.GetOrAdd(serviceType, s => new ThreadSafeDictionary<string, Action<DynamicMethodInfo>>(StringComparer.InvariantCultureIgnoreCase));
        }

        private ThreadSafeDictionary<string, OpenGenericEmitter> GetOpenGenericRegistrations(Type serviceType)
        {
            return this.openGenericEmitters.GetOrAdd(serviceType, s => new ThreadSafeDictionary<string, OpenGenericEmitter>(StringComparer.InvariantCultureIgnoreCase));
        }

        private void RegisterService(Type serviceType, Type implementingType, LifeCycleType lifeCycleType, string serviceName)
        {
            if (serviceType.IsGenericTypeDefinition)
            {
                RegisterOpenGenericService(serviceType, implementingType, lifeCycleType, serviceName);
            }
            else
            {
                var serviceInfo = new ServiceInfo { ServiceType = serviceType, ImplementingType = implementingType, ServiceName = serviceName };
                Action<DynamicMethodInfo> emitDelegate = GetEmitDelegate(serviceInfo, IsFactory(serviceType) ? LifeCycleType.Singleton : lifeCycleType);                    
                GetServiceRegistrations(serviceType).AddOrUpdate(serviceName, s => emitDelegate, (s, i) => emitDelegate);
            }
        }

        private void RegisterOpenGenericService(Type serviceType, Type implementingType, LifeCycleType lifeCycleType, string serviceName)
        {
            var openGenericTypeInfo = new OpenGenericEmitter { ImplementingType = implementingType };            
            if (lifeCycleType == LifeCycleType.Transient)
            {
                openGenericTypeInfo.EmitMethod = (dmi, closedGenericServiceType, closedGenericImplementingType) =>
                    {
                        var serviceInfo = new ServiceInfo { ServiceType = closedGenericServiceType, ImplementingType = closedGenericImplementingType, ServiceName = serviceName };
                        EmitNewInstance(serviceInfo, dmi);
                    };
            }
            else if (lifeCycleType == LifeCycleType.Singleton)
            {
                openGenericTypeInfo.EmitMethod = (dmi, closedGenericServiceType, closedGenericImplementingType) =>
                    {
                        var serviceInfo = new ServiceInfo { ServiceType = closedGenericImplementingType, ImplementingType = closedGenericImplementingType, ServiceName = serviceName };
                        EmitSingletonInstance(serviceInfo, GetEmitDelegate(serviceInfo, LifeCycleType.Transient), dmi);
                    };
            }
            else if (lifeCycleType == LifeCycleType.Request)
            {
                openGenericTypeInfo.EmitMethod = (dmi, closedGenericServiceType, closedGenericImplementingType) =>
                    {
                        var serviceInfo = new ServiceInfo { ServiceType = closedGenericServiceType, ImplementingType = closedGenericImplementingType, ServiceName = serviceName };
                        EmitRequestInstance(serviceInfo, dmi);
                    };
            }

            GetOpenGenericRegistrations(serviceType).AddOrUpdate(serviceName, s => openGenericTypeInfo, (s, o) => openGenericTypeInfo);
        }

        private Action<DynamicMethodInfo> GetEmitDelegate(ServiceInfo serviceInfo, LifeCycleType lifeCycleType)
        {
            Action<DynamicMethodInfo> emitter = null;
            switch (lifeCycleType)
            {
                case LifeCycleType.Transient:
                    emitter = dynamicMethodInfo => EmitNewInstance(serviceInfo, dynamicMethodInfo);                    
                    break;
                case LifeCycleType.Request:
                    emitter = dynamicMethodInfo => EmitRequestInstance(serviceInfo, dynamicMethodInfo);                   
                    break;
                case LifeCycleType.Singleton:
                    emitter = dynamicMethodInfo => EmitSingletonInstance(serviceInfo, this.GetEmitDelegate(serviceInfo, LifeCycleType.Transient), dynamicMethodInfo);                   
                    break;
            }

            return emitter;
        }

        private void EmitSingletonInstance(ServiceInfo serviceInfo, Action<DynamicMethodInfo> instanceEmitter, DynamicMethodInfo dynamicMethodInfo)
        {
            EmitLoadConstant(dynamicMethodInfo, this.CreateSingletonConstantIndex(serviceInfo, instanceEmitter), serviceInfo.ServiceType);
        }
        
        private int CreateSingletonConstantIndex(ServiceInfo serviceInfo, Action<DynamicMethodInfo> instanceEmitter)
        {
            return constants.Add(this.GetSingletonInstance(serviceInfo, instanceEmitter));
        }
               
        private object GetSingletonInstance(ServiceInfo serviceInfo, Action<DynamicMethodInfo> instanceEmitter)
        {            
            return this.singletons.GetOrAdd(serviceInfo, t => new Lazy<object>(() => this.CreateSingletonInstance(instanceEmitter))).Value;
        }

        private object CreateSingletonInstance(Action<DynamicMethodInfo> instanceEmitter)
        {
            var dynamicMethodInfo = new DynamicMethodInfo();
            instanceEmitter(dynamicMethodInfo);
            object instance = dynamicMethodInfo.CreateDelegate()(constants.Items);
            return instance;
        }

        private Func<object[], object> CreateDelegate(Type serviceType, string serviceName)
        {
            if (this.FirstServiceRequest())
            {
                EnsureThatServiceRegistryIsConfigured(serviceType);
                CreateCustomFactories();
            }

            dependencyStack.Clear();
            var serviceEmitter = this.GetEmitMethod(serviceType, serviceName);
            if (serviceEmitter == null)
            {
                throw new InvalidOperationException(string.Format("Unable to resolve type: {0}, service name: {1}", serviceType, serviceName));
            }

            try
            {
                return CreateDynamicMethodDelegate(serviceEmitter, serviceType);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Format("Unable to resolve type: {0}, service name: {1}", serviceType, serviceName), ex);
            }            
        }

        private bool FirstServiceRequest()
        {
            if (firstServiceRequest)
            {                
                firstServiceRequest = false;
                return true;
            }

            return false;
        }

        private void CreateCustomFactories()
        {            
            factories = new Storage<IFactory>(GetInstance<IEnumerable<IFactory>>());            
        }

        private void EnsureThatServiceRegistryIsConfigured(Type serviceType)
        {
            if (ServiceRegistryIsEmpty())
            {
                this.RegisterAssembly(serviceType.Assembly);
            }
        }
         
        private bool ServiceRegistryIsEmpty()
        {
            return this.emitters.Count == 0 && this.openGenericEmitters.Count == 0;
        }

        private void RegisterValue(Type serviceType, object value, string serviceName)
        {
            int index = constants.Add(value);
            Action<DynamicMethodInfo> emitter = dmi => EmitLoadConstant(dmi, index, serviceType);
            GetServiceRegistrations(serviceType).AddOrUpdate(serviceName, d => emitter, (s, d) => emitter);
        }

        private void RegisterServiceFromLambdaExpression<TService>(
            Expression<Func<IServiceFactory, TService>> factory, LifeCycleType lifeCycleType, string serviceName)
        {
            var serviceInfo = new ServiceInfo { ServiceType = typeof(TService), FactoryExpression = factory, ServiceName = serviceName };
            Action<DynamicMethodInfo> emitDelegate = GetEmitDelegate(serviceInfo, IsFactory(typeof(TService)) ? LifeCycleType.Singleton : lifeCycleType);
            GetServiceRegistrations(typeof(TService)).AddOrUpdate(serviceName, s => emitDelegate, (s, i) => emitDelegate);            
        }
       
        /// <summary>
        /// Parses a <see cref="LambdaExpression"/> into a <see cref="ConstructionInfo"/> instance.
        /// </summary>
        public class LambdaExpressionParser
        {                                                
            /// <summary>
            /// Parses the <paramref name="lambdaExpression"/> and returns a <see cref="ConstructionInfo"/> instance.
            /// </summary>
            /// <param name="lambdaExpression">The <see cref="LambdaExpression"/> to parse.</param>
            /// <returns>A <see cref="ConstructionInfo"/> instance.</returns>
            public ConstructionInfo Parse(LambdaExpression lambdaExpression)
            {                                
                switch (lambdaExpression.Body.NodeType)
                {
                    case ExpressionType.New:
                        return CreateServiceInfoBasedOnNewExpression((NewExpression)lambdaExpression.Body);
                    case ExpressionType.MemberInit:
                        return CreateServiceInfoBasedOnHandleMemberInitExpression((MemberInitExpression)lambdaExpression.Body);                                      
                    default:
                        return CreateServiceInfoBasedOnLambdaExpression(lambdaExpression);                        
                }                
            }

            private static ConstructionInfo CreateServiceInfoBasedOnLambdaExpression(LambdaExpression lambdaExpression)
            {
                return new ConstructionInfo { FactoryDelegate = lambdaExpression.Compile() };
            }

            private static ConstructionInfo CreateServiceInfoBasedOnNewExpression(NewExpression newExpression)
            {
                var serviceInfo = CreateServiceInfo(newExpression);
                ParameterInfo[] parameters = newExpression.Constructor.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    ConstructorDependency constructorDependency = CreateConstructorDependency(parameters[i]);
                    ApplyDependencyDetails(newExpression.Arguments[i], constructorDependency);
                    serviceInfo.ConstructorDependencies.Add(constructorDependency);
                }

                return serviceInfo;
            }

            private static ConstructionInfo CreateServiceInfo(NewExpression newExpression)
            {
                var serviceInfo = new ConstructionInfo { Constructor = newExpression.Constructor, ImplementingType = newExpression.Constructor.DeclaringType };
                return serviceInfo;
            }

            private static ConstructionInfo CreateServiceInfoBasedOnHandleMemberInitExpression(MemberInitExpression memberInitExpression)
            {
                var serviceInfo = CreateServiceInfoBasedOnNewExpression(memberInitExpression.NewExpression);
                foreach (MemberBinding memberBinding in memberInitExpression.Bindings)
                {
                    HandleMemberAssignment((MemberAssignment)memberBinding, serviceInfo);
                }

                return serviceInfo;
            }
           
            private static void HandleMemberAssignment(MemberAssignment memberAssignment, ConstructionInfo constructionInfo)
            {
                var propertyDependency = CreatePropertyDependency(memberAssignment);
                ApplyDependencyDetails(memberAssignment.Expression, propertyDependency);
                constructionInfo.PropertyDependencies.Add(propertyDependency);
            }

            private static ConstructorDependency CreateConstructorDependency(ParameterInfo parameterInfo)
            {
                var constructorDependency = new ConstructorDependency
                {
                    Parameter = parameterInfo,
                    ServiceType = parameterInfo.ParameterType
                };
                return constructorDependency;
            }

            private static PropertyDependecy CreatePropertyDependency(MemberAssignment memberAssignment)
            {
                var propertyDependecy = new PropertyDependecy
                {
                    Property = (PropertyInfo)memberAssignment.Member,
                    ServiceType = ((PropertyInfo)memberAssignment.Member).PropertyType
                };
                return propertyDependecy;
            }

            private static void ApplyDependencyDetails(Expression expression, Dependency dependency)
            {                
                if (RepresentsServiceFactoryMethod(expression))
                {
                    ApplyDependencyDetailsFromMethodCall((MethodCallExpression)expression, dependency);
                }
                else
                {
                    ApplyDependecyDetailsFromExpression(expression, dependency);
                }
            }

            private static bool RepresentsServiceFactoryMethod(Expression expression)
            {
                return IsMethodCall(expression) &&
                    IsServiceFactoryMethod(((MethodCallExpression)expression).Method);
            }

            private static bool IsMethodCall(Expression expression)
            {
                return expression.NodeType == ExpressionType.Call;
            }

            private static bool IsServiceFactoryMethod(MethodInfo methodInfo)
            {
                return methodInfo.DeclaringType == typeof(IServiceFactory);
            }

            private static void ApplyDependecyDetailsFromExpression(Expression expression, Dependency dependency)
            {
                dependency.FactoryExpression = expression;
                dependency.ServiceName = string.Empty;
            }

            private static void ApplyDependencyDetailsFromMethodCall(MethodCallExpression methodCallExpression, Dependency dependency)
            {
                dependency.ServiceType = methodCallExpression.Method.ReturnType;
                if (RepresentsGetNamedInstanceMethod(methodCallExpression))
                {
                    dependency.ServiceName = (string)((ConstantExpression)methodCallExpression.Arguments[0]).Value;
                }
                else
                {
                    dependency.ServiceName = string.Empty;
                }
            }

            private static bool RepresentsGetNamedInstanceMethod(MethodCallExpression node)
            {
                return IsGetInstanceMethod(node.Method) && HasOneArgumentRepresentingServiceName(node);
            }

            private static bool IsGetInstanceMethod(MethodInfo methodInfo)
            {
                return methodInfo.Name == "GetInstance";
            }

            private static bool HasOneArgumentRepresentingServiceName(MethodCallExpression node)
            {
                return HasOneArgument(node) && IsConstantExpression(node.Arguments[0]);
            }

            private static bool HasOneArgument(MethodCallExpression node)
            {
                return node.Arguments.Count == 1;
            }

            private static bool IsConstantExpression(Expression argument)
            {                
                return argument.NodeType == ExpressionType.Constant;
            }
        }
 
        public class ServiceInfo 
        {
            public Type ServiceType { get; set; }

            public string ServiceName { get; set; }

            public Type ImplementingType { get; set; }

            public LambdaExpression FactoryExpression { get; set; }

            public override int GetHashCode()
            {
                return ServiceType.GetHashCode() ^ ServiceName.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var other = obj as ServiceInfo;
                if (other == null)
                {
                    return false;
                }

                var result = this.ServiceName == other.ServiceName && this.ServiceType == other.ServiceType;
                return result;                
            }            
        }

        /// <summary>
        /// Contains information about how to create a service instance.
        /// </summary>
        public class ConstructionInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConstructionInfo"/> class.
            /// </summary>
            public ConstructionInfo()
            {
                PropertyDependencies = new List<PropertyDependecy>();
                ConstructorDependencies = new List<ConstructorDependency>();
            }

            /// <summary>
            /// Gets or sets the implementing type that represents the concrete class to create.
            /// </summary>
            public Type ImplementingType { get; set; }
            
            /// <summary>
            /// Gets or sets the <see cref="ConstructorInfo"/> that is used to create a service instance.
            /// </summary>
            public ConstructorInfo Constructor { get; set; }

            /// <summary>
            /// Gets a list of <see cref="PropertyDependecy"/> instances that represent 
            /// the property dependencies for the target service instance. 
            /// </summary>
            public List<PropertyDependecy> PropertyDependencies { get; private set; }

            /// <summary>
            /// Gets a list of <see cref="ConstructorDependency"/> instances that represent 
            /// the property dependencies for the target service instance. 
            /// </summary>
            public List<ConstructorDependency> ConstructorDependencies { get; private set; }

            /// <summary>
            /// Gets or sets the function delegate to be used to create the service instance.
            /// </summary>
            public Delegate FactoryDelegate { get; set; }
        }

        /// <summary>
        /// Represents a class dependency.
        /// </summary>
        public abstract class Dependency
        {
            /// <summary>
            /// Gets or sets the service <see cref="Type"/> of the <see cref="Dependency"/>.
            /// </summary>
            public Type ServiceType { get; set; }

            /// <summary>
            /// Gets or sets the service name of the <see cref="Dependency"/>.
            /// </summary>
            public string ServiceName { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="FactoryExpression"/> that represent getting the value of the <see cref="Dependency"/>.
            /// </summary>            
            public Expression FactoryExpression { get; set; }

            /// <summary>
            /// Returns textual information about the dependency.
            /// </summary>
            /// <returns>A string that describes the dependency.</returns>
            public override string ToString()
            {
                var sb = new StringBuilder();
                return sb.AppendFormat("[Requested dependency: ServiceType:{0}, ServiceName:{1}]", ServiceType, ServiceName).ToString();                                
            }
        }

        /// <summary>
        /// Represents a property dependency.
        /// </summary>
        public class PropertyDependecy : Dependency
        {
            /// <summary>
            /// Gets or sets the <see cref="MethodInfo"/> that is used to set the property value.
            /// </summary>
            public PropertyInfo Property { get; set; }

            /// <summary>
            /// Returns textual information about the dependency.
            /// </summary>
            /// <returns>A string that describes the dependency.</returns>
            public override string ToString()
            {
                return string.Format("[Target Type: {0}], [Property: {1}({2})]", this.Property.DeclaringType, this.Property.Name, this.Property.PropertyType) + ", " + base.ToString();
            }
        }

        /// <summary>
        /// Represents a constructor dependency.
        /// </summary>
        public class ConstructorDependency : Dependency
        {
            /// <summary>
            /// Gets or sets the <see cref="ParameterInfo"/> for this <see cref="ConstructorDependency"/>.
            /// </summary>
            public ParameterInfo Parameter { get; set; }

            /// <summary>
            /// Returns textual information about the dependency.
            /// </summary>
            /// <returns>A string that describes the dependency.</returns>
            public override string ToString()
            {
                return string.Format("[Target Type: {0}], [Parameter: {1}({2})]", this.Parameter.Member.DeclaringType, this.Parameter.Name, this.Parameter.ParameterType) + ", " + base.ToString();
            }
        }

        private class Storage<T>
        {
            private readonly object lockObject = new object();
            private T[] items = new T[0];

            public Storage()
            {
            }
            
            public Storage(IEnumerable<T> collection)
            {
                items = collection.ToArray();
            }

            public T[] Items
            {
                get
                {
                    return this.items;
                }
            }

            public int Add(T value)
            {
                int index = Array.IndexOf(this.items, value);
                if (index == -1)
                {
                    return TryAddValue(value);
                }

                return index;
            }

            private int TryAddValue(T value)
            {
                lock (lockObject)
                {
                    int index = Array.IndexOf(items, value);
                    if (index == -1)
                    {
                        index = AddValue(value);
                    }

                    return index;
                }
            }

            private int AddValue(T value)
            {
                int index = items.Length;
                T[] snapshot = CreateSnapshot();
                snapshot[index] = value;
                this.items = snapshot;
                return index;
            }

            private T[] CreateSnapshot()
            {
                var snapshot = new T[items.Length + 1];
                Array.Copy(items, snapshot, items.Length);
                return snapshot;
            }
        }

        private class KeyValueStorage<TKey, TValue>
        {
            private readonly object lockObject = new object();
            private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

            public bool TryGetValue(TKey key, out TValue value)
            {
                return dictionary.TryGetValue(key, out value);                
            }

            public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
            {
                TValue value;
                if (!dictionary.TryGetValue(key, out value))
                {
                    lock (lockObject)
                    {
                        value = TryAddValue(key, valueFactory);
                    }
                }

               return value;
            }

            private TValue TryAddValue(TKey key, Func<TKey, TValue> valueFactory)
            {
                TValue value;
                if (!this.dictionary.TryGetValue(key, out value))
                {
                    var snapshot = new Dictionary<TKey, TValue>(this.dictionary);
                    value = valueFactory(key);
                    snapshot.Add(key, value);
                    this.dictionary = snapshot;
                }

                return value;
            }
        }

        private class DynamicMethodInfo
        {
            private readonly IDictionary<ServiceInfo, LocalBuilder> localVariables = new Dictionary<ServiceInfo, LocalBuilder>();

            private DynamicMethod dynamicMethod;

            public DynamicMethodInfo()
            {
                CreateDynamicMethod();
            }

            public ILGenerator GetILGenerator()
            {
                return dynamicMethod.GetILGenerator();
            }

            public Func<object[], object> CreateDelegate()
            {
                dynamicMethod.GetILGenerator().Emit(OpCodes.Ret);
                return (Func<object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object[], object>));
            }

            public bool ContainsLocalVariable(ServiceInfo serviceInfo)
            {
                return localVariables.ContainsKey(serviceInfo);
            }

            public void EmitLoadLocalVariable(ServiceInfo serviceInfo)
            {
                dynamicMethod.GetILGenerator().Emit(OpCodes.Ldloc, localVariables[serviceInfo]);
            }

            public void EmitStoreLocalVariable(ServiceInfo serviceInfo)
            {
                localVariables.Add(serviceInfo, dynamicMethod.GetILGenerator().DeclareLocal(serviceInfo.ServiceType));
                dynamicMethod.GetILGenerator().Emit(OpCodes.Stloc, localVariables[serviceInfo]);
            }

            private void CreateDynamicMethod()
            {
#if NET                
                dynamicMethod = new DynamicMethod(
                    "DynamicMethod", typeof(object), new[] { typeof(object[]) }, typeof(ServiceContainer).Module, false);
#endif
#if SILVERLIGHT
                dynamicMethod = new DynamicMethod(
                    "DynamicMethod", typeof(object), new[] { typeof(List<object>) });
#endif
            }
        }
#if NET
                
        private class ThreadSafeDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
        {
            public ThreadSafeDictionary()
            {
            }

            public ThreadSafeDictionary(IEqualityComparer<TKey> comparer)
                : base(comparer)
            {
            }
        }
#endif
#if SILVERLIGHT
        
        private class ThreadSafeDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            private readonly Dictionary<TKey, TValue> dictionary;
            private readonly object syncObject = new object();

            public ThreadSafeDictionary()
            {
                dictionary = new Dictionary<TKey, TValue>();
            }            

            public ThreadSafeDictionary(IEqualityComparer<TKey> comparer)
            {
                dictionary = new Dictionary<TKey, TValue>(comparer);
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            public ICollection<TValue> Values
            {
                get
                {
                    lock (syncObject)
                    {
                        return dictionary.Values;
                    }
                }
            }

            public ICollection<TKey> Keys
            {
                get
                {
                    lock (syncObject)
                    {
                        return dictionary.Keys;
                    }
                }
            }

            public TValue GetOrAdd(TKey key, Func<TKey, TValue> valuefactory)
            {
                lock (syncObject)
                {
                    TValue value;
                    if (!dictionary.TryGetValue(key, out value))
                    {
                        value = valuefactory(key);
                        dictionary.Add(key, value);
                    }

                    return value;
                }
            }    
        
            public void AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
            {
                lock (syncObject)
                {
                    TValue value;
                    if (!dictionary.TryGetValue(key, out value))
                    {
                        dictionary.Add(key, addValueFactory(key));
                    }
                    else
                    {
                        dictionary[key] = updateValueFactory(key, value);
                    }
                }
            }

            public void TryGetValue(TKey key, out TValue value)
            {
                lock (syncObject)
                {
                    dictionary.TryGetValue(key, out value);
                }
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                lock (syncObject)
                {
                    return dictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value).GetEnumerator();
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }        
#endif

        private class ServiceRegistry<T> : ThreadSafeDictionary<Type, ThreadSafeDictionary<string, T>>
        {
        }

        private class DelegateRegistry<TKey> : KeyValueStorage<TKey, Func<object[], object>>
        {
        }

        private class OpenGenericEmitter
        {
            public Type ImplementingType { get; set; }

            public Action<DynamicMethodInfo, Type, Type> EmitMethod { get; set; }
        }      
    }

    /// <summary>
    /// Contains information about a service request passed to an <see cref="IFactory"/> instance.
    /// </summary>    
    internal class ServiceRequest
    {
        /// <summary>
        /// Gets a value indicating whether the service request can be resolved by the underlying container.  
        /// </summary>
        public bool CanProceed
        {
            get { return Proceed != null; }
        }

        /// <summary>
        /// Gets or sets the requested service type.
        /// </summary>
        public Type ServiceType { get; internal set; }

        /// <summary>
        /// Gets or sets the requested service name.
        /// </summary>
        public string ServiceName { get; internal set; }

        /// <summary>
        /// Gets or sets the function delegate used to proceed.
        /// </summary>
        public Func<object> Proceed { get; internal set; }
    }

    /// <summary>
    /// An assembly scanner that registers services based on the types contained within an <see cref="Assembly"/>.
    /// </summary>    
    internal class AssemblyScanner : IAssemblyScanner
    {
        /// <summary>
        /// Scans the target <paramref name="assembly"/> and registers services found within the assembly.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to scan.</param>        
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/> instance.</param>
        /// <param name="lifeCycleType">The <see cref="LifeCycleType"/> used to register the services found within the assembly.</param>
        /// <param name="shouldRegister">A function delegate that determines if a service implementation should be registered.</param>
        public void Scan(Assembly assembly, IServiceRegistry serviceRegistry, LifeCycleType lifeCycleType, Func<Type, bool> shouldRegister)
        {            
            IEnumerable<Type> concreteTypes = GetConcreteTypes(assembly).ToList();
            var compositionRoots = concreteTypes.Where(t => typeof(ICompositionRoot).IsAssignableFrom(t)).ToList();
            if (compositionRoots.Count > 0)
            {
                ExecuteCompositionRoots(compositionRoots, serviceRegistry);
            }
            else
            {
                foreach (Type type in concreteTypes.Where(shouldRegister))
                {
                    BuildImplementationMap(type, serviceRegistry, lifeCycleType);
                }
            }
        }

        private static void ExecuteCompositionRoots(IEnumerable<Type> compositionRoots, IServiceRegistry serviceRegistry)
        {
            foreach (var compositionRoot in compositionRoots)
            {
                ((ICompositionRoot)Activator.CreateInstance(compositionRoot)).Compose(serviceRegistry);
            }
        }

        private static string GetServiceName(Type serviceType, Type implementingType)
        {
            string implementingTypeName = implementingType.Name;
            string serviceTypeName = serviceType.Name;
            if (implementingType.IsGenericTypeDefinition)
            {
                var regex = new Regex("((?:[a-z][a-z]+))", RegexOptions.IgnoreCase);
                implementingTypeName = regex.Match(implementingTypeName).Groups[1].Value;
                serviceTypeName = regex.Match(serviceTypeName).Groups[1].Value;
            }

            if (serviceTypeName.Substring(1) == implementingTypeName)
            {
                implementingTypeName = string.Empty;
            }

            return implementingTypeName;
        }

        private static IEnumerable<Type> GetBaseTypes(Type concreteType)
        {
            Type baseType = concreteType;
            while (baseType != typeof(object) && baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }

        private static IEnumerable<Type> GetConcreteTypes(Assembly assembly)
        {            
            return assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !(t.Namespace ?? string.Empty).StartsWith("System"));
        }

        private void BuildImplementationMap(Type implementingType, IServiceRegistry serviceRegistry, LifeCycleType lifeCycleType)
        {
            Type[] interfaces = implementingType.GetInterfaces();
            foreach (Type interfaceType in interfaces)
            {
                RegisterInternal(interfaceType, implementingType, serviceRegistry, lifeCycleType);
            }

            foreach (Type baseType in GetBaseTypes(implementingType))
            {
                RegisterInternal(baseType, implementingType, serviceRegistry, lifeCycleType);
            }
        }

        private void RegisterInternal(Type serviceType, Type implementingType, IServiceRegistry serviceRegistry, LifeCycleType lifeCycleType)
        {
            if (serviceType.IsGenericType && serviceType.ContainsGenericParameters)
            {
                serviceType = serviceType.GetGenericTypeDefinition();
            }

            serviceRegistry.Register(serviceType, implementingType, GetServiceName(serviceType, implementingType), lifeCycleType);
        }
    }

    /// <summary>
    /// Selects the properties that represents a dependency to the target <see cref="Type"/>.
    /// </summary>
    internal class PropertySelector : IPropertySelector
    {
        /// <summary>
        /// Selects properties that represents a dependency from the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which to select the properties.</param>
        /// <returns>A list of properties that represents a dependency to the target <paramref name="type"/></returns>
        public IEnumerable<PropertyInfo> Select(Type type)
        {
            return type.GetProperties().Where(IsInjectable).ToList();
        }

        /// <summary>
        /// Determines if the <paramref name="propertyInfo"/> represents an injectable property.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> that describes the target property.</param>
        /// <returns><b>true</b> if the property is injectable, otherwise <b>false</b>.</returns>
        protected virtual bool IsInjectable(PropertyInfo propertyInfo)
        {
            return !IsReadOnly(propertyInfo);
        }

        private static bool IsReadOnly(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetSetMethod(false) == null || propertyInfo.GetSetMethod(false).IsStatic;
        }
    }
#if NET
    
    /// <summary>
    /// Loads all assemblies from the application base directory that matches the given search pattern.
    /// </summary>
    internal class AssemblyLoader : IAssemblyLoader
    {
        /// <summary>
        /// Loads a set of assemblies based on the given <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">The search pattern to use.</param>
        /// <returns>A list of assemblies based on the given <paramref name="searchPattern"/>.</returns>
        public IEnumerable<Assembly> Load(string searchPattern)
        {
            string directory = Path.GetDirectoryName(new Uri(typeof(ServiceContainer).Assembly.CodeBase).LocalPath);
            if (directory != null)
            {
                string[] searchPatterns = searchPattern.Split('|');
                foreach (string file in searchPatterns.SelectMany(sp => Directory.GetFiles(directory, sp)).Where(CanLoad))
                {
                    yield return Assembly.LoadFrom(file);
                }
            }
        }

        /// <summary>
        /// Indicates if the current <paramref name="fileName"/> represent a file that can be loaded.
        /// </summary>
        /// <param name="fileName">The name of the target file.</param>
        /// <returns><b>true</b> if the file can be loaded, otherwise <b>false</b>.</returns>
        protected virtual bool CanLoad(string fileName)
        {
            return true;
        }
    }
#endif
}