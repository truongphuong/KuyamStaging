using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database.BlogModels;
using Kuyam.Database;

namespace Kuyam.Domain.Mappers
{
    public class Mapper
    {
        public BlogUser Map(List<be_Profiles> source)
        {
            var destination = new BlogUser();
            var dictionary = new Dictionary<string, object>();
            foreach (var profile in source)
            {
                var settingName = MappingField.GetFieldFromUserProfile(profile.SettingName);
                if (!string.IsNullOrEmpty(settingName.FieldName))
                {                    
                    var value = UtilityHelper.ChangeType(settingName.Type,profile.SettingValue);
                    dictionary.Add(settingName.FieldName, value);
                }
                
            }
            return (BlogUser)UtilityHelper.ConvertTo<BlogUser>(dictionary);
        }
    }
}
