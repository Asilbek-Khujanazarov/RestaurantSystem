using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Domain.Entities;
using RestaurantManagementSystem.infrastructure.Data;

namespace RestaurantManagementSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly RestaurantDbContext _context;
        public OrderController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpPost("[Action]")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
            {
                return BadRequest("Invalid order data.");
            }

            var order = new Order
            {
                CustomId = int.Parse(Generate5DigitId()),
                OrderName = request.OrderName,
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };

            decimal totalOrderPrice = 0;

            foreach (var item in request.Items)
            {
                var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Name == item.MenuName);
                if (menu == null)
                {
                    return BadRequest($"Menu item '{item.MenuName}' not found.");
                }

                var size = menu.Sizes.FirstOrDefault(s => s.Key == item.SizeKey);
                if (size.Key == null)
                {
                    return BadRequest($"Size '{item.SizeKey}' not found for menu item '{item.MenuName}'.");
                }

                var price = size.Value;
                var totalItemPrice = price * item.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    MenuName = item.MenuName,
                    Quantity = item.Quantity,
                    Price = price,
                    OrderId = order.Id,
                    CustomId = order.CustomId
                    
                });
                if ((menu.QuantityProduct < 1 && menu.Considered == true) || menu.IsPresent == false)
                {
                    menu.IsPresent = false;
                    return BadRequest("Vaqtinchalik mavjud emas!");
                }

                totalOrderPrice += totalItemPrice;
            }

            order.TotalPrice = totalOrderPrice;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order created successfully", orderId = order.Id, customerId = order.CustomId });
        }
        private string Generate5DigitId()
        {
            Random random = new Random();
            int newId;
            do
            {
                newId = random.Next(10000, 100000);
            }
            while (_context.Orders.Any(s => s.CustomId == newId));

            return newId.ToString();
        }


        [HttpGet("[Action]/id")]
        public async Task<ActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.CustomId == id);

            if (order == null)
            {
                return NotFound(new { message = $"{id}: Buyurtma topilmadi" });
            }

            return Ok(new
            {
                id = order.Id,
                customId = order.CustomId,
                orderName = order.OrderName,
                totalPrice = order.TotalPrice,
                process = order.Process,
                orderDate = order.OrderDate,
                orderItems = order.OrderItems.Select(item => new
                {
                    orderId = item.OrderId,
                    menuName = item.MenuName,
                    quantity = item.Quantity,
                    price = item.Price,
                    totalItemPrice = item.TotalItemPrice
                })
            });
        }


        [HttpPut("[Action]")]
        public IActionResult UpdateOrder(int orderId, [FromBody] UpdateOrderRequestDto updateOrderRequestDto)
        {
            var existingOrder = _context.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == orderId);
            if (existingOrder == null)
            {
                return NotFound(new { message = "Buyurtma topilmadi!" });
            }

            var timeDifference = DateTime.Now - existingOrder.OrderDate;
            if (timeDifference.TotalMinutes > 5)
            {
                return BadRequest("Buyurtma 5 daqiqadan ko'proq vaqt o'tgach o'zgartrib bo'lmaydi.");
            }

            // Eski buyurtma elementlarini o'chirish
            _context.OrderItems.RemoveRange(existingOrder.OrderItems);

            var newOrderItems = new List<OrderItem>();
            foreach (var item in updateOrderRequestDto.Items)
            {
                var menu = _context.Menus.FirstOrDefault(m => m.Name == item.MenuName);
                if (menu == null)
                {
                    return BadRequest(new { message = $"'{item.MenuName}' menyu ma'lumotlar bazasida topilmadi!" });
                }

                var size = menu.Sizes.FirstOrDefault(s => s.Key == item.SizeKey);
                if (size.Key == null)
                {
                    return BadRequest(new { message = $"'{item.SizeKey}' o'lchami '{item.MenuName}' menyusi uchun topilmadi!" });
                }

                newOrderItems.Add(new OrderItem
                {
                    OrderId = orderId,
                    MenuName = menu.Name,
                    Quantity = item.Quantity,
                    Price = size.Value
                });
            }

            existingOrder.TotalPrice = newOrderItems.Sum(oi => oi.Quantity * oi.Price);

            _context.OrderItems.AddRange(newOrderItems);

            if (!string.IsNullOrEmpty(updateOrderRequestDto.OrderName))
            {
                existingOrder.OrderName = updateOrderRequestDto.OrderName;
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = "Buyurtma muvaffaqiyatli yangilandi",
                orderId = existingOrder.Id,
                customId = existingOrder.CustomId,
                orderName = existingOrder.OrderName,
                totalPrice = existingOrder.TotalPrice,
                items = newOrderItems.Select(oi => new
                {
                    menuName = oi.MenuName,
                    quantity = oi.Quantity,
                    price = oi.Price,
                    totalItemPrice = oi.TotalItemPrice
                })
            });
        }


        [HttpDelete("[Action]")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var delOrder = await _context.Orders.FirstOrDefaultAsync(o=> o.CustomId == id);
            if (delOrder == null)
            {
                return NotFound("Buyurtmangiz topilmadi");
            }
            var timeDifference = DateTime.Now - delOrder.OrderDate;
            if (timeDifference.TotalMinutes > 5)
            {
                return BadRequest("Buyurtma 5 daqiqadan ko'proq vaqt o'tgach Bekor qilinmaydi.");
            }
            _context.Orders.Remove(delOrder);
            await _context.SaveChangesAsync();
            return Ok($"Buyurtma {delOrder.OrderName}bekor qilindi.");
        }


        [HttpDelete("[Action]")]
        public async Task<IActionResult> DeletePreparedOrder(int id)
        {
            var delPreparedOrder = await _context.Orders.FirstOrDefaultAsync(o=> o.CustomId == id);
            if (delPreparedOrder == null)
            {
                return NotFound("Buyurtmangiz topilmadi");
            };
            var archivedOrder = new ArchevedOrder
            {                
                OrderName = delPreparedOrder.OrderName,
                TotalPrice = delPreparedOrder.TotalPrice,
                OrderDate = delPreparedOrder.OrderDate,
                Process = delPreparedOrder.Process
            };


            _context.ArchevedOrders.Add(archivedOrder);
            _context.Orders.Remove(delPreparedOrder);
            await _context.SaveChangesAsync();
            return Ok($"Buyurtma topshirildi.");


        }

        [HttpPost("[Action]")]
        [Authorize(Roles = "Staff,GeniralStaff")]
        public IActionResult SetProcessTrue(int id)
        {
            try
            {
                var order = _context.Orders.FirstOrDefault(o => o.Id == id);

                if (order == null)
                {
                    return NotFound($"ID = {id} bo'lgan buyurtma topilmadi.");
                }
                order.Process = false;
                _context.SaveChanges();

                return Ok($"Order ID = {id} ga Tayyorlanyabdi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Serverda xatolik yuz berdi: {ex.Message}");
            }
        }

        [HttpPost("[Action]")]
        [Authorize(Roles = "Staff,GeniralStaff")]
        public IActionResult SetPreparedTrue(int id)
        {
            try
            {
                var order = _context.Orders.FirstOrDefault(o => o.Id == id);

                if (order == null)
                {
                    return NotFound($"ID = {id} bo'lgan buyurtma topilmadi.");
                }
                order.Process = true;
                _context.SaveChanges();

                return Ok($"Order ID = {id} ga tayyor  bo'ldi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Serverda xatolik yuz berdi: {ex.Message}");
            }
        }
        //Kutilyotganlar 
        [HttpGet("[Action]")]
        public IActionResult GetExpectedingOrders()
        {
            try
            {
                var expectedingOrders = _context.Orders.Where(o => o.Process == null).ToList();
                var orderIds = expectedingOrders.Select(o => o.Id).ToList();

                return Ok(orderIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Serverda xatolik yuz berdi: {ex.Message}");
            }
        }
        [HttpGet("[Action]")]
        public IActionResult GetProcessingOrders()
        {
            try
            {
                var processedingOrders = _context.Orders.Where(o => o.Process == false).ToList();
                if (!processedingOrders.Any())
                {
                    return NotFound("Burutma mavjud emas.");
                }
                var orderIds = processedingOrders.Select(o => o.Id).ToList();

                return Ok(orderIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Serverda xatolik yuz berdi: {ex.Message}");
            }
        }

        [HttpGet("[Action]")]
        public IActionResult GetPreparedOrders()
        {
            try
            {
                var processedOrders = _context.Orders.Where(o => o.Process == true).ToList();
                if (!processedOrders.Any())
                {
                    return NotFound("Burutma mavjud emas.");
                }
                var orderIds = processedOrders.Select(o => o.Id).ToList();
                return Ok(orderIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Serverda xatolik yuz berdi: {ex.Message}");
            }
        }
    }
}
