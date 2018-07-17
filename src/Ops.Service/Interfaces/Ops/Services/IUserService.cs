﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IUserService
    {
        Task<User> LookupUser(string username);
        Task<User> AddUser(User user, int? createdById = null);
        Task<User> EnsureSysadminUserAsync();
        Task<User> GetByIdAsync(int id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> EditNicknameAsync(User user);
        Task LoggedInAsync(string username);
    }
}