using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Repository.Interface;
using Kuyam.Database;
using Kuyam.Database.BlogModels;
using Kuyam.Domain.Mappers;

namespace Kuyam.Domain.BlogServices
{
    public class BlogUserService: IBlogUserService
    {
        #region Private Fields
        private readonly IRepository<be_Profiles> _profileRepository;
        private readonly IRepository<be_Users> _userRepository;
        #endregion
        public BlogUserService(IRepository<be_Profiles> profileRepository, IRepository<be_Users> userRepository)
        {
            _profileRepository = profileRepository;
            _userRepository = userRepository;
        }

        public BlogUser GetById(string userName)
        {
            var mapper = new Mapper();
            var profiles = _profileRepository.Table.Where(t => t.UserName == userName).ToList();
            var userInfo = mapper.Map(profiles);
            return userInfo;
        }

        public BlogUser GetByEmail(string email)
        {
            var mapper = new Mapper();
            var user = _userRepository.Table.Where(t => t.EmailAddress == email).FirstOrDefault();
            if (user == null) return null;
            var profiles = _profileRepository.Table.Where(t => t.UserName == user.UserName).ToList();
            var userInfo = mapper.Map(profiles);
            return userInfo;
        }

        public  Dictionary<string, dynamic> GetProfile(string userName)
        {
            return _profileRepository.Table.Where(m => m.UserName.ToLower() == userName.ToLower()).ToDictionary(k => k.SettingName, v => (dynamic)v.SettingValue);
        }
    }
}
