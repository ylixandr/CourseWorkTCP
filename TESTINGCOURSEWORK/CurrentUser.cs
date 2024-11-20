using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TESTINGCOURSEWORK
{
    public static class CurrentUser
    {
        public static int UserId { get; set; }
        public static string UserName { get; set; }

        public static void SetUser(int userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }

        public static void Clear()
        {
            UserId = 0;
            UserName = null;
        }
    }

}
