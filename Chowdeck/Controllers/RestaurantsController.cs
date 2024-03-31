using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Chowdeck.Models;
using System.Drawing.Printing;
using System.Data.Entity;
using Microsoft.AspNetCore.Http.HttpResults;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Chowdeck.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RestaurantsController : Controller
    {
        private readonly ChowdeckContext _context;

        public RestaurantsController(ChowdeckContext context)
        {
            _context = context;
        }

        // GET: Restaurants
        [HttpGet("")]
        public IActionResult Restaurants(
            //IQueryCollection query
            )
        {
            int pageSize = 40;

            int page = 1;  // Default to page 1

            // Retrieve the "page" query parameter with error handling
            int.TryParse(HttpContext.Request.Query["page"].ToString() ?? "", out page);

            // If parsing failed or the value was non-positive, use the default
            if (page <= 0) page = 1;

            var totalCount = _context.Restaurants.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            List<Restaurant> restaurants = _context.Restaurants
                .Skip((page - 1) * pageSize)
                .Take(pageSize).ToList();

            var response = new
            {
                totalPages,
                totalCount,
                page,
                restaurants
            };

            return Ok(response);
        }

        [HttpGet("{restaurantId}/menus")]
        public IActionResult RestaurantMenus(string restaurantId)
        {
            Restaurant? restaurant = _context.Restaurants
                .FirstOrDefault(r => r.Id == restaurantId);

            if(restaurant == null) return NotFound();

            var response = new
            {
                menus = _context.RestaurantMenus.Where(m => m.RestaurantId == restaurantId)
                .Select(m => new { m.Id, m.Name, m.Category, 
                    m.CreatedAt, m.Image, Price = (decimal) m.Price })
                .ToList()
            };

            return Ok(response);
        }

    }
}
