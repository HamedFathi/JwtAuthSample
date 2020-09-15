using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtAuthSample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _configuration;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login(string userName, string password)
        {
            User login = new User();
            login.UserName = userName;
            login.Password = password;
            IActionResult response = Unauthorized();

            var user = AuthenticateUser(login);

            if(user != null)
            {
                var token = GenerateJwt(user);
                response = Ok(new
                {
                    token
                });
            }

            return response;
        }

        private User AuthenticateUser(User login)
        {
            User user = null;
            if (login.UserName == "hamed" && login.Password == "1234")
            {
                user = new User
                {
                    UserName = "hamed",
                    Password = "1234",
                    EmailAddress = "hamed@gmail.com"
                };
            }
            return user;
        }

        private string GenerateJwt(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                new Claim(JwtRegisteredClaimNames.Email,user.EmailAddress),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodedToken;
        }
    }
}
