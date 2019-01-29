using Microsoft.AspNetCore.Mvc;
using P3AddNewFunctionalityDotNetCore.Models;

namespace P3AddNewFunctionalityDotNetCore.Components
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly Cart _cart;

        public CartSummaryViewComponent(ICart cart)
        {
            _cart = cart as Cart;
        }

        public IViewComponentResult Invoke()
        {
            return View(_cart);
        }
    }
}
