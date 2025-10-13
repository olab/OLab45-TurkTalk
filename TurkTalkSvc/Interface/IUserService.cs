using OLab.Api.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TurkTalkSvc.Interface;

public interface IUserService
{
  //Users Authenticate(LoginRequest model);
  void ChangePassword(Users user, ChangePasswordRequest model);

  Task<List<AddUserResponse>> AddUsersAsync(List<AddUserRequest> items);
  Task<List<AddUserResponse>> DeleteUsersAsync(List<AddUserRequest> items);
  Task<AddUserResponse> AddUserAsync(AddUserRequest item);

  IEnumerable<Users> GetAll();
  Users GetById(int id);
  Users GetByUserName(string userName);
}