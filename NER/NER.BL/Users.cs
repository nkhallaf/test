using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NER.BL
{
    public class Users
    {
        public static int CheckLogin(string p, string p_2)
        {
            var entities = new DAL.NEREntities();
            return entities.Users.Where(x => x.UserName == p && x.Password == p_2).Select(x => x.ID).FirstOrDefault();

        }
    }
}
