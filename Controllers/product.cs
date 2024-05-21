using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApi.dataasset;
using TodoApi.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly applicationContext _context;
        private readonly IConfiguration _config;

        public ProductController(applicationContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private string GenerateJSONWebToken(string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost("login/User")]
        public IActionResult LoginUser()
        {
            var username = "testuser";
            var password = "password";

            // Validate the hard-coded username and password
            if (username == "testuser" && password == "password")
            {
                var tokenString = GenerateJSONWebToken("user");
                return Ok(new { tokenString });
            }

            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("login/Admin")]
        public IActionResult LoginAdmin()
        {
            var username = "testadmin";
            var password = "password";

            // Validate the hard-coded username and password
            if (username == "testadmin" && password == "password")
            {
                var tokenString = GenerateJSONWebToken("admin");
                return Ok(new { tokenString });
            }

            return Unauthorized();
        }

        [HttpGet("product")]
        [Authorize(Roles = "user,admin")]
        public IActionResult Get()
        {
            var products = _context.Products.ToList();
            return Ok(products);
        }

        [HttpGet("product/{id}")]
        [Authorize(Roles = "user,admin")]
        public IActionResult GetById([FromRoute] int id)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost("product")]
        [Authorize(Roles = "admin")]
        public IActionResult Post([FromBody] Product product)
        {
            _context.Add(product);
            _context.SaveChanges();
            return Ok(product);
        }

        [HttpDelete("product/{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteById([FromRoute] int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            _context.Remove(product);
            _context.SaveChanges();
            return Ok("ok");
        }

        [HttpPut("product/{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult PutById([FromRoute] int id, [FromBody] Product updatedProduct)
        {
            if (id != updatedProduct.Id)
            {
                return BadRequest();
            }
            _context.Entry(updatedProduct).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(updatedProduct);
        }
    }
}
