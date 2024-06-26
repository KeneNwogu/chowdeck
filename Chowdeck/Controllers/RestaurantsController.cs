﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Chowdeck.Models;
using System.Drawing.Printing;
//using System.Data.Entity;
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
            string category = HttpContext.Request.Query["category"].ToString() ?? "";
            string search = HttpContext.Request.Query["search"].ToString() ?? "";

            int pageSize = 40;

            if (category.Length > 0) pageSize = 10;

            int page = 1;  // Default to page 1

            // Retrieve the "page" query parameter with error handling
            int.TryParse(HttpContext.Request.Query["page"].ToString() ?? "", out page);

            // If parsing failed or the value was non-positive, use the default
            if (page <= 0) page = 1;

            var totalCount = _context.Restaurants.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var query = _context.Restaurants.AsQueryable();

            if (category.Length > 0)
            {
                query = query.Where(r => EF.Functions.Like(r.Category.ToLower(), $"%{category.ToLower()}%"));
            }

            if(search.Length > 0)
            {
                query = query.Where(r =>
                    EF.Functions.Like(r.Category.ToLower(), $"%{search.ToLower()}%") ||
                    EF.Functions.Like(r.Name.ToLower(), $"%{search.ToLower()}%"));
            }

            List<Restaurant> restaurants = query
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
            var restaurant = _context.Restaurants
                .FirstOrDefault(r => r.Id ==  restaurantId);

            if(restaurant == null) return NotFound();

            var response = new
            {
                restaurant,
                menus = _context.RestaurantMenus.Where(m => m.RestaurantId == restaurantId)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Category,
                    m.CreatedAt,
                    m.Image,
                    Price = (decimal)m.Price
                })
                .ToList()
            };

            return Ok(response);
        }

    }
}
