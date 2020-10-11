using System;
using Microsoft.AspNetCore.Identity;

namespace SharingOffice.Domain.models
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string Description { get; set; }
    }
}