using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Repository.Infrastructure.DependencyManagement;

namespace Kuyam.Repository.Infrastructure
{
    public interface IEngine
    {
        ContainerManager ContainerManager { get; }        

        T Resolve<T>() where T : class;

        object Resolve(Type type);

        Array ResolveAll(Type serviceType);

        T[] ResolveAll<T>();
    }
}
