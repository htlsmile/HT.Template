using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HT.Template.BackEnd.DTOs
{
    public class UserDTO
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 新密码
        /// </summary>
        public string NewPassword { get; set; }
    }
}
