using PosudaShop.Models.Data;
using PosudaShop.Models.ViewModels.Account;
using PosudaShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PosudaShop.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: /account/login
        [HttpGet]
        public ActionResult Login()
        {
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("user-profile");

            return View();
        }

        // POST: /account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            if (! ModelState.IsValid)
            {
                return View(model);
            }

            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (! isValid)
            {
                ModelState.AddModelError("", "Неверное имя или пароль");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
        }

        // GET: /account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // POST: /account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {

            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }


            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Пароли не совпадают.");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
    
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "Имя " + model.Username + " занято.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }


                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password
                };

                db.Users.Add(userDTO);


                db.SaveChanges();


                int id = userDTO.Id;

                UserRoleDTO userRolesDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRolesDTO);
                db.SaveChanges();
            }

            TempData["SM"] = "Вы успешно зарегистрировались, и можете войти";

            return Redirect("~/account/login");
        }

        // GET: /account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {

            string username = User.Identity.Name;


            UserNavPartialVM model;

            using (Db db = new Db())
            {

                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

         
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }

            return PartialView(model);
        }

        // GET: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {

            string username = User.Identity.Name;


            UserProfileVM model;

            using (Db db = new Db())
            {

                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);


                model = new UserProfileVM(dto);
            }

      
            return View("UserProfile", model);
        }

        // POST: /account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {

            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }


            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Пароли не совпадают");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {

                string username = User.Identity.Name;

                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == username))
                {
                    ModelState.AddModelError("", "Имя " + model.Username + " занято.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }


                db.SaveChanges();
            }

            TempData["SM"] = "Вы отредактировали профиль";

            return Redirect("~/account/user-profile");
        }

        // GET: /account/Orders
        [Authorize(Roles="User")]
        public ActionResult Orders()
        {

            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {

                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;


                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();


                foreach (var order in orders)
                {

                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

            
                    decimal total = 0m;

   
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();


                    foreach (var orderDetails in orderDetailsDTO)
                    {

                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        decimal price = product.Price;

                        string productName = product.Name;

                        productsAndQty.Add(productName, orderDetails.Quantity);


                        total += orderDetails.Quantity * price;
                    }

 
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }

            }

  
            return View(ordersForUser);
        }
    }
}