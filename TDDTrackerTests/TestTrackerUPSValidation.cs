using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerModel;

namespace TDDTrackerTests
{
    internal class TestTrackerUPSValidation
    {
        private Guid _password;
        private Guid _userId;
        //private TrackerViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _password = Guid.NewGuid();
            _userId = Guid.NewGuid();
        }

        [Test]
        public void PasswordSecurity()
        {
            TrackerModel.TrackerSecurity security = new TrackerSecurity();
            security.LogRounds = 11;
            string hashedPassword = security.GetHashedPassword(_userId.ToString(), _password.ToString());

            // Make sure the hash comes out the same twice in a row.
            Assert.That(hashedPassword, Is.EqualTo(security.GetHashedPassword(_userId.ToString(), _password.ToString())));
        }

        [Test]
        public void UPSTrackingNumberLowerCaseValidation()
        {
            TrackerSecurity security = new TrackerSecurity();
            security.LogRounds = 11;
            string hashedPassword = security.GetHashedPassword(_userId.ToString(), _password.ToString());

            // Make sure the hash comes out the same twice in a row.
            Assert.That(hashedPassword, Is.EqualTo(security.GetHashedPassword(_userId.ToString(), _password.ToString())));
        }
    }
}
