using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharingOffice.Domain.Contracts.Repositories;
using SharingOffice.Domain.models;
using SharingOffice.Infra.DbContexts;

namespace SharingOffice.Infra.Repositories
{
    public class UserRepository : IUserRepository
    {
        private SharringOfficeDbContext _dbContext;

        public UserRepository(SharringOfficeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(User user)
        {
            _dbContext.Users.Update(user);
            _dbContext.SaveChanges();
        }

        public async Task<User> Get(string token)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(q => q
                    .RefreshTokens
                    .Any(a => a.Token == token));

            if (user != null)
            {
                await _dbContext.Entry(user)
                    .Collection(q => q.RefreshTokens).LoadAsync();
            }

            return user;
        }

        public async Task<User> GetByEmail(string emailAddress)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(q => q.Email == emailAddress);
            return user;
        }
    }
}