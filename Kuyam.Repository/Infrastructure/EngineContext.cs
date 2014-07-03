using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Configuration;

namespace Kuyam.Repository.Infrastructure
{
    public class EngineContext
    {
        #region Initialization Methods
       
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IEngine Initialize(bool forceRecreate)
        {
            if (Singleton<IEngine>.Instance == null || forceRecreate)
            {                      
                Singleton<IEngine>.Instance = CreateEngineInstance();               
               
            }
            return Singleton<IEngine>.Instance;
        }
              
        public static void Replace(IEngine engine)
        {
            Singleton<IEngine>.Instance = engine;
        }
         
        public static IEngine CreateEngineInstance()
        {           
            return new KuyamEngine();
        }

        #endregion
                
        public static IEngine Current
        {
            get
            {
                if (Singleton<IEngine>.Instance == null)
                {
                    Initialize(false);
                }
                return Singleton<IEngine>.Instance;
            }
        }
    }
}
