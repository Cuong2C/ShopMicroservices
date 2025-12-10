
using Discount.Grpc;

namespace Basket.Api.Basket.StoreBasket;

public record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;

public record StoreBasketResult(string UserName);

public class StoreBasketCommandValidator : AbstractValidator<StoreBasketCommand>
{
    public StoreBasketCommandValidator()
    {
        RuleFor(x => x.Cart).NotNull().WithMessage("Cart cannot be null");
        RuleFor(x => x.Cart.UserName).NotEmpty().WithMessage("UserName is required");
    }
}
public class StoreBasketCommandHandler(IBasketRepository basketRepository, DiscountProtoService.DiscountProtoServiceClient discountProtoServiceClient) : ICommandHandler<StoreBasketCommand, StoreBasketResult>
{
    public async Task<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
    {
        foreach(var item in command.Cart.Items)
        {
            var discountRequest = new GetDiscountRequest { ProductName = item.ProductName };
            var discountResponse = await discountProtoServiceClient.GetDiscountAsync(discountRequest, cancellationToken: cancellationToken);
            item.Price -= discountResponse.Amount;
        }

        await basketRepository.StoreBasket(command.Cart, cancellationToken);

        return new StoreBasketResult(command.Cart.UserName);
    }
}
