using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JenkinsAccess;

namespace libJenkinsAccess_t
{
    [TestClass]
    public class __utils_t
    {
        [TestMethod]
        public void ExtractsJobCorrectly()
        {
            var v = new Uri("http://jenks-higgs.phys.washington.edu:8080/view/LLP/job/CalR-JetMVATraining/").JenkinsJobName();
            Assert.IsTrue(v.IsSome);
            v.IfSome(s => Assert.AreEqual("CalR-JetMVATraining", s));
        }

        [TestMethod]
        public void BadJobString()
        {
            var v = new Uri("http://jenks-higgs.phys.washington.edu:8080/view/LLP").JenkinsJobName();
            Assert.IsTrue(v.IsNone);
        }
    }
}
