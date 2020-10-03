using System;
using System.Net.Http;

namespace SharingOffice.Domain.Exceptions
{
    public class AppException: Exception
    {
        public AppException(string message)
            : base(message)
        {
            
        }
    }
}