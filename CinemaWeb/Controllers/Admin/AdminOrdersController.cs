using CinemaDomain.Model;
using CinemaInfrastructure;
using CinemaWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;

namespace CinemaWeb.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminOrdersController : Controller
{
    private readonly CinemaDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AdminOrdersController(CinemaDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
            .OrderByDescending(o => o.Id)
            .Select(o => new AdminOrderIndexViewModel
            {
                Id = o.Id,
                UserEmail = o.User.Email,
                StatusName = o.Status.Name,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                TicketCount = o.Tickets.Count()
            })
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var tickets = await _context.Tickets
            .Where(t => t.OrderId == id)
            .Select(t => new AdminOrderDetailsViewModel
            {
                TicketId = t.Id,
                SessionId = t.Session.Id,
                Row = t.Seat.Row,
                SeatNumber = t.Seat.NumberInRow,
                Price = t.PurchasePrice
            })
            .ToListAsync();

        return View(tickets);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        var statuses = await _context.OrderStatuses.ToListAsync();
        ViewBag.StatusList = new SelectList(statuses, "Id", "Name", order.StatusId);

        var model = new AdminOrderEditViewModel
        {
            Id = order.Id,
            UserEmail = order.User?.Email,
            OrderDate = order.OrderDate,
            TotalPrice = order.TotalPrice,
            StatusId = order.StatusId
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminOrderEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var statuses = await _context.OrderStatuses.ToListAsync();
            ViewBag.StatusList = new SelectList(statuses, "Id", "Name", model.StatusId);
            return View(model);
        }

        var orderToUpdate = await _context.Orders.FindAsync(model.Id);

        if (orderToUpdate == null) return NotFound();

        orderToUpdate.StatusId = model.StatusId;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}
