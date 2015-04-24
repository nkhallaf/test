using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NER.DAL;

namespace NER.BL
{
    public class Users
    {
        public static int CheckLogin(string p, string p_2)
        {
            var entities = new DAL.NEREntities();
            return entities.Users.Where(x => x.UserName == p && x.Password == p_2).Select(x => x.ID).FirstOrDefault();

        }

        public static bool IsAdmin(int id)
        {
            var entities = new DAL.NEREntities();
            return entities.Users.Where(x => x.ID == id).Select(x => x.IsAdmin).FirstOrDefault();

        }

        public static List<BL.Users> GetAll()
        {
            var entities = new DAL.NEREntities();
            return entities.Users.Select(x => new BL.Users { Name = x.UserName, Password = x.Password, Admin = x.IsAdmin, ID = x.ID }).ToList();


        }

        public string Name { get; set; }

        public string Password { get; set; }
        public bool Admin { get; set; }

        public int ID { get; set; }

        public static BL.Users GetUser(int Id)
        {
            var entities = new DAL.NEREntities();
            return entities.Users.Where(x => x.ID == Id).Select(x => new BL.Users { Name = x.UserName, Password = x.Password, Admin = x.IsAdmin, ID = x.ID }).First();

        }

        public static void Add(string mail, string pass, bool admin)
        {
            var entities = new DAL.NEREntities();
            entities.Users.AddObject(new User
            {
                IsAdmin = admin,
                UserName = mail,
                Password = pass
            });
            entities.SaveChanges();
        }

        public static void Update(int ID, string mail, string pass, bool admin)
        {
            var entities = new DAL.NEREntities();
            var itemToUpdate = entities.Users.Where(x => x.ID == ID).First();

            itemToUpdate.IsAdmin = admin;
            itemToUpdate.UserName = mail;
            itemToUpdate.Password = pass;

            entities.SaveChanges();
        }

        public static void DeleteUser(int ID)
        {
            var entities = new DAL.NEREntities();
            var itemToDelete = entities.Users.Where(x => x.ID == ID).First();
            entities.Users.DeleteObject(itemToDelete);

            entities.SaveChanges();
        }
    }
}
