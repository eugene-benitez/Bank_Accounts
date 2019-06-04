using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Bank_Accounts.Models
{
    public class LoginUser
    {
        // No other fields!
        public string Email { get; set; }
        public string Password { get; set; }
    }
}