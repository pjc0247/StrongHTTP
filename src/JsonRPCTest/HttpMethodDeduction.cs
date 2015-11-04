using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CsRestClient;
using CsRestClient.Attributes;

namespace CsRestClientTest
{
    [TestClass]
    public class HttpMethodDeduction
    {
        [AutoHttpMethod]
        interface TestModel
        {
            void GetUser();
            void CreateUser();
            void UpdateUser();
            void DeleteUser();
        }
        interface TestModel2
        {
            void GetUser();
            void CreateUser();
            void UpdateUser();
            void DeleteUser();
        }

        [TestMethod]
        public void DeductionByName()
        {
            var intf = typeof(TestModel);

            Assert.AreEqual(
                HttpMethod.Get,
                intf.GetMethod("GetUser").GetHttpMethod());
            Assert.AreEqual(
                HttpMethod.Post,
                intf.GetMethod("CreateUser").GetHttpMethod());
            Assert.AreEqual(
                HttpMethod.Put,
                intf.GetMethod("UpdateUser").GetHttpMethod());
            Assert.AreEqual(
                HttpMethod.Delete,
                intf.GetMethod("DeleteUser").GetHttpMethod());
        }

        [TestMethod]
        public void DeductionByAttribute()
        {

        }
    }
}
