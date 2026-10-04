using System;
using NhaGiaKim.Domain.Enums;

namespace NhaGiaKim.Domain.Entities;

/// <summary>Don dat hang (guest checkout, khong can tai khoan).</summary>
public class Order
{
    public int Id { get; set; }

    /// <summary>Ma don dang NGK-yyyyMMdd-XXXX, unique.</summary>
    public string OrderCode { get; set; } = string.Empty;

    public int BookId { get; set; }
    public Book? Book { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Quantity { get; set; }

    /// <summary>Don gia chot tai thoi diem dat (snapshot, khong doi khi admin sua gia sach).</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Luon do server tinh lai: UnitPrice * Quantity. Khong tin gia client gui len.</summary>
    public decimal TotalPrice { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
