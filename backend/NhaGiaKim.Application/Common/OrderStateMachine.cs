using NhaGiaKim.Domain.Enums;

namespace NhaGiaKim.Application.Common;

/// <summary>
/// Luat chuyen trang thai don hang.
/// New -> Confirmed -> Shipping -> Completed; New|Confirmed -> Cancelled.
/// Khong duoc nhay lui, khong duoc roi khoi Completed/Cancelled.
/// </summary>
public static class OrderStateMachine
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Allowed = new()
    {
        [OrderStatus.New] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Shipping, OrderStatus.Cancelled],
        [OrderStatus.Shipping] = [OrderStatus.Completed],
        [OrderStatus.Completed] = [],
        [OrderStatus.Cancelled] = []
    };

    public static bool CanTransition(OrderStatus from, OrderStatus to)
        => Allowed.TryGetValue(from, out var next) && next.Contains(to);

    public static IReadOnlyCollection<OrderStatus> NextStates(OrderStatus from)
        => Allowed.TryGetValue(from, out var next) ? next : [];
}
