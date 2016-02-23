using Microsoft.VisualStudio.TestTools.UnitTesting;
using DOJ;

namespace DojTest
{
    [TestClass]
    public class DojSecurityManagerTest
    {
        [TestMethod]
        public void TestCrypting()
        {
            var sm = new DojSecurityManager();
            const string plain = "this is a string to be fucked up";
            var enc = sm.Encrypt(plain);

            var sm2 = new DojSecurityManager();
            var dec = sm2.Decrypt(enc);
            Assert.AreEqual(plain, dec);

            var user = new User("testuser", "testpw");
            var line = user.ToString();
            var cipher = sm.Encrypt(line);
            var roundLine = sm2.Decrypt(cipher);
            var roundTrip = User.FromString(roundLine);
            Assert.IsTrue(user.Equals(roundTrip));
        }
    }
    [TestClass]
    public class AdministrationTest
    {
        [TestMethod]
        public void TestGetUserbaseDir()
        {
//            Assert.Fail(Administration.GetUserbaseDir());
        }
    }
}
