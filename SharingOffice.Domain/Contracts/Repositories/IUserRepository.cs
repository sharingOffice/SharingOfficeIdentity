using System;
using System.Threading.Tasks;
using SharingOffice.Domain.models;

namespace SharingOffice.Domain.Contracts.Repositories
{
    public interface IUserRepository
    {
        void Update(User account);
        Task<User> Get(string token);
        Task<User> GetByEmail(string emailAddress);
    }
}