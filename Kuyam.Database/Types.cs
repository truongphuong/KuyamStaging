using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using M2.Util;
using Kuyam.Database;
using System.Dynamic;

namespace Kuyam.Database
{
    // TODO: Move to domain
    public static class Types
    {
        /*		public static StandardTypeList CompanyTypes { get { return DAL.GetTypes(DAL.TypeGroupEnum.Company); } }
                public static StandardTypeList BusinessTypes { get { return DAL.GetTypes(DAL.TypeGroupEnum.Business); } }
                public static StandardTypeList CompanyStatus { get { return DAL.GetTypes(DAL.TypeGroupEnum.CompanyStatus); } }
                public static StandardTypeList CustStatus { get { return DAL.GetTypes(DAL.TypeGroupEnum.CustStatus); } }
                public static StandardTypeList DeactivationReasons { get { return DAL.GetTypes(DAL.TypeGroupEnum.DeactivationReason); } }
                public static StandardTypeList DataFieldTypes { get { return DAL.GetTypes(DAL.TypeGroupEnum.DataField); } }
                public static StandardTypeList GenderTypes { get { return DAL.GetTypes(DAL.TypeGroupEnum.Gender); } }
                public static StandardTypeList PreferredPhoneTypes { get { return DAL.GetTypes(DAL.TypeGroupEnum.PreferredPhone); } }
                public static StandardTypeList AppointmentStatus { get { return DAL.GetTypes(DAL.TypeGroupEnum.AppointmentStatus); } }
                public static StandardTypeList CustTypes { get { return DAL.GetTypes(DAL.TypeGroupEnum.Cust); } }
                public static StandardTypeList AppointmentNextActionActor { get { return DAL.GetTypes(DAL.TypeGroupEnum.AppointmentNextActionActor); } }
                public static StandardTypeList Days { get { return DAL.GetTypes(DAL.TypeGroupEnum.Days); } }
                public static StandardTypeList MediaTypes { get { return DAL.GetTypes(DAL.TypeGroupEnum.Media); } }
                public static StandardTypeList MediaLocations { get { return DAL.GetTypes(DAL.TypeGroupEnum.MediaLocation); } }
                public static StandardTypeList AccountStatus { get { return DAL.GetTypes(DAL.TypeGroupEnum.AccountStatus); } }
            }
        */

        public const string LightBlue = "7fd4ce";
        public const string Yellow = "FDF690";
        public const string Red = "FBC98E";
        public const string KaturaDoman = "https://cdnsecakmi.kaltura.com";
        public static StandardTypeList GetTypeList(TypeGroup tg)
        {
            return DAL.GetTypes((int)tg);
        }

        public static List<Type> GetCarrierList(TypeGroup tg)
        {
            return DAL.GetCarrier((int)tg);
        }

        public static string GetTypeName(TypeGroup groupID, int typeID)
        {
            return GetTypeList(groupID)[typeID];
        }

        //-----------------

        // Read data from database and assemble types.  Probably need to build list
        // using dictionary methods since using data, not explicitly typed-in field names
        static Types()
        {
            dynamic test = new ExpandoObject();
            test.hello = "hello";
            test.level1 = new ExpandoObject();
            test.level1.level2 = 5;
        }


        //-----------------

        public enum TypeGroup
        {
            AccountStatus = 16,
            AppointmentNextActionActor = 11,
            AppointmentParticipantType = 21,
            AppointmentStatus = 9,
            AppointmentCompanyAction = 17,
            CalendarDisplayType = 20,
            CompanyCategory = 2,
            CompanyStatus = 3,
            CompanyType = 1,
            CustStatus = 4,
            CustType = 10,
            Day = 12,
            DeactivationReason = 5,
            Gender = 7,
            MediaLocation = 15,
            MediaType = 14,
            PreferredPhone = 8,
            PrivacyType = 18,
            RelationshipType = 19,
            ShareType = 22,
            Temp = 13,
            Uifielddef = 6,
            Carrier = 23,
            LandingPageStatus = 24
        };


        public enum AccountStatus
        {
            Unknown = -1,
            Ok = 138,
            UnservedZipCode = 139
        };

        public enum AlertTime
        {
            none = 0,
            AtTimeOfEvent = 1,
            FifteenMinBefore = 2,
            ThirtyMinBefore = 3,
            OneHourBefore = 4,
            TwoHoursBefore = 5,
            OneDayBefore = 6,
            TwoDaysBefore = 7

        };

        public enum AppointmentNextActionActor
        {
            Unknown = -1,
            Nobody = 117,
            Customer = 118,
            Company = 119,
            Kuyam = 120
        };

        public enum AppointmentParticipantType
        {
            Unknown = -1,
            Owner = 253,
            Invitee = 254
        };

        public enum AppointmentStatus
        {
            Unknown = -1,
            Delete = 114,
            Confirmed = 113,
            CompanyModified = 112,//Company Modified
            Pending = 111,
            Modified = 110, //User modified         
            Cancelled = 144,
            TemporaryPending = 100  // the appointment will be pending a short time ( 10 mins) because it belong to another appointment process
        };

        public enum NoteType
        {
            Company = 1,
            User = 2
        };

        public enum AppointmentVendorAction
        {
            Unknown = -1,
            Accept = 141,
            ProposeAlternateTime = 142,
            Decline = 143
        };

        public enum CalendarDisplayType
        {
            Unknown = -1,
            Hidden = 250,
            Selectable = 251,
            Selected = 252
        };

        public enum CompanyCategory
        {
            Unknown = 140,
            Accounting = 29,
            Advertising = 30,
            Antiques = 31,
            Apartments = 32,
            AthleticTraining = 33,
            Attorney = 34,
            AutoDealer = 35,
            AutoRepairAndService = 36,
            AutoWash = 37,
            BusinessCenter = 38,
            CarDetailing = 39,
            CarpetAndUpholsteryCleaning = 40,
            Caterer = 41,
            ChildCare = 42,
            Chiropractor = 43,
            ComputerConsulting = 44,
            ComputerServices = 45,
            ConcreteContractors = 46,
            Consignments = 47,
            Construction = 48,
            Consulting = 49,
            Cosmetics = 50,
            Counseling = 51,
            Dentist = 52,
            Doors, Windows = 53,
            Electrical = 54,
            Employment = 55,
            Engineering = 56,
            ExcavatingContractor = 57,
            FenceContractors = 58,
            FitnessCenter = 60,
            Florist = 61,
            FuneralHome = 62,
            GolfCourse = 63,
            GolfDrivingRange = 64,
            HairSalon = 65,
            HealthCareBusiness = 66,
            Heating_Cooling = 67,
            HomeConstruction = 68,
            HomePartyConsultants = 69,
            IndependentConsultant = 70,
            Insurance = 71,
            InteriorDecorators = 72,
            InteriorDesigner = 73,
            Investments = 74,
            LandTitle = 75,
            LandscapeContractors = 76,
            LawnService = 77,
            LeasingManager = 78,
            LimousineService = 79,
            Marketing = 80,
            Optometrist = 81,
            PaintingContractor = 82,
            PersonalCareWorkers = 83,
            PetCareAndBoarding = 84,
            Pharmacy = 85,
            Photography = 86,
            PhysicalTherapy = 87,
            Physician = 88,
            Plumber = 89,
            Printers = 90,
            PropertyManager = 91,
            RealEstate = 92,
            Restaurant = 93,
            ScreenPrinting = 94,
            SecuritySystems = 95,
            SkydivingAndTraining = 96,
            SportsTherapy = 97,
            SportsTraining = 98,
            Staffing = 99,
            Surveyor = 100,
            Tanning = 101,
            TattooAndBodyPiercing = 102,
            Tavern = 103,
            TavernAndRestaurant = 104,
            TaxiService = 105,
            Theatre = 106,
            Tires = 107,
            TravelAgent = 108,
            TreeAndLawnCare = 109,
            Veterinarian = 110,
            Banking = 149,
            HomeFurnishingsservices = 150,
            Hotel = 151,
            NonProfit = 152,
            Medical = 153,
            Market = 154,
            Automobile = 155,
            Cafe = 156,
            Yoga = 157,
            Entertainment = 158,
            Financial = 159,
            Bakery = 160,
            OfficeofficeServices = 161,
            Museum = 162,
            Spa = 163,
            PawnShop = 164,
            CommunityResources = 165,
            Jewelery = 166,
            Clothing = 167,
            Technology = 168,
            HumanResources = 169,
            Architects = 170,
            Environmental = 171,
            Association = 172,
            Parking = 173,
            Publications = 174,
            Education = 175,
            Unknownnon_existent = 176,
            GraphicDesign = 177,
            PublicRelations = 178,
            Transportation = 179,
            Manufacturing = 180,
            DesignAndCommunications = 181,
            For_profitSchool = 183,
            Mail = 184,
            University = 185,
            OrganizationtimeManagement = 186,
            ConsumerGoodsservices = 187,
            Rental = 188,
            BusinessServices = 189,
            Conglomerate = 190,
            FoodServices = 191,
            PublicUtility = 192,
            EventPlanning = 193,
            PetSupplies = 194,
            Tours = 195,
            Religious = 196,
            Gallery = 197,
            PersonalCoaching = 198,
            Books = 199,
            TalentManagement = 200,
            Energy = 201,
            Wine = 202,
            Political = 203,
            AssistedLivingnursing = 204,
            Psychiatrytherapy = 205,
            Videogame = 206,
            Science = 207
        };

        public enum CompanyStatus
        {
            Unknown = -1,
            Frozen = 5,
            Cancelled = 6,
            Active = 7,
            Pending = 8,
            Deleted = 9
        };

        public enum EmployeeType
        {
            Unknown = -1,
            service = 0,
            instructor = 1
        };

        public enum ServiceType
        {
            Unknown = -1,
            ServiceType = 0,
            ClassType = 1
        };


        public enum PaymentMethod
        {
            Paypal,
            PayInPerson,
            GiftCard
        }

        public enum PaymentStatus
        {
            Pending = 10,
            Authorized = 20,
            Paid = 30,
            PartiallyRefunded = 40,
            Refunded = 50,
            Voided = 60,
        }

        public enum OrderStatus
        {
            Pending = 10,
            Processing = 20,
            Complete = 30,
            Cancelled = 40
        }

        public enum CompanyType
        {
            Unknown = -1,
            KuyamInstantBook = 1,
            KuyamBookIt = 2,
            HybridKuyamBookIt = 3,
            NonKuyamBookIt = 4,
            GeneralAvailability = 5
        };

        public enum CustStatus
        {
            Unknown = -1,
            Definition = 10,
            Review = 11,
            Active = 12,
            Inactive = 13,
            Deleted = 14
        };

        public enum ServiceCompanyStatus
        {
            Active = 0,
            Delete = 1
        };

        public enum CustType
        {
            Unknown = -1,
            Facebook = 114,
            Personal = 115,
            Company = 116,

        };

        public enum InviteType
        {
            User = 0,
            Company = 1,
            SMSVerify = 2
        }
        public enum FlagInvite
        {
            Unknown = -1,
            Verified = 0,
            Unverified = 1
        }

        public enum Day
        {
            Unknown = -1,
            Isdaily = 1234560,
            Sunday = 0,
            Monday = 1,
            Tuesday = 2,
            Wednesday = 3,
            Thursday = 4,
            Friday = 5,
            Saturday = 6
        };

        public enum ShortDay
        {
            Unknown = -1,
            Sun = 0,
            Mon = 1,
            Tue = 2,
            Wed = 3,
            Thu = 4,
            Fri = 5,
            Sat = 6

        };

        public enum DeactivationReason
        {
            Unknown = -1,
            Billingissue = 15
        };

        public enum Gender
        {
            Unknown = -1,
            Female = 25,
            Male = 24
        };

        public enum SortBy
        {
            AutoSchedule = 0,
            CompanyName = 1
        };

        public enum MediaLocation
        {
            Unknown = -1,
            Localimage = 134,
            Localvideo = 135,
            Kaltura = 136,
            Youtube = 137
        };

        public enum MediaType
        {
            Unknown = -1,
            Image = 131,
            Video = 132,
            Audio = 133
        };

        public enum MediaCropType
        {
            DetailAfterLogin = 1,
            DetailBeforeLogin = 2,
            MainPostBeforeLogin = 3,
            MainPostAfterLogin = 4,
            HomeAfterLogin = 5,
            HomeBeforeLogin = 6,
        }


        public enum PreferredPhone
        {
            Default = 0,
            Email = 1,
            Text = 2,
            Call = 4,
        };

        public enum ContactType
        {
            None = 0,
            Email = 1,
            SMS = 2,
            EmailSMS = 3
        }

        public enum PrivacyType
        {
            Unknown = -1,
            Private = 239,
            Public = 240
        };

        public enum RelationshipType
        {
            Unknown = -1,
            Spouse = 241,
            SignificantOtherpartner = 242,
            Child = 243,
            Co_worker = 244,
            Managerboss = 245,
            Parent = 246,
            Grandparent = 247,
            OtherFamily = 248,
            Other = 249,
            Company = 255
        };

        public enum ShareType
        {
            Unknown = -1,
            Freebusy = 256,
            Reader = 257,
            Writer = 258
        };

        public enum CalendarType
        {
            Default = 0,
            Google = 1,
            Facebook = 2,
            iCal = 3
        };

        public enum Temp
        {
            Unknown = -1,
            Weekdays = 128,
            Weekend = 129,
            EveryDay = 130
        };
        public enum CompanyPolicies
        {
            Unknown = -1,
            None = 0,
            Standard = 24,
            Strict = 72
        };
        public enum Uifielddef
        {
            Unknown = -1,
            Textbox = 16,
            Checkbox = 17,
            RadioButtons = 18,
            Combobox = 19,
            DropdownList = 20,
            Checkboxlist = 21,
            Listbox = 22,
            Radiobuttons = 23,
            Image = 234,
            Hours = 235,
            Fileupload = 236,
            Imageupload = 237,
            Videoupload = 238
        };

        public enum CompanyMediaType
        {
            IsLogo = 0,
            IsBanner = 1,
            IsVideo = 2
        };

        public enum CompanyPackageStatus
        {
            Inactive = 0,
            Active = 1
        };

        public enum CompanyPackageType
        {
            ByUnlimited = 0,
            ByQuanlity = 1,
        };

        public enum RegularClientSearchKeyType
        {
            Unknown = -1,
            FirstName = 0,
            LastName = 1,
            Email = 2,
            Newest = 3
        };

        public enum DiscountStatus
        {
            Inactive = 0,
            Active = 1
        };

        public enum CancellationType
        {
            None = 0,
            Standard = 1,
            Strict = 2,
            Custom = 3
        }
        public enum UserStatusType
        {
            Unknown = -1,
            Cancelled = 0,
            Active = 1,
            Frozen = 2,
            Deleted = 3
        }
        public enum UserInviteCodeStatusType
        {
            Unknown = -1,
            Deleted = 0,
            Active = 1,
            Pending = 2,
            Approved = 3,
            Denied = 4

        }
        public enum FlagPageType
        {
            Unknown = -1,
            CalendarSetting = 0
        }

        public enum WebServiceStatus
        {
            TimeOut = -1,
            Fail = 0,
            Success = 1,
            ErrorHandling = 2,
            NoResult = 3
        }


        public enum AvaibilityCalendarIphoneStatus
        {
            Unknown = 0,
            Busy = 3,
            Avaibility = 1,
            Pending = 2
        }

        public enum UserPackageStatus
        {
            Active = 0,
            Delete = 1
        }

        public enum KuyamVersion
        {
            Dev = 0,
            QA = 1,
            Production = 2
        }

        public enum DeviceType
        {
            Website = 0,
            iOS = 1,
            Android = 2
        }

        public enum NotificationType
        {
            AppointmentConfirmed = 1,
            AppointmentCancelled = 2,
            AppointmentModified = 3,
            AppointmentDeleted = 4,
            AppointmentNote = 5,
            ProposedAppointment = 6,
            GeneralPushMessage = 7
        }

        public enum GettyImageStatus
        {
            Unknown = -1,
            Pending = 0,
            Paid = 1,
            Downloaded = 3,
            Completed = 4,
            Delete = 5
        };
        public enum ImageType
        {
            Unknown = -1,
            Kaltura = 0,
            GettyImage = 1,
        };
        public enum Role
        {
            Unknown = -1,
            Admin = 0,
            Agent = 1,
            Guest = 2,
            HotelStaff = 3,
            HotelAdmin = 4,
            Concierge = 5
        };

        #region hotel enum

        public enum HotelCodeStatus
        {
            Active = 0,
            InActive = 1,
            Delete = 2
        }

        public enum HotelType
        {
            Default = 0
        }

        public enum HotelMediaType
        {
            IsLogo = 1,
            IsBanner = 2,
            IsVideo = 3
        }

        public enum ApplicationType
        {
            Kuyam = 0,
            Concierge = 1
        }

        public enum FeatureCompanyType
        {
            Unknown = 0,
            Relax = 1,
            Enegize = 2,
            Grow = 3
        }
        #endregion hotel enum

        public enum ServiceVersion
        {
            AppointmentStatus = 1,
            AppointmentNote = 2
        }

        public enum NonKuyamAppointment
        {
            pending = 0,
            booked = 1
        }

        public enum GiftCardType
        {
            email = 1,
            mail = 2
        }

        public enum CheckoutType
        {
            Availability = 0,
            NonAvailability = 1,
            GeneralAvailability = 2,
            ClassBooking = 3,
        }

        public enum EntityAlertType
        {
            FirstAlert = 0,
            SecondAlert = 1
        }
        public enum GiftCardShippingMethod
        {
            ViaEmail = 0,
            ViaFreeStandardShipping = 1,
            ViaPreminumShipping = 2
        }
        public enum GiftCardStatus
        {
            Send = 1,
            NoSend = 0
        }

        public enum DiscountType
        {
            Company = 0,
            Admin = 1
        }

        public enum SmsStatus
        {
            Old = 0,
            New = 1
        }
        public enum DeliveryStatus
        {
            Send = 1,
            CanNotSend = 0,
            Delete = -1,
            Temp = 2
        }
        public enum ReceiveStatus
        {
            Sender = 0,
            Receive = 1
        }

        public enum RequestAppoitmentStatus
        {
            Default = 0,
            Booked = 1
        }

        public enum ProposedAppointmentStatus
        {
            Default = 1,
            booked = 2
        }

        public enum LandingPageStatus
        {
            Draft = 274,
            Published = 275,
            Unpublished = 276
        }

        public enum LinkType
        {
            Opentable = 1,
            Searchbox = 2
        }


        public enum NotifyType
        {
            Confirm = 1,
            Modify = 2,
            Cancel = 3
        }

        public enum BookingType
        {
            ServiceBooking = 0,
            ClassBooking = 1
        }
    }
}