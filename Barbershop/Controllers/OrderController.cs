using Barbershop.Data;
using Barbershop.Models;
using Barbershop.Models.ViewModels;
using Barbershop.Utility.BrainTreeSettings;
using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Barbershop.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IBrainTreeGate _brain;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(ApplicationDbContext db, IBrainTreeGate brain)
        {
            _db = db;
            _brain = brain;
        }

        public IActionResult Index(string searchName = null, string searchEmail = null, string searchPhone = null, string Status = null)
        {
            OrderListVM orderListVM = new OrderListVM()
            {
                OrderHList = _db.OrderHeader,
                StatusList = WC.listStatus.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = i,
                    Value= i
                })
            };

            if (!string.IsNullOrEmpty(searchName))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.FullName.ToLower().Contains(searchName.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchEmail))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchPhone))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower()));
            }
            if (!string.IsNullOrEmpty(Status) && Status != "--Статус замовлення--")
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.OrderStatus.ToLower().Contains(Status.ToLower()));
            }

            return View(orderListVM);
        }

        [AllowAnonymous]
        public async Task <IActionResult> IndexUser(string searchId = null, string searchEmail = null, string searchPhone = null, string Status = null)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderListVM orderListVM = new OrderListVM()
            {
                OrderHList = _db.OrderHeader.Where(u => u.CreatedByUserId == claim.Value),
                StatusList = WC.listStatus.ToList().Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };

             if (!string.IsNullOrEmpty(searchId))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.FullName.ToLower().Contains(searchId.ToLower()) && u.CreatedByUserId == claim.Value);
            }
            if (!string.IsNullOrEmpty(searchEmail))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower()) && u.CreatedByUserId == claim.Value);
            }
            if (!string.IsNullOrEmpty(searchPhone))
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower()) && u.CreatedByUserId == claim.Value);
            }
            if (!string.IsNullOrEmpty(Status) && Status != "--Статус замовлення--")
            {
                orderListVM.OrderHList = orderListVM.OrderHList.Where(u => u.OrderStatus.ToLower().Contains(Status.ToLower()) && u.CreatedByUserId == claim.Value);
            }

            return View(orderListVM);
        }

            public IActionResult Details(int id)
            {
                OrderVM = new OrderVM()
                {
                    OrderHeader = _db.OrderHeader.FirstOrDefault(o => o.Id == id),
                    OrderDetail = _db.OrderDetail.Where(d => d.OrderHeaderId == id).Include("Product").ToList()
                };
                return View(OrderVM);
            }

        public IActionResult UserDetails(int id)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _db.OrderHeader.FirstOrDefault(o => o.Id == id),
                OrderDetail = _db.OrderDetail.Where(d => d.OrderHeaderId == id).Include("Product").ToList()
            };
            return View(OrderVM);
        }




        [HttpPost]
        public IActionResult StartProcessing()
        {
            OrderHeader orderHeader = _db.OrderHeader.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WC.StatusInProcess;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _db.OrderHeader.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WC.StatusArrived;
            orderHeader.ShippingDate = DateTime.Now;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ReceiveOrder()
        {
            OrderHeader orderHeader = _db.OrderHeader.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WC.StatusReceived;
            orderHeader.ReceivingDate = DateTime.Now;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = _db.OrderHeader.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

            var gateway = _brain.GetGateway();
            Transaction transaction = gateway.Transaction.Find(orderHeader.TransactionId);

            if (transaction.Status == TransactionStatus.AUTHORIZED || transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            {
                //no refund
                Result<Transaction> resultvoid = gateway.Transaction.Void(orderHeader.TransactionId);
            }
            else
            {
                Result<Transaction> resultRefund = gateway.Transaction.Refund(orderHeader.TransactionId);
            }

            orderHeader.OrderStatus = WC.StatusReturned;

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        //[HttpPost]
        //public IActionResult UpdateOrderDetails()
        //{
        //    OrderHeader orderHeaderFromDb = _db.OrderHeader.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

        //    orderHeaderFromDb.FullName = OrderVM.OrderHeader.FullName;
        //    orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
        //    orderHeaderFromDb.Region = OrderVM.OrderHeader.Region;
        //    orderHeaderFromDb.City = OrderVM.OrderHeader.City;
        //    orderHeaderFromDb.PostOffice = OrderVM.OrderHeader.PostOffice;
        //    orderHeaderFromDb.Email = OrderVM.OrderHeader.Email;

        //    _db.SaveChanges();
        //    return RedirectToAction("Details", "Order", new { id = orderHeaderFromDb.Id });
        //}
    }
}
