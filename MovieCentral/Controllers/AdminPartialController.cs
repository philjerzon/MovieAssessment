using MovieCentral.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieCentral.Controllers
{
    public class AdminPartialController : Controller
    {
        // GET movie list
        public ActionResult _MovieList()
        {
            using (dbContext _db = new dbContext())
            {
                var objMovieList = _db.movies.ToList();
                return PartialView(objMovieList);
            }

        }

        public ActionResult _schedules()
        {
            using (dbContext _db = new dbContext())
            {
                var objScreenList = _db.movieScheds.ToList();
                return PartialView(objScreenList);
            }
        }

        public ActionResult _getMovieSchedules(int? mid)
        {
            using (dbContext _db = new dbContext())
            {
                var newmdl = new ScheduleWithSeats();
                var objScreenList = _db.movieScheds.Where(u => u.MovieId == mid).ToList();
                
                if (objScreenList.Count != 0)
                {
                    foreach (var item in objScreenList)
                    {
                        var getRow = _db.seatRows.FirstOrDefault(u => u.MovieSchedId == item.Id);
                        if (item.TimeStart < DateTime.Now)
                        {
                            var getSeats = _db.seats.Where(u => u.RowId == getRow.Id).ToList();
                            foreach (var seat in getSeats)
                            {
                                seat.Occupied = true;
                                _db.SaveChanges();
                            }
                            newmdl.isElapsed = true;
                        }
                    }
                    movieSchedModel objScreenId = _db.movieScheds.FirstOrDefault(u => u.MovieId == mid);
                    var objRow = _db.seatRows.FirstOrDefault(u => u.MovieSchedId == objScreenId.Id);
                    newmdl.movieScheds = objScreenList;
                    newmdl.seats = objRow.Seats;
                    newmdl.rows = objRow.Number;
                    newmdl.price = objScreenId.Price;
                    return PartialView(newmdl);
                }
                return PartialView();
            }
        }
        public ActionResult _getReservations(int? mid)
        {
            using (dbContext _db = new dbContext())
            {
                List<reservationModel> reservationModel = _db.reservations.ToList();
                List<myreservationVModel> myreslists = new List<myreservationVModel>();
                if (mid == null || mid == 0)
                {
                    foreach (var item in reservationModel)
                    {
                        var res_details = (from r in _db.reservations
                                           join ms in _db.movieScheds on r.MovieSchedId equals ms.Id
                                           join m in _db.movies on ms.MovieId equals m.Id
                                           join a in _db.accounts on r.CustomerId equals a.Id
                                           join c in _db.customers on a.Id equals c.AccountsId
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
                                               customer = c.Name,
                                               status = r.HasReserved
                                           }).ToList();
                        myreslists.AddRange(res_details);
                    }
                }
                else
                {
                    foreach (var item in reservationModel)
                    {
                        var res_details = (from r in _db.reservations
                                           join ms in _db.movieScheds on r.MovieSchedId equals ms.Id
                                           join m in _db.movies on ms.MovieId equals m.Id
                                           join a in _db.accounts on r.CustomerId equals a.Id
                                           join c in _db.customers on a.Id equals c.AccountsId
                                           where r.Id == item.Id && m.Id == mid
                                           select new myreservationVModel
                                           {
                                               reservation_id = r.Id,
                                               moviename = m.Name,
                                               movieid = m.Id,
                                               reserved_seats = r.SeatNames,
                                               reserved_seat_ids = r.SeatIds,
                                               show_time = ms.TimeStart,
                                               reservation_date = r.ReservationDate,
                                               customer = c.Name,
                                               status = r.HasReserved
                                           }).ToList();
                        myreslists.AddRange(res_details);
                    }
                }
                reservationListVModel newmdl = new reservationListVModel
                {
                    myreservations = myreslists
                };
                return PartialView(newmdl);
            }
        }
    }
}