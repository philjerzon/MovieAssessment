using MovieCentral.Functions;
using MovieCentral.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieCentral.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Movie()
        {
            return View();
        }
        public ActionResult Reservation(int mid,int schedid)
        {
            DateTime now = DateTime.Now;
            DateTime tomorrow = now.AddDays(1).Date;
            using (dbContext _db = new dbContext())
            {
                MovieListsIndexModel newmdl = new MovieListsIndexModel();
                var join = (from m in _db.movies
                            join ms in _db.movieScheds on m.Id equals ms.MovieId
                            join rs in _db.seatRows on ms.Id equals rs.MovieSchedId
                            where m.Id == mid && (ms.TimeStart.CompareTo(tomorrow) < 0 || ms.TimeStart.CompareTo(tomorrow) > 0)
                            select new MovieIndexViewModel
                            {
                                schedId = ms.Id,
                                movieID = m.Id,
                                movieName = m.Name,
                                show_time = ms.TimeStart,
                                price = ms.Price
                            }).ToList();
                newmdl.movieLists = join;
                newmdl.movieId = mid;
                newmdl.schedId = schedid;
                return View(newmdl);
            }
        }

        public ActionResult AddReservation(int customer_id, int schedule_id, string seatids = "", string seatnames = "")
        {
            DateTime now = DateTime.Now;
            DateTime tomorrow = now.AddDays(1).Date;
            using (dbContext _db = new dbContext())
            {
                if (seatids.Length > 0)
                {
                    string[] seat_ids = seatids.Split(',');
                    string[] seat_names = seatnames.Split(',');
                    foreach (var item in seat_ids)
                    {
                        int seat_id = int.Parse(item);
                        seatModel getSeatInfo = _db.seats.FirstOrDefault(s => s.Id == seat_id);
                        getSeatInfo.Occupied = true;
                    }
                    reservationModel newreservation = new reservationModel();
                    newreservation.SeatIds = string.Join(",", seat_ids.ToArray());
                    newreservation.SeatNames = string.Join(",", seat_names.ToArray());
                    newreservation.CustomerId = customer_id;
                    newreservation.ReservationDate = now;
                    newreservation.MovieSchedId = schedule_id;
                    newreservation.HasReserved = 1;
                    _db.reservations.Add(newreservation);
                    _db.SaveChanges();

                    TempData["success"] = "Thank you for reserving seats.";
                }
                return Json(new { redirectToUrl = Url.Action("Index", "Home") });
            }
        }

        public ActionResult MyReservation(int user_id)
        {
            using (dbContext _db = new dbContext())
            {
                
                List<reservationModel> reservationModel = _db.reservations.Where(u => u.CustomerId == user_id).ToList();
                List<myreservationVModel> myreslists = new List<myreservationVModel>();

                foreach (var item in reservationModel)
                {
                    var res_details = (from r in _db.reservations
                                       join ms in _db.movieScheds on r.MovieSchedId equals ms.Id
                                       join m in _db.movies on ms.MovieId equals m.Id
                                       where r.Id == item.Id
                                       select new myreservationVModel
                                       {
                                           reservation_id = r.Id,
                                           moviename = m.Name,
                                           movieid = m.Id,
                                           reserved_seats = r.SeatNames,
                                           reserved_seat_ids = r.SeatIds,
                                           show_time = ms.TimeStart,
                                           reservation_date = r.ReservationDate,
                                           status = r.HasReserved
                                       }).ToList();
                    myreslists.AddRange(res_details);
                }
                reservationListVModel newmdl = new reservationListVModel
                {
                    myreservations = myreslists
                };
                return View(newmdl);


            }
        }
        public JsonResult clearReservations(int[] reservation_ids)
        {
            string seat_ids = "";
            int my_id = int.Parse(Request.Cookies["_usr"].Value.Split(',')[0]);
            if (reservation_ids != null)
            {
                using (dbContext _db = new dbContext())
                {
                    foreach (var x in reservation_ids)
                    {
                        reservationModel reservation_info = _db.reservations.FirstOrDefault(r => r.Id == x);
                        seat_ids += reservation_info.SeatIds + ',';
                        if (reservation_info != null)
                        {
                            _db.reservations.Remove(reservation_info);
                            _db.SaveChanges();
                        }
                    }
                    string[] arrayseat_ids = seat_ids.Split(',');
                    for (int i = 0; i < arrayseat_ids.Length - 1; i++)
                    {
                        int parsed_id = int.Parse(arrayseat_ids[i]);
                        seatModel seatinfo = _db.seats.FirstOrDefault(s => s.Id == parsed_id);
                        seatinfo.Occupied = false;
                        _db.SaveChanges();
                    }
                    return Json(new { redirectToUrl = Url.Action("MyReservation", "Home", new { user_id = my_id }) });
                }
            }
            return Json(new { redirectToUrl = Url.Action("MyReservation", "Home") });
        }
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string Username, string customername, string Password)
        {
            using (dbContext _db = new dbContext())
            {
                var join = (from a in _db.accounts
                            join r in _db.accountRoles on a.RoleId equals r.Id
                            where a.Username.Equals(Username)
                            select new
                            {
                                Id = a.Id,
                                Username = a.Username,
                                Rolename = r.RoleName
                            }).FirstOrDefault();
                if (join != null)
                {
                    TempData["error"] = "Username already registered";
                    return View();

                }
                else
                {
                    accountModel newacc = new accountModel();
                    newacc.Username = Username;
                    newacc.Password = Password;
                    newacc.RoleId = 2;
                    _db.accounts.Add(newacc);
                    _db.SaveChanges();
                    customerModel newcustomer = new customerModel();
                    newcustomer.Name = customername;
                    newcustomer.AccountsId = newacc.Id;
                    _db.customers.Add(newcustomer);
                    _db.SaveChanges();
                    TempData["success"] = "Successfully registered";
                    return RedirectToAction("Login");
                }
            }
        }
        public ActionResult Login(accountModel objUser)
        {
            if (ModelState.IsValid)
            {
                using (dbContext _db = new dbContext())
                {
                    var get_account = (from a in _db.accounts
                                       join r in _db.accountRoles on a.RoleId equals r.Id
                                       where a.Username.Equals(objUser.Username) && a.Password.Equals(objUser.Password)
                                       select new
                                       {
                                           Id = a.Id,
                                           Username = a.Username,
                                           Rolename = r.RoleName,
                                           Password = a.Password
                                       }).FirstOrDefault();
                    if (get_account != null)
                    {
                        
                        if (get_account.Rolename == "Admin")
                        {
                            HttpCookie userCookie = new HttpCookie("_usr", get_account.Id + "," + get_account.Username + "," + get_account.Rolename + "," + get_account.Username);
                            userCookie.HttpOnly = true;
                            userCookie.Expires.AddDays(1);
                            HttpContext.Response.SetCookie(userCookie);
                            return RedirectToAction("Index","Admin");
                        }
                        else
                        {
                            var customer = (from c in _db.customers
                                        join a in _db.accounts on c.AccountsId equals a.Id
                                        join r in _db.accountRoles on a.RoleId equals r.Id
                                        where a.Username.Equals(get_account.Username) && a.Password == get_account.Password
                                            select new
                                        {
                                            Id = a.Id,
                                            Username = a.Username,
                                            Rolename = r.RoleName,
                                            CustomerName = c.Name
                                        }).FirstOrDefault();
                            HttpCookie userCookie = new HttpCookie("_usr", customer.Id + "," + customer.Username + "," + customer.Rolename + "," + customer.CustomerName);
                            userCookie.HttpOnly = true;
                            userCookie.Expires.AddDays(1);
                            HttpContext.Response.SetCookie(userCookie);
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        TempData["error"] = "No User found, please try again.";
                        return View(objUser);
                    }
                }
            }
            return View(objUser);
        }
        public ActionResult Logout()
        {
            Response.Cookies["_usr"].Expires = DateTime.Now.AddDays(-1);
            return RedirectToAction("Login");
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}