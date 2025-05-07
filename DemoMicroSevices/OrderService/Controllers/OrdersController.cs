using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Messaging;
using OrderService.Models;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly OrderPublisher _publisher;
        private readonly HttpClient _client;

        public OrdersController(OrderDbContext context, OrderPublisher orderPublisher)
        {
            _context = context;
            _publisher = orderPublisher;
            _client = new HttpClient();
        }

        // GET: api/Orders
        [HttpGet("product/{id}")]
        public async Task<Product> GetProductAsync(Guid id)
        {

            var resp = await _client.GetAsync($"http://apigateway:/8080/products/{id}");
            resp.EnsureSuccessStatusCode();
            var prod = await resp.Content.ReadFromJsonAsync<Product>();
            return prod!;
            //return await _context.Orders.ToListAsync();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>>GetOrder(Guid id)
        {
            var order = await _context.Orders.SingleOrDefaultAsync(o=>o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await _publisher.PublishOrderAsync(order);
                return CreatedAtAction(nameof(Order), new {id =order.Id},order);
            }
            else
            {
                return BadRequest();
            }
        }


    }
}
