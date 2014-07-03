using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuyam.Database
{
    public class Constants
    {
        public static Dictionary<string, string> colors = new Dictionary<string, string>{
            {"Light Blue","00FFFF"},
            {"Green Yellow","ADFF2F"},
            {"Dark Salmon","E9967A"},
            {"Brown","A52A2A"},
            {"Alice Blue","F0F8FF"},
            {"Aquamarine","7FFFD4"},
            {"Peru","CD853F"},
            {"Pink","FFC0CB"},
            {"Sea Green","2E8B57"},
            {"Steel Blue","4682B4"},
            {"Sky Blue","87CEEB"},
            {"Olive","808000"},
            {"Lime Green","32CD32"},
            {"Deep Sky Blue","00BFFF"},
            {"Slate Blue","6A5ACD"}
            
        };

        public static Dictionary<string, string> classNames = new Dictionary<string, string>{
            {"lightblue","00FFFF"},
            {"greenyellow","ADFF2F"},
            {"darksalmon","E9967A"},
            {"brown","A52A2A"},
            {"aliceblue","F0F8FF"},
            {"aquamarine","7FFFD4"},
            {"peru","CD853F"},
            {"pink","FFC0CB"},
            {"seagreen","2E8B57"},
            {"steelblue","4682B4"},
            {"skyblue","87CEEB"},
            {"olive","808000"},
            {"limegreen","32CD32"},
            {"deepskyblue","00BFFF"},
            {"slateblue","6A5ACD"}
            
        };
        public const string GoogleErrorText = "access_denied";
        public const string iPhone = "https://itunes.apple.com/us/app/kuyam/id606531094?mt=8";
        public const string Android = "https://play.google.com/store/apps/details?id=com.kourosh.kuyam";
    }
}
