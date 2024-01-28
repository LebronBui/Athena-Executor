using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using RealEstateAuction.Models;

namespace RealEstateAuction.DAL
{
    public class UserDAO
    {
        private readonly RealEstateContext context;
        public UserDAO()
        {
            context = new RealEstateContext();
        }
        public User GetUserByEmail(string email)
        {
            return context.Users.SingleOrDefault(u => u.Email.Equals(email));
        }
        public User GetUserById(int id)
        {
            return context.Users.SingleOrDefault(u => u.Id.Equals(id));
        }
        public User GetUserByEmailAndPassword(string email, string password)
        {
            return context.Users.SingleOrDefault(u => u.Email.Equals(email) && u.Password.Equals(password));
        }
        public bool AddUser(User user)
        {
            try
            {
                context.Users.Add(user);
                context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool UpdateUser(User user)
        {
            try
            {
                context.Users.Update(user);
                context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool DeleteUser(User user)
        {
            try
            {
                context.Users.Remove(user);
                context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdatePassword(string email, string newPwd)
        {
            try
            {
                var user = GetUserByEmail(email);
                user.Password = newPwd;
                context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
