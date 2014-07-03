using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Repository.Infrastructure.DependencyManagement;
using Autofac;

namespace Kuyam.Repository.Infrastructure
{
    public class KuyamEngine : IEngine
    {
        #region Fields

        private ContainerManager _containerManager;

        #endregion

        #region Ctor
              
        public KuyamEngine()
        {
            InitializeContainer();
        }

        #endregion

        #region Utilities


        private void InitializeContainer()
        {
            var builder = new ContainerBuilder();
            _containerManager = new ContainerManager(builder.Build());

            _containerManager.AddComponent<ITypeFinder, WebAppTypeFinder>("kuyam.typeFinder");
            var typeFinder = _containerManager.Resolve<ITypeFinder>();

            var drTypes = typeFinder.FindClassesOfType<IDependencyRegistrar>();
            var drInstances = new List<IDependencyRegistrar>();
            foreach (var drType in drTypes)
                drInstances.Add((IDependencyRegistrar)Activator.CreateInstance(drType));

            //sort
            drInstances = drInstances.AsQueryable().OrderBy(t => t.Order).ToList();

            _containerManager.UpdateContainer(x =>
            {
                foreach (var dependencyRegistrar in drInstances)
                    dependencyRegistrar.Register(x, typeFinder);
            });

        }

        #endregion
       
        #region Methods

        public T Resolve<T>() where T : class
        {
            return ContainerManager.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return ContainerManager.Resolve(type);
        }

        public Array ResolveAll(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public T[] ResolveAll<T>()
        {
            return ContainerManager.ResolveAll<T>();
        }

        #endregion

        #region Properties

        public IContainer Container
        {
            get { return _containerManager.Container; }
        }

        public ContainerManager ContainerManager
        {
            get { return _containerManager; }
        }

        #endregion
    }
}
