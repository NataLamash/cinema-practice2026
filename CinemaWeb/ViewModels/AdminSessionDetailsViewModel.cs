using CinemaDomain.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CinemaWeb.ViewModels
{
    public class AdminSessionDetailsViewModel
    {
        public Session Session { get; set; } = null!;

        public List<SeatTypePricingGroup> PricingDetails { get; set; } = new();
    }

    public class SeatTypePricingGroup
    {
        public string SeatTypeName { get; set; } = string.Empty;

        public int SeatCount { get; set; }

        public decimal MarkUp { get; set; }

        public decimal FinalPrice { get; set; }
    }
}
