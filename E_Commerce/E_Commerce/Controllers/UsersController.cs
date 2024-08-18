using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using E_Commerce.Models;

namespace E_Commerce.Controllers
{
    public class UsersController : Controller
    {
        private ecommerceEntities1 db = new ecommerceEntities1();

        // GET: Users
        public ActionResult Index()
        {
            return View(db.Categories.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,email,password,ConfirmPassword")] User user)
        {
            if (ModelState.IsValid && user.password == user.ConfirmPassword)
            {

                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.b = "Passwords do not match! ";
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,email,password")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult login()
        {

            return View();
        }
        [HttpPost]
        public ActionResult login(FormCollection form)
        {
            var email = form["email"];
            var password = form["password"];
            var user = db.Users.FirstOrDefault(u => u.email == email && u.password == password);
            if (user == null)
            {

                return View();
            }
            Session["userId"] = user.id;
            Session["userName"] = user.name;



            return RedirectToAction("Index");
        }

        public ActionResult logout()
        {
            Session["userId"] = null;

            return RedirectToAction("Index");
        }

        public ActionResult product(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }
            var products = db.Products.Where(p => p.category_id == id).ToList();
            return View(products);
        }

        public ActionResult cart(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user_id = Session["userId"];
            if (user_id == null)
            {
                return RedirectToAction("login");
            }

            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            var cartitemexisting = db.Carts.FirstOrDefault(c => c.product_id == id && c.Users_id == (int)user_id);
            if (cartitemexisting != null)
            {
                cartitemexisting.quantity += 1;
            }
            else
            {
                Cart cart = new Cart()
                {
                    product_id = product.id,
                    Users_id = (int)user_id,
                    quantity = 1
                };
                db.Carts.Add(cart);


            }
            db.SaveChanges();
            return RedirectToAction("showcart", "Users");
        }

        public ActionResult showcart()
        {
            var user_id = Session["userId"];
            var cart = db.Carts.Where(c => c.Users_id == (int)user_id).ToList();

            return View(cart);
        }
        public ActionResult product_details(int? id)
        {
            if (id == null)
            {

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product_details = db.Products.Find(id);

            return View(product_details);

        }

        public ActionResult Shop(int? categoryId)
        {
            var products = db.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.category_id == categoryId.Value);
            }

            return View(products.ToList());
        }


        public ActionResult about()
        {

            return View();
        }

        public ActionResult contact()
        {

            return View();
        }

        public ActionResult Addquantity(int? id)
        {
            var user = (int)Session["userId"];
            var cartItem = db.Carts.FirstOrDefault(m => m.product_id == id);
            if (cartItem == null)
            {
                cartItem = new Cart();
                cartItem.quantity = 1;
                cartItem.Users_id = (int)user;
                cartItem.product_id = id;
                db.Carts.Add(cartItem);
                db.SaveChanges();
                return RedirectToAction("showcart", "Users");
            }
            cartItem.quantity += 1;
            db.Entry(cartItem).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("showcart", "Users");
        }

        public ActionResult Subquantity(int? id)
        {
            var user = (int)Session["userId"];
            var cartItem = db.Carts.FirstOrDefault(m => m.product_id == id);

            if (cartItem.quantity > 1)
            {
                cartItem.quantity -= 1;
                db.Entry(cartItem).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("showcart", "Users");
        }

        public ActionResult DeletItem(int? id)
        {
            var user = (int)Session["userId"];
            var cartItem = db.Carts.FirstOrDefault(m => m.product_id == id);
            db.Carts.Remove(cartItem);
            db.SaveChanges();
            return RedirectToAction("showcart", "Users");
        }
    }
}
