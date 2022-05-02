using System;

namespace BusinessLogic
{
    public class UserInfo
    {
        private string _userId;
        private string _password;
        private string _zipcode;

        public UserInfo(string userId, string password, string zipcode)
        {
            _userId = userId;
            _password = password;
            _zipcode = zipcode;
        }

    }
}
