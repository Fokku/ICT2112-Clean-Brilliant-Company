using System;
using System.Collections.Generic;
using CleanBrilliant.Interfaces;

namespace CleanBrilliant.DTO
{
    public class PreShipmentDetailDTO
    {
        public int OrderId { get; set; }
        public DateTime TimeStamp { get; set; }
        public List<ProductOrderDetailDTO> ProductOrderDetailList { get; set; } = new();
        public int PackageQuantity { get; set; }
        public List<DateTime> ReceivedDateList { get; set; } = new();
        public IProductDetailReader ProductReader { get; set; }
    }
}