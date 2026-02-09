using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.ViewModels;

public class AdminOrderEditViewModel
{
    [Display(Name = "Id замовлення")]
    public int Id { get; set; }

    [Display(Name = "Email замовника")]
    public string? UserEmail { get; set; }

    [Display(Name = "Дата замовлення")]
    public DateTime OrderDate { get; set; }

    [Display(Name = "Загальна вартість")]
    public decimal TotalPrice { get; set; }

    [Display(Name = "Змінити статус")]
    [Required]
    public int StatusId { get; set; }
}
