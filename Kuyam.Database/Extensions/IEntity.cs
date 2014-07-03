using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
	interface IEntity
	{
		//T Load(int id);
		void Save();
		void Update();
	}
}
