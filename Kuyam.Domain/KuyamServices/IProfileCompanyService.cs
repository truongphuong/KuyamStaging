using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;

namespace Kuyam.Domain.KuyamServices
{
    public interface IProfileCompanyService
    {
        ProfileCompany GetByProfileId(int profileId);
        IQueryable<ProfileCompany> GetAll();
    }
}
