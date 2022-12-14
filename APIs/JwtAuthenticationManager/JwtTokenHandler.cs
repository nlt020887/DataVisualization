using JwtAuthenticationManager.Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Net.Http;

namespace JwtAuthenticationManager
{
    public class JwtTokenHandler
    {
        public const string JWT_SECURITY_KEY = "HSko5EsTIfHoBhrc4sgmKC5vXKymRnb04viu24bBsfC9ewh7HVtIMaK8AL94JP9CXZAoCdxvP6Ubul1zOszZaOAtgEV0MTy5lRbMymO9FEcNUEff4kVmj3mf7EjPTZGRfFgv/6!!@#$%z&*v";
        public const string ISSUE_URL = "http://localhost:8080/";
        private const int JWT_TOKEN_VALIDITY_MINS = 1440;      

        public async Task<AuthenticationResponse?> GenerateJwtToken(AuthenticationRequest authenticationRequest,IdentityUser identityUser, List<string> roles)
        {
            if (string.IsNullOrWhiteSpace(authenticationRequest.UserName) || string.IsNullOrWhiteSpace(authenticationRequest.Password))
                return null;

            /* Validation */
            var userAccount = identityUser;//await _userAccountList.Where(x => x.UserName == authenticationRequest.UserName && x.Password == authenticationRequest.Password).FirstOrDefault();
            if (userAccount == null) return null;

            var tokenExpiryTimeStamp = DateTime.Now.AddMinutes(JWT_TOKEN_VALIDITY_MINS);
            var tokenKey = Encoding.ASCII.GetBytes(JWT_SECURITY_KEY);
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, authenticationRequest.UserName)
                //,
                //new Claim(JwtRegisteredClaimNames.Email, authenticationRequest.Email)
            });
            if (roles != null && roles.Count >0)
            {
                foreach (var item in roles)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role,item));
                }
            }

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey),
                SecurityAlgorithms.HmacSha256Signature);

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = tokenExpiryTimeStamp,
                SigningCredentials = signingCredentials
            };

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            var token = jwtSecurityTokenHandler.WriteToken(securityToken);
            

            return new AuthenticationResponse
            {
                UserName = userAccount.UserName,
                ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.Now).TotalSeconds,
                JwtToken = token
            };
        }
    }
}
