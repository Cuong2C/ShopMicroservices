
namespace Basket.Api.Exceptions;

public class BasketNotfoundException : NotFoundException
{
    public BasketNotfoundException(string userName) : base("Basket" ,userName)
    {
        
    }
}
