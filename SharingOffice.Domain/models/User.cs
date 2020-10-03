using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SharingOffice.Domain.models
{
    public class User : IdentityUser<Guid>
    {
        
        public  string FullName {
            get
            {
                return FirstName + " " + LastName;
            }
        }


        [StringLength(32)]
        public string FirstName { get; set; }

        [StringLength(32)]
        public  string LastName { get; set; }
        public Role Role { get; set; }
        
        public string ResetToken { get; set; }
       
        public DateTime? ResetTokenExpires { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }

        public string OAuthSubject { get; set; }
        public string OAuthIssuer { get; set; }
        
        public bool OwnsToken(string token) 
        {
            return this.RefreshTokens?.Find(x => x.Token == token) != null;
        }

        public User()
        {
            RefreshTokens = new List<RefreshToken>();
        }
        
    }
}