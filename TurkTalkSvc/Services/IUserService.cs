using OLabWebAPI.Model;
using System.Collections.Generic;

namespace OLabWebAPI.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(LoginRequest model);
        AuthenticateResponse AuthenticateExternal(ExternalLoginRequest model);
        void ChangePassword(Users user, ChangePasswordRequest model);

        void AddUser(Users newUser);
        IEnumerable<Users> GetAll();
        Users GetById(int id);
        Users GetByUserName(string userName);

        // OLabUser Login(string userName, string password);
    }
}