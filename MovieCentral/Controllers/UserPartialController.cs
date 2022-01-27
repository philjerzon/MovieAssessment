using MovieCentral.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieCentral.Controllers
{
    public class UserPartialController : Controller
    {
        DateTime now = DateTime.Now;
        public ActionResult _MoviePosters()
        {
            using (dbContext _db = new dbContext())
            {
                MovieListsIndexModel newmdl = new MovieListsIndexModel();
                if (_db.movieScheds.Count(u => DbFunctions.TruncateTime(u.TimeStart) >= DbFunctions.TruncateTime(now)) == 0)
                {
                    return PartialView();
                }
                //GET info of every schedule of every Movies
                var join = (from m in _db.movies
                            join ms in _db.movieScheds on m.Id equals ms.MovieId
                            join rs in _db.seatRows on ms.Id equals rs.MovieSchedId
                            where ms.TimeStart > now && (DbFunctions.TruncateTime(ms.TimeStart) >= DbFunctions.TruncateTime(now))
                            select new MovieIndexViewModel
                            {
                                schedId = ms.Id,
                                movieID = m.Id,
                                movieName = m.Name,
                                movieDesc = m.Description,
                                imagePath = m.ImagePath,
                                show_time = ms.TimeStart,
                                price = ms.Price,
                                seats_today = rs.Number * rs.Seats,
                            }).ToList();
                newmdl.movieLists = join;
                return PartialView(newmdl);
            }
        }
        public ActionResult _MovieTimeSlots(int? mid)
        {
            using (dbContext _db = new dbContext())
            {
                DateTime now = DateTime.Now;
                MovieListsIndexModel newmdl = new MovieListsIndexModel();
                //SUM the total seats of every time slot of every movie
                int getTotalSeats = (from rs in _db.seatRows
                                     join ms in _db.movieScheds on rs.MovieSchedId equals ms.Id
                                     join m in _db.movies on ms.MovieId equals m.Id
                                     where DbFunctions.TruncateTime(ms.TimeStart) == DbFunctions.TruncateTime(now) && m.Id == mid
                                     select new
                                     {
                                         row_seats = rs.Number * rs.Seats
                                     }).Sum(x => x.row_seats);
                //GET info of every schedule of every Movies
                var join = (from m in _db.movies
                            join ms in _db.movieScheds on m.Id equals ms.MovieId
                            join rs in _db.seatRows on ms.Id equals rs.MovieSchedId
                            where DbFunctions.TruncateTime(ms.TimeStart) == DbFunctions.TruncateTime(now) && m.Id == mid
                            select new MovieIndexViewModel
                            {
                                movieName = m.Name,
                                movieDesc = m.Description,
                                imagePath = m.ImagePath,
                                show_time = ms.TimeStart,
                                price = ms.Price,
                                seats_today = rs.Number * rs.Seats,
                            }).ToList();
                newmdl.movieLists = join;
                newmdl.total_seats_today = getTotalSeats;
                return PartialView(newmdl);
            }
        }

        public ActionResult _reservationpage(int sched)
        {
            using (dbContext _db = new dbContext())
            {
                movieSchedModel getsched = _db.movieScheds.FirstOrDefault(u => u.Id == sched);
                var getRow = _db.seatRows.FirstOrDefault(u => u.MovieSchedId == getsched.Id);
                if (getsched.TimeStart < DateTime.Now)
                {
                    var getSeats = _db.seats.Where(u => u.RowId == getRow.Id && u.Occupied == false).ToList();
                    if (getSeats != null)
                    {
                        foreach (var seat in getSeats)
                        {
                            seat.Occupied = true;
                            _db.SaveChanges();
                        }
                    }
                }
                reservationVModel newmdl = new reservationVModel();
                var join = (from ms in _db.movieScheds
                            join rs in _db.seatRows on ms.Id equals rs.MovieSchedId
                            join s in _db.seats on rs.Id equals s.RowId
                            where ms.Id == sched
                            select new seatViewModel
                            {
                                Id = s.Id,
                                RowId = rs.Id,
                                Number = s.Number,
                                Name = s.Name,
                                Occupied = s.Occupied
                            }).ToList();
                seatRowModel getrow = _db.seatRows.FirstOrDefault(r => r.MovieSchedId == sched);
                newmdl.seats = join;
                newmdl.showtime = getsched.TimeStart;
                newmdl.rows = getrow.Number;
                newmdl.spr = getrow.Seats;
                newmdl.Price = getsched.Price;
                newmdl.schedId = getsched.Id;
                return PartialView(newmdl);
            }
        }
    }
}