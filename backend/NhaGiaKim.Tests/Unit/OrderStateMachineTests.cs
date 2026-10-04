using NhaGiaKim.Application.Common;
using NhaGiaKim.Domain.Enums;
using Shouldly;

namespace NhaGiaKim.Tests.Unit;

public class OrderStateMachineTests
{
    [Theory]
    [InlineData(OrderStatus.New, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.New, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Shipping)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Shipping, OrderStatus.Completed)]
    public void CanTransition_AllowsValidMoves(OrderStatus from, OrderStatus to)
        => OrderStateMachine.CanTransition(from, to).ShouldBeTrue();

    // AC-27: khong duoc nhay lui, khong duoc nhay coc, khong roi khoi trang thai ket thuc.
    [Theory]
    [InlineData(OrderStatus.Confirmed, OrderStatus.New)]
    [InlineData(OrderStatus.Shipping, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Completed)]
    [InlineData(OrderStatus.New, OrderStatus.Shipping)]
    [InlineData(OrderStatus.Completed, OrderStatus.New)]
    [InlineData(OrderStatus.Completed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.New)]
    [InlineData(OrderStatus.Shipping, OrderStatus.Cancelled)]
    public void CanTransition_RejectsInvalidMoves(OrderStatus from, OrderStatus to)
        => OrderStateMachine.CanTransition(from, to).ShouldBeFalse();

    [Theory]
    [InlineData(OrderStatus.New)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Shipping)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public void CanTransition_NeverAllowsSelfTransition(OrderStatus status)
        => OrderStateMachine.CanTransition(status, status).ShouldBeFalse();

    [Fact]
    public void NextStates_TerminalStatesHaveNoSuccessor()
    {
        OrderStateMachine.NextStates(OrderStatus.Completed).ShouldBeEmpty();
        OrderStateMachine.NextStates(OrderStatus.Cancelled).ShouldBeEmpty();
    }
}
