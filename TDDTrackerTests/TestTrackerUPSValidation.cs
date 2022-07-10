using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerModel;
using TrackerVM;

namespace TDDTrackerTests
{
    internal class TestTrackerUPSValidation
    {
        private Guid _password;
        private Guid _userId;
        private TrackerViewModel _viewModel;

                
        [SetUp]
        public void Setup()
        {
            _password = Guid.NewGuid();
            _userId = Guid.NewGuid();
            _viewModel = new TrackerViewModel();
        }

        [Test]
        public void PasswordSecurity()
        {
            TrackerSecurity security = new TrackerSecurity();
            security.LogRounds = 11;
            string hashedPassword = security.GetHashedPassword(_userId.ToString(), _password.ToString());

            // Make sure the hash comes out the same twice in a row.
            Assert.That(hashedPassword, Is.EqualTo(security.GetHashedPassword(_userId.ToString(), _password.ToString())));
        }

        [
            Test]
        public void UPSTrackingNumberGoodValidation()
        {
            string upsTrackingNumber = "1Z9e79559027281578";
            bool isValid = TrackerViewModel.IsvalidUPSCheckDigit(upsTrackingNumber);
            // Make sure the hash comes out the same twice in a row.
            Assert.That(isValid, Is.EqualTo(true));
        }

        [Test]
        public void UPSTrackingNumberLowerCaseValidation()
        {
            string upsTrackingNumber = "1z9e79559027281578";
            bool isValid = TrackerViewModel.IsvalidUPSCheckDigit(upsTrackingNumber);
            // Make sure the hash comes out the same twice in a row.
            Assert.That(isValid, Is.EqualTo(true));
        }

        [Test]
        public void UPSTrackingNumberBadValidation()
        {
            string upsTrackingNumber = "1Z9e79559027281573";
            bool isValid = TrackerViewModel.IsvalidUPSCheckDigit(upsTrackingNumber);
            // Make sure the hash comes out the same twice in a row.
            Assert.That(isValid, Is.EqualTo(false));
        }
    }
}
