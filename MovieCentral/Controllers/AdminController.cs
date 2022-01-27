using ClosedXML.Excel;
using MovieCentral.Functions;
using MovieCentral.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieCentral.Controllers
{
    [AdminSecurity]
    public class AdminController : Controller
    {
        FileUpload filefuncs = new FileUpload();
        // GET: Admin Dashboard
        public ActionResult Index()
        {
            TempData["success"] = "Welcome Admin";
            return RedirectToAction("MovieList");
        }
        // GET movie list
        public ActionResult MovieList()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMovie(movieModel obj, HttpPostedFileBase movieImage)
        {
            using (dbContext _db = new dbContext())
            {
                if (!ModelState.IsValid)
                {
                    return RedirectToAction("MovieList");
                }
                var newMovie = _db.movies.Add(obj);
                _db.SaveChanges();
                filefuncs.FileUploadInFolder(newMovie.Id, movieImage);
                TempData["success"] = "Added new movie successfully";
                return RedirectToAction("MovieList");
            }
            
        }
        [HttpPost]
        public ActionResult UpdateMovie(movieModel obj, int? mid, HttpPostedFileBase movieImage)
        {
            if (!ModelState.IsValid)
            {
                return View(obj);
            }
            if (mid == null)
            {
                return View(obj);
            }
            using (dbContext _db = new dbContext())
            {
                movieModel movieobj = _db.movies.Find(mid);
                if (movieobj != null)
                {
                    movieobj.Name = obj.Name;
                    movieobj.Description = obj.Description;
                    _db.SaveChanges();
                    filefuncs.FileUploadInFolder(movieobj.Id, movieImage,true);
                    TempData["success"] = "Movie Updated Successfully";
                }
            }
            return RedirectToAction("MovieList");
        }
        public ActionResult AddMovieSchedules(string[] starts, string[] ends, int srows, int spr, int mId, decimal seat_price)
        {
            int counter = 0;
            List<int> num_per_row = new List<int>();
            DateTime dateValue;
            DateTime dateValue2;
            DateTime defaultTime = Convert.ToDateTime("1/1/0001");
            
            List<DateTime> startTimeVal = new List<DateTime>();
            List<DateTime> endTimeVal = new List<DateTime>();
            for (int i = 1; i <= srows; i++)
            {
                num_per_row.Add(i);
            }
            using (dbContext _db = new dbContext())
            {
                var msched1 = _db.movieScheds.Where(u => u.MovieId == mId).ToList();
                if (msched1.Count != 0)
                {
                    foreach (string dateString in starts)
                    {
                        if (DateTime.TryParse(dateString, out dateValue)) 
                        {
                            startTimeVal.Add(dateValue);  //add parsed date to the list
                        }
                    }
                    foreach (string dateString in ends)
                    {
                        if (DateTime.TryParse(dateString, out dateValue))
                        {
                            endTimeVal.Add(dateValue); // same with start values
                        }
                    }
                    foreach (var item in msched1) // update schedule(start&end)
                    {
                        item.TimeStart = startTimeVal[counter];
                        item.TimeEnd = endTimeVal[counter];
                        item.Price = seat_price;
                        _db.SaveChanges();
                        var seatRow = _db.seatRows.FirstOrDefault(u => u.MovieSchedId == item.Id);
                        seatRow.Number = srows;
                        seatRow.Seats = spr;
                        _db.SaveChanges();
                        updateSeats(seatRow.Id, num_per_row, spr, item.TimeStart);
                        counter++;
                    }
                    
                    counter = 0;
                }
                else
                {
                    foreach (string dateString in starts)
                    {
                        movieSchedModel movieSchedModel = new movieSchedModel();
                        if (DateTime.TryParse(dateString, out dateValue))
                        {
                            movieSchedModel.TimeStart = dateValue;
                            movieSchedModel.MovieId = mId;
                            movieSchedModel.Price = seat_price;
                            var newmoviesched = _db.movieScheds.Add(movieSchedModel);
                            _db.SaveChanges();
                            seatRowModel seatRow = new seatRowModel();
                            seatRow.Number = srows;
                            seatRow.Seats = spr;
                            seatRow.MovieSchedId = newmoviesched.Id;
                            var new_row = _db.seatRows.Add(seatRow);
                            _db.SaveChanges();
                            //update seats table
                            updateSeats(new_row.Id, num_per_row,spr, movieSchedModel.TimeStart);

                        }
                    }

                    foreach (string dateString in ends)
                    {
                        if (DateTime.TryParse(dateString, out dateValue2))
                        {
                            movieSchedModel msched2 = _db.movieScheds.FirstOrDefault(u => u.MovieId == mId && u.TimeEnd == defaultTime);
                            msched2.TimeEnd = dateValue2;
                            _db.SaveChanges();
                        }
                        else
                            Console.WriteLine("Unable to parse '{0}'.", dateString);
                    }
                }
                TempData["success"] = "Movie Schedule Updated Successfully";
                return RedirectToAction("MovieList");
            }

        }
        // GET all reservation history
        public ActionResult Reservations()
        {
            using (dbContext _db = new dbContext())
            {
                List<movieModel> movies = _db.movies.ToList(); 
                
                return View(movies);
            }
        }
        public JsonResult clearReservations(int[] reservation_ids)
        {
            string seat_ids = "";
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
                            reservation_info.HasReserved = 0;
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

                    return Json(new { redirectToUrl = Url.Action("Reservations", "Admin") });
                }
            }
            return Json(new { redirectToUrl = Url.Action("Reservations", "Admin") });
        }
        public void updateSeats(int row_id, List<int> num_rows, int seats_per_row, DateTime start_time)
        {
            List<string> seatnames = new List<string>(){
                "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"
            };
            using (dbContext _db = new dbContext())
            {
                List<seatModel> getseatmodel = _db.seats.Where(u => u.RowId == row_id).ToList();
                if (getseatmodel.Count != 0)
                {
                   _db.Database.ExecuteSqlCommand("DELETE FROM dbo.Seat WHERE RowId = " + row_id);
                }
                //TO ADD SEATS
                //num_rows[1,2,3,...]
                for (int i = 0; i < num_rows.Count; i++)
                {
                    for (int j = 0; j < seats_per_row; j++)
                    {
                        seatModel newseat = new seatModel();
                        newseat.RowId = row_id;
                        newseat.Number = num_rows[i];
                        newseat.Name = seatnames[j];
                        newseat.Occupied = false;
                        if (start_time < DateTime.Now)
                        {
                            newseat.Occupied = true;
                        }
                        _db.seats.Add(newseat);
                        _db.SaveChanges();
                    }
                }
            }
        }

        [HttpPost]
        public FileResult ExportTable(int mid)
        {
            DateTime now = DateTime.Now;
            DataTable dt = new DataTable("Reservation");
            //create header
            dt.Columns.AddRange(new DataColumn[6]{
                new DataColumn("Movie"),
                new DataColumn("Schedule"),
                new DataColumn("Customer"),
                new DataColumn("Seats"),
                new DataColumn("Reservation_Date"),
                new DataColumn("Status")
            });
            //Query data for rows
            using (dbContext _db = new dbContext())
            {
                List<reservationModel> reservationModel = _db.reservations.ToList();
                List<myreservationVModel> reslists = new List<myreservationVModel>();
                string r_status = "Reserved";
                if (mid == 0)
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
                                               moviename = m.Name,
                                               reserved_seat_ids = r.SeatIds,
                                               reserved_seats = r.SeatNames,
                                               show_time = ms.TimeStart,
                                               reservation_date = r.ReservationDate,
                                               customer = c.Name,
                                               status = r.HasReserved
                                           }).ToList();
                        reslists.AddRange(res_details);
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
                                               moviename = m.Name,
                                               reserved_seat_ids = r.SeatIds,
                                               reserved_seats = r.SeatNames,
                                               show_time = ms.TimeStart,
                                               reservation_date = r.ReservationDate,
                                               customer = c.Name,
                                               status = r.HasReserved
                                           }).ToList();
                        reslists.AddRange(res_details);
                    }
                }
                
                foreach (var res in reslists)
                {
                    if (res.status == 1) r_status = "Reserved";
                    else if (res.status == 2) r_status = "Used";
                    else if (res.status == 3) r_status = "Cancelled";
                    dt.Rows.Add(res.moviename, res.show_time, res.customer, res.reserved_seats,res.reservation_date, r_status);
                }
            }
            //using ClosedXML package
            using (XLWorkbook wb = new XLWorkbook())
            {
                //add dataTable values
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    //then save
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "reservation" + now.ToString("ddMMyyhhmmss")+".xlsx");
                }
            }
        }
    }
}