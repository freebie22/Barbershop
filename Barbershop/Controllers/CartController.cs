using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Barbershop.Utility;
using Barbershop.Utility.BrainTreeSettings;
using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Security.Claims;
using System.Text;

namespace Barbershop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IBrainTreeGate _brain;
        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender, IBrainTreeGate brain)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _brain = brain;
        }
        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exists
                shoppingCartList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).ToList();
            }
            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Products> prodListTemp = _db.Products.Where(u => prodInCart.Contains(u.Id));
            IList<Products> prodList = new List<Products>();

            foreach(var cartObj in shoppingCartList)
            {
                Products prodTemp = prodListTemp.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempCount = cartObj.ProductCount;
                prodList.Add(prodTemp);
            }

            return View(prodList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(List<Products> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Products prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, ProductCount = prod.TempCount });
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }


        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var gateway = _brain.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exists
                shoppingCartList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).ToList();
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Products> prodList = _db.Products.Where(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                BarbershopUser = _db.BarbershopUser.FirstOrDefault(u => u.Id == claim.Value),
                
            };

            foreach(var cartObj in shoppingCartList)
            {
                Products prodTemp = _db.Products.FirstOrDefault(u=> u.Id == cartObj.ProductId);
                prodTemp.TempCount = cartObj.ProductCount;
                ProductUserVM.ProductList.Add(prodTemp);    
            }

            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(IFormCollection collection,ProductUserVM ProductUserVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var region = ProductUserVM.BarbershopUser.Region;

            if (region == "" || region == null)
            {
                region = "Житомирська область";
            }

            var city = ProductUserVM.BarbershopUser.City;

            if (city == "" || city == null)
            {
                city = "Бердичів";
            }

            var postOffice = ProductUserVM.BarbershopUser.PostOffice;

            if (postOffice == "" || postOffice == null)
            {
                postOffice = "Самовивіз (м.Бердичів, вул.Європейська 54)";
            }


            OrderHeader orderHeader = new OrderHeader()
            {
                CreatedByUserId = claim.Value,
                FinalOrderTotal = ProductUserVM.ProductList.Sum(x => x.TempCount * x.Price),
                City = city,
                Region = region,
                PostOffice = postOffice,
                FullName = ProductUserVM.BarbershopUser.FullName,
                Email = ProductUserVM.BarbershopUser.Email,
                PhoneNumber = ProductUserVM.BarbershopUser.PhoneNumber,
                OrderDate = DateTime.Now,
                OrderStatus = WC.StatusPending,
                TransactionId = "NONE"

                };
                _db.OrderHeader.Add(orderHeader);
                _db.SaveChanges();

                foreach (var prod in ProductUserVM.ProductList)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderHeaderId = orderHeader.Id,
                        PricePerOne = prod.Price,
                        Count = prod.TempCount,
                        ProductId = prod.Id
                    };
                    _db.OrderDetail.Add(orderDetail);

                }
                _db.SaveChanges();

            string nonceFromTheClient = collection["payment_method_nonce"];

            var request = new TransactionRequest
            {
                Amount = Convert.ToDecimal(orderHeader.FinalOrderTotal / 38.0m),
                PaymentMethodNonce = nonceFromTheClient,
                OrderId = orderHeader.Id.ToString(),
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };
            var gateway = _brain.GetGateway();
            Result<Transaction> result = gateway.Transaction.Sale(request);

            if (result.Target.ProcessorResponseText == "Approved")
            {
                orderHeader.TransactionId = result.Target.Id;
                orderHeader.OrderStatus = WC.StatusApproved;
            }
            else
            {
                orderHeader.OrderStatus = WC.StatusCancelled;
            }
            _db.OrderHeader.Update(orderHeader);
            _db.SaveChanges();

            var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
            + "templates" + Path.DirectorySeparatorChar.ToString() + "Template.html";

            var subject = "Ваше замовлення";

            ProductUserVM.OrderTotal = 0.0m;

            StringBuilder productListSB = new StringBuilder();

            for (int i = 0; i < ProductUserVM.ProductList.Count; i++)
            {
                var prod = ProductUserVM.ProductList[i];
                ProductUserVM.OrderTotal += prod.Price * prod.TempCount;
                productListSB.Append($"Товар: {prod.ProductName} - {prod.TempCount} шт;<br/>");
            }

            string messageBody = $@"
                <!DOCTYPE html>
                            <html>
                            <head>
                                <title></title>
                                <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
                                <meta name='viewport' content='width=device-width, initial-scale=1'>
                                <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                                <style type='text/css'>
	
                                    @media screen {{
                                        @font-face {{
                                            font-family: 'Lato';
                                            font-style: normal;
                                            font-weight: 400;
                                            src: local('Lato Regular'), local('Lato-Regular'), url(https://fonts.gstatic.com/s/lato/v11/qIIYRU-oROkIk8vfvxw6QvesZW2xOQ-xsNqO47m55DA.woff) format('woff');
                                        }}

                                        @font-face {{
                                            font-family: 'Lato';
                                            font-style: normal;
                                            font-weight: 700;
                                            src: local('Lato Bold'), local('Lato-Bold'), url(https://fonts.gstatic.com/s/lato/v11/qdgUG4U09HnJwhYI-uK18wLUuEpTyoUstqEm5AMlJo4.woff) format('woff');
                                        }}

                                        @font-face {{
                                            font-family: 'Lato';
                                            font-style: italic;
                                            font-weight: 400;
                                            src: local('Lato Italic'), local('Lato-Italic'), url(https://fonts.gstatic.com/s/lato/v11/RYyZNoeFgb0l7W3Vu1aSWOvvDin1pK8aKteLpeZ5c0A.woff) format('woff');
                                        }}

                                        @font-face {{
                                            font-family: 'Lato';
                                            font-style: italic;
                                            font-weight: 700;
                                            src: local('Lato Bold Italic'), local('Lato-BoldItalic'), url(https://fonts.gstatic.com/s/lato/v11/HkF_qI1x_noxlxhrhMQYELO3LdcAZYWl9Si6vvxL-qU.woff) format('woff');
                                        }}
                                    }}

                                    /* CLIENT-SPECIFIC STYLES */
                                    body,
                                    table,
                                    td,
                                    a {{
                                        -webkit-text-size-adjust: 100%;
                                        -ms-text-size-adjust: 100%;
                                    }}

                                    table,
                                    td {{
                                        mso-table-lspace: 0pt;
                                        mso-table-rspace: 0pt;
                                    }}

                                    img {{
                                        -ms-interpolation-mode: bicubic;
                                    }}

                                    /* RESET STYLES */
                                    img {{
                                        border: 0;
                                        height: auto;
                                        line-height: 100%;
                                        outline: none;
                                        text-decoration: none;
                                    }}

                                    table {{
                                        border-collapse: collapse !important;
                                    }}

                                    body {{
                                        height: 100% !important;
                                        margin: 0 !important;
                                        padding: 0 !important;
                                        width: 100% !important;
                                    }}

                                    /* iOS BLUE LINKS */
                                    a[x-apple-data-detectors] {{
                                        color: inherit !important;
                                        text-decoration: none !important;
                                        font-size: inherit !important;
                                        font-family: inherit !important;
                                        font-weight: inherit !important;
                                        line-height: inherit !important;
                                    }}

                                    /* MOBILE STYLES */
                                    @media screen and (max-width:600px) {{
                                        h1 {{
                                            font-size: 32px !important;
                                            line-height: 32px !important;
                                        }}
                                    }}

                                    /* ANDROID CENTER FIX */
                                    div[style*='margin: 16px 0;'] {{
                                        margin: 0 !important;
                                    }}
                                </style>
                            </head>

                            <body style='background-color: #f4f4f4; margin: 0 !important; padding: 0 !important;'>
                            <!-- HIDDEN PREHEADER TEXT -->
                            <table border='0' cellpadding='0' cellspacing='0' width='100%'>
                                <!-- LOGO -->
                                <tr>
                                    <td bgcolor='#000000' align='center'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                            <tr>
                                                <td align='center' valign='top' style='padding: 40px 10px 40px 10px;'> </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td bgcolor='#000000' align='center' style='padding: 0px 10px 0px 10px;'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                            <tr>
                                                <td bgcolor='#ffffff' align='center' valign='top' style='padding: 40px 20px 20px 20px; border-radius: 4px 4px 0px 0px; color: #111111; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 48px; font-weight: 400; letter-spacing: 4px; line-height: 48px;'>
                                                    <img src=""https://i.ibb.co/tDWZZnD/Oasis.jpg"" width = '225' height = '120' alt=""""Oasis"""" border=""""0"""" />
                                                     <h1 style='font-size: 24px; font-weight: 400; margin: 2;'>Вами було оформленно замовлення в нашому магазині!</h1> 
                                                </td>
                                            </tr>
                                            
                                            <tr>
                                                <td bgcolor='#ffffff' align='center' style='padding: 0px 30px 20px 30px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0;'>Деталі Вашого замовлення</p>
                                                    <table width=""95%"" border=""0"" align=""center"" cellpadding=""0"" cellspacing=""0"">

                                                        <tr>
                                                            <td width=""100%"" style=""color:darkslategrey; font-family:Arial, Helvetica, sans-serif; padding:10px;"">
                                                                <div style=""font-size:16px; color:#564319;"">
                                                                    Інформація про замовника: 
                                                                </div>
                                                                <div style=""font-size:16px;padding-left:15px;"">
                                                                    Ім'я та прізвище  : {ProductUserVM.BarbershopUser.FullName}
                                                                    <br />
                                                                    Ел. пошта &nbsp;: {ProductUserVM.BarbershopUser.Email}
                                                                    <br />
                                                                    Номер телефону : {ProductUserVM.BarbershopUser.PhoneNumber}
                                                                </div>
                                                                <br />
                                                                <hr>
                        
                                                            </td>
                                                        </tr>
                        
                                                        <tr>
                                                            <td width=""100%"" style=""color:darkslategrey; font-family:Arial, Helvetica, sans-serif; padding:10px;"">
                                                                <div style=""font-size:16px; color:#564319;"">
                                                                    Замовлені товари:
                                                                </div>
                                                                <div style=""font-size:16px; color:#525252;padding-left:15px;"">
                                                                  
                                                                    {productListSB.ToString()}
                                                                    Ціна до сплати:
                                                                    {ProductUserVM.OrderTotal} грн
                                                                </div>
                                                            </td>
                                                        </tr>
                        
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td bgcolor='#f4f4f4' align='center' style='padding: 0px 10px 0px 10px;'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                           <!-- <tr>
                                                <td bgcolor='#ffffff' align='left' style='padding:0px 30px 20px 30px; color: #666666; font-family: 'Lato', Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0; text-align:center'>YOUR OPT : *****</p>
                                                </td>
                                            </tr>-->
                                            <tr>
                                                <td bgcolor='#ffffff' width=""100%"" style=""color:darkslategrey; font-family:Arial, Helvetica, sans-serif; padding:0px 30px 20px 30px;"">
                                                                <br />
                                                                <hr>
                                                                <div style=""font-size:16px; color:#564319;"">
                                                                    Інформація про доставку: 
                                                                </div>
                                                                <div style=""font-size:16px;padding-left:15px;"">
                                                                    Область : {region}
                                                                    <br />
                                                                    Місто : {city}
                                                                    <br />
                                                                    Адреса доставки : {postOffice}
                                                                </div>
                                                                <br />
                                                                <hr>
                        
                                                            </td>
                                            </tr>
                                            <tr>
                                                <td bgcolor='#ffffff' align='left' style='padding: 0px 30px 40px 30px; border-radius: 0px 0px 4px 4px; color: #666666; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <p style='margin: 0;'>Наші соціальні мережі:</p>
                                                    <div>
                                                        <a style='padding-right:10px' href='https://www.instagram.com/boykov_artem'><img src='https://cdn-icons-png.flaticon.com/512/2111/2111463.png' width='25'></a>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td bgcolor='#f4f4f4' align='center' style='padding: 30px 10px 0px 10px;'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px;'>
                                            <tr>
                                                <td bgcolor='#000000' align='center' style='padding: 30px 30px 30px 30px; border-radius: 4px 4px 4px 4px; color: #fff; font-family: Lato, Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                    <h2 style='font-size: 20px; font-weight: 400; color: #fff; margin: 0;'>Наші контакти</h2>
                                                    <p style='margin: 0;'><a href='tel:+380685034088' target='_blank' style='color: #fff;'>Тел: +380-68-503-40-88</a></p>
                                                        <p style='margin: 0;'><a href='mailto:barbershop.oasis@ukr.net' target='_blank' style='color: #fff;'>Email: barbershop.oasis@ukr.net</a></p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                            </body>
                            </html>";


            await _emailSender.SendEmailAsync(ProductUserVM.BarbershopUser.Email, subject, messageBody);


            return RedirectToAction(nameof(InquiryConfirmation), new { id = orderHeader.Id });
        }

        public IActionResult InquiryConfirmation(int id=0)
        {
            OrderHeader orderHeader = _db.OrderHeader.FirstOrDefault(u => u.Id == id);
            HttpContext.Session.Clear();
            return View(orderHeader);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exists
                shoppingCartList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).ToList();
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(List<Products> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach(Products prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, ProductCount = prod.TempCount });
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
