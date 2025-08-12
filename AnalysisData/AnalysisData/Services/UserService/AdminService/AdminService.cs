﻿using AnalysisData.Dtos.UserDto.UserDto;
using AnalysisData.Exception.RoleException;
using AnalysisData.Exception.UserException;
using AnalysisData.Models.UserModel;
using AnalysisData.Repositories.RoleRepository.Abstraction;
using AnalysisData.Repositories.UserRepository.Abstraction;
using AnalysisData.Services.JwtService.Abstraction;
using AnalysisData.Services.UserService.AdminService.Abstraction;
using AnalysisData.Services.ValidationService.Abstraction;

namespace AnalysisData.Services.UserService.AdminService;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidationService _validationService;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtService _jwtService;

    public AdminService(IUserRepository userRepository, IValidationService validationService,
        IRoleRepository roleRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _validationService = validationService;
        _roleRepository = roleRepository;
        _jwtService = jwtService;
    }

    public async Task UpdateUserInformationByAdminAsync(Guid id, UpdateAdminDto updateAdminDto)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user !=null && user.Username == "admin")
        {
            throw new AdminProtectedException();
        }
        await ValidateUserInformation(user, updateAdminDto);
        _validationService.EmailCheck(updateAdminDto.Email);
        _validationService.PhoneNumberCheck(updateAdminDto.PhoneNumber);
        await CheckExistenceOfRole(user, updateAdminDto);
        await SetUpdatedInformation(user, updateAdminDto);
    }

    private async Task ValidateUserInformation(User user, UpdateAdminDto updateAdminDto)
    {
        var checkUsername = await _userRepository.GetUserByUsernameAsync(updateAdminDto.UserName);
        var checkEmail = await _userRepository.GetUserByEmailAsync(updateAdminDto.Email);
        var checkPhoneNumber = await _userRepository.GetUserByPhoneNumberAsync(updateAdminDto.PhoneNumber);
        if ((checkUsername != null && !user.Equals(checkUsername)) || (checkEmail != null && !user.Equals(checkEmail)) || (checkPhoneNumber != null && !user.Equals(checkPhoneNumber)))
            throw new DuplicateUserException();
    }

    private async Task CheckExistenceOfRole(User user, UpdateAdminDto updateAdminDto)
    {
        var role = await _roleRepository.GetRoleByNameAsync(updateAdminDto.RoleName);
        if (role == null)
        {
            throw new RoleNotFoundException();
        }
        await SetUpdatedInformation(user, updateAdminDto);
    }

    private async Task SetUpdatedInformation(User user, UpdateAdminDto updateAdminDto)
    {
        user.FirstName = updateAdminDto.FirstName;
        user.LastName = updateAdminDto.LastName;
        user.Email = updateAdminDto.Email;
        user.PhoneNumber = updateAdminDto.PhoneNumber;
        user.Username = updateAdminDto.UserName;
        var role = await _roleRepository.GetRoleByNameAsync(updateAdminDto.RoleName);
        if (role is null)
        {
            throw new RoleNotFoundException();
        }
        user.RoleId = role.Id;
        await _userRepository.UpdateUserAsync(user.Id, user);
    }


    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user != null && user.Username == "admin")
        {
            throw new AdminProtectedException();
        }
        var isDelete = await _userRepository.DeleteUserAsync(id);
        if (!isDelete)
            throw new UserNotFoundException();
        return true;
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _userRepository.GetUsersCountAsync();
    }

    public async Task<List<UserPaginationDto>> GetAllUserAsync(int page, int limit)
    {
        var users = await _userRepository.GetAllUserPaginationAsync(page, limit);
        var paginationUsers = users.Select(x => new UserPaginationDto()
        {
            Guid = x.Id.ToString(), UserName = x.Username, FirstName = x.FirstName, LastName = x.LastName,
            Email = x.Email, PhoneNumber = x.PhoneNumber, RoleName = x.Role.RoleName
        });
        return paginationUsers.ToList();
    }
}