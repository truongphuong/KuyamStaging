using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository.Interface;

namespace Kuyam.Domain
{
    public class HotelService : IDisposable
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService" /> class.
        /// </summary>
        /// <param name="hotelRepository">The hotel repository.</param>
        /// <param name="hotelMediaRepository">The hotel media repository.</param>
        /// <param name="hotelCodeRepository">The hotel code repository.</param>
        /// <param name="featuredHotelCodeRepository">The featured hotel code repository.</param>
        public HotelService(IRepository<Hotel> hotelRepository, IRepository<HotelMedia> hotelMediaRepository,
            IRepository<HotelCode> hotelCodeRepository, 
            IRepository<FeaturedHotel> featuredHotelCodeRepository,
            IRepository<HotelStaff> hotelStaffRepository)
        {
            _hotelRepository = hotelRepository;
            _hotelMediaRepository = hotelMediaRepository;
            _hotelCodeRepository = hotelCodeRepository;
            _featuredHotelCodeRepository = featuredHotelCodeRepository;
            _hotelStaffRepository = hotelStaffRepository;
        }

        #endregion


        #region Private Fields

        private readonly IRepository<Hotel> _hotelRepository;
        private readonly IRepository<HotelMedia> _hotelMediaRepository;
        private readonly IRepository<HotelCode> _hotelCodeRepository;
        private readonly IRepository<FeaturedHotel> _featuredHotelCodeRepository;
        private readonly IRepository<HotelStaff> _hotelStaffRepository;
        #endregion


        #region Public Methods

        /// <summary>
        /// Gets the hotel code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public HotelCode GetHotelCode(string code)
        {
            return _hotelCodeRepository.Table.FirstOrDefault(c => c.CodeNumber.ToLower() == code.ToLower());
        }


        /// <summary>
        /// Gets the hotel.
        /// </summary>
        /// <param name="hotelId">The hotel id.</param>
        /// <returns></returns>
        public Hotel GetHotel(int hotelId)
        {
            return _hotelRepository.Table.FirstOrDefault(h => h.HotelID == hotelId);
        }

        public IList<HotelStaff> GetHotelStaffByCustId(int custId)
        {
            return _hotelStaffRepository.Table.Where(a => a.CustID == custId).ToList();
        }
        #endregion

        public void Dispose()
        {

        }
    }
}
