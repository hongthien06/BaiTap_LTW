using System;

namespace NhaGiaKim.Domain.Entities;

/// <summary>Danh gia cua doc gia (so sao + noi dung). Mac dinh chua duyet.</summary>
public class Feedback
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    /// <summary>So sao, 1 den 5.</summary>
    public int Rating { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>Chi feedback da duyet moi hien tren landing va duoc tinh vao diem trung binh.</summary>
    public bool IsApproved { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
