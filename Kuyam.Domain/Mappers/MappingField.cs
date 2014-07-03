using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Domain.Mappers
{
    public class MappingField
    {

        public static FieldInfo GetFieldFromUserProfile(string field)
        {
            var result = new FieldInfo();
            switch(field)
            {
                case "displayname": result.FieldName = "DisplayName";
                    result.Type = typeof(string);
                    break;
                case "firstname": result.FieldName = "FirstName";
                    result.Type = typeof(string);
                    break;
                case "lastname": result.FieldName = "LastName";
                    result.Type = typeof(string);
                    break;
                case "birthday": result.FieldName = "Birthday";
                    result.Type = typeof(DateTime);
                    break;
                case "emailaddress": result.FieldName = "EmailAddress";
                    result.Type = typeof(string);
                    break;
                case "aboutme": result.FieldName = "AboutMe";
                    result.Type = typeof(string);
                    break;
                case "isprivate": result.FieldName = "IsPrivate";
                    result.Type = typeof(bool);
                    break;
                case "facebook": result.FieldName = "Facebook";
                    result.Type = typeof(string);
                    break;
                case "twitter": result.FieldName = "Twitter";
                    result.Type = typeof(string);
                    break;
                case "pinterest": result.FieldName = "Pinterest";
                    result.Type = typeof(string);
                    break;
                case "website": result.FieldName = "Website";
                    result.Type = typeof(string);
                    break;
            }
            return result;
        }
    }
}
