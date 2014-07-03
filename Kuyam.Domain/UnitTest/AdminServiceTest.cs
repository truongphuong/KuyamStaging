using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository.Interface;
using NUnit.Framework;
using Rhino.Mocks;

namespace Kuyam.Domain.UnitTest
{
    [TestFixture]
    class AdminServiceTest
    {
        private List<AccessKeyManagement> listAccessKeyManagement;
        private AdminService adminService;

        [SetUp]
        public void SetUp()
        {
            listAccessKeyManagement = new List<AccessKeyManagement>();
            for (int i = 0; i < 10; i++)
            {
                AccessKeyManagement akm = new AccessKeyManagement()
                                              {
                                                  AccessKeyID = i + 1,
                                                  Active = true,
                                                  EmailAdmin = string.Format("testAdmin_{0}@gmail.com", i + 1),
                                                  EmailUser = string.Format("testUser_{0}@gmail.com", i + 1),
                                                  Key = string.Format("{0}", 1001 + i)
                                              };
                listAccessKeyManagement.Add(akm);
            }
            var mockAccessKeyRepository = MockRepository.GenerateMock<IRepository<AccessKeyManagement>>();
            mockAccessKeyRepository.Stub(x => x.Table).Return(listAccessKeyManagement.AsQueryable());
            mockAccessKeyRepository.Stub(x => x.Delete(Arg<AccessKeyManagement>.Is.Anything)).Callback(
                delegate(AccessKeyManagement item)
                    {
                        if (listAccessKeyManagement.Any(a => a.AccessKeyID == item.AccessKeyID))
                        {
                            listAccessKeyManagement.Remove(item);
                        }
                        return true;
                    });

            adminService = new AdminService(mockAccessKeyRepository, null, null, null, null, null, null, null, null, null, null, null, null, null);
        }

        [TearDown]
        public void CleanUp()
        {
            
        }
        
        [Test]
        public void Test_GetAccessKeyManagementByUserAndkey_Success_HaveData()
        {
            var item = adminService.GetAccessKeyManagementByUserAndkey("testUser_1@gmail.com", "1001");
            Assert.IsNotNull(item);
            Assert.AreEqual(item.Active, true);
        }

        [Test]
        public void Test_GetAccessKeyManagementByUserAndkey_Success_NoData()
        {
            var item = adminService.GetAccessKeyManagementByUserAndkey("testUser_11@gmail.com", "1001");
            Assert.IsNull(item);
        }

        [Test]
        public void Test_GetAccessKeyManagementByAdminAndkey_Success_HaveData()
        {
            var item = adminService.GetAccessKeyManagementByAdminAndkey("testAdmin_1@gmail.com");
            Assert.IsNotNull(item);
            Assert.AreEqual(item.Count, 1);
        }

        [Test]
        public void Test_GetAccessKeyManagementByAdminAndkey_Success_NoData()
        {
            var item = adminService.GetAccessKeyManagementByAdminAndkey("testAdmin_11@gmail.com");
            Assert.IsNotNull(item);
            Assert.AreEqual(item.Count, 0);
        }

        [Test]
        public void Test_DeleteAccessKeyManagement_Success()
        {
            adminService.DeleteAccessKeyManagement(1);
            Assert.AreEqual(listAccessKeyManagement.Count, 9);

        }
    }
}
