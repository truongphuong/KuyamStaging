﻿using Kuyam.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Domain.CategoryServices
{
    public interface ICategoryService
    {
        /// <summary>
        /// Get list active categories 
        /// </summary>
        /// <returns></returns>
        List<Service> GetSequenceCategories();
    }
}