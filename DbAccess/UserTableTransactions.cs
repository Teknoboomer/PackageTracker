using System;
using System.Data;
using System.Data.SqlClient;

namespace UserTableAccess
{
    public class UserTableAccess
    {
        public bool CreateUser(string userId, string password, string zipcode)
        {
            bool success = false;

            Console.WriteLine("UserId: " + userId + " Password: " + password + " Zip Code: " + zipcode);

            string connetionString = @"Data Source =LAPTOP-LT3PRTBM;Initial Catalog=USPSTracking;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connetionString);
            connection.Open();
            int i = password.Length;
            using (SqlCommand cmd = new SqlCommand("CreateUser", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.Parameters.AddWithValue("@user_password", password);
                cmd.Parameters.AddWithValue("@zipcode", zipcode);

                try
                {
                    int rowsAffected = cmd.ExecuteNonQuery();
                    success = rowsAffected > 0;
                }
                catch (SqlException e)
                {
                    success = false;
                }
            }

            return success;
        }

        public bool CheckUser(string userId, string password, out DateTime lastLogin)
        {
            Console.WriteLine("UserId: " + userId + " Password: " + password);

            bool success = false;
            lastLogin = DateTime.Now;

            string connetionString = @"Data Source =LAPTOP-LT3PRTBM;Initial Catalog=USPSTracking;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connetionString);
            connection.Open();

            using (SqlCommand cmd = new SqlCommand("CheckUser", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.Parameters.AddWithValue("@user_password", password);

                try
                {
                    SqlDataReader sqlDr = cmd.ExecuteReader();
                    while (sqlDr.Read())
                    {
                        success = true;
                        Object lastLogin2 = sqlDr["LastLogin"];
                        Console.WriteLine(
                            "ID: {0,-25} Name: {1,6}",
                            sqlDr["UserId"],
                            sqlDr["LastLogin"]);
                    }
                }
                catch (SqlException e)
                {
                    success = false;
                }
            }
            // https://fkkland.in.net/ https://sunnyday.in.net/tags/young/page/26/ https://family-nudism.pw/page/2/ https://garilas.site/ https://nudism-beauty.com/tags/photo https://onlynud.site/ https://viphentai.club/page/235/ https://secrethentai.club/page/130/

            return success;
        }
    }
}
