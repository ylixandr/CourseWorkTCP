using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public class AccountViewModel
    {
        public int Id { get; set; }          // Уникальный идентификатор пользователя
        public string Login { get; set; }   // Логин пользователя
        public string Password { get; set; } // Пароль пользователя
        public int RoleId { get; set; }     // ID роли пользователя
    }

}
