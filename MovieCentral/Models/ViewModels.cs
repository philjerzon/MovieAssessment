using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieCentral.Models
{
    public class ScheduleWithSeats
    {
        public List<movieSchedModel> movieScheds { get; set; }
        public int rows { get; set; }
        public int seats { get; set; }
        public decimal price { get; set; }
        public bool isElapsed { get; set; }
    }
    public class MovieIndexViewModel
    {
        public int schedId { get; set; }
        public int movieID { get; set; }
        public string movieName { get; set; }
        public string movieDesc { get; set; }
        public DateTime show_time { get; set; }
        public string imagePath { get; set; }
        public int seats_today { get; set; }
        public decimal price { get; set; }
    }
    public class MovieListsIndexModel
    {
        public List<MovieIndexViewModel> movieLists { get; set; }
        public int movieId { get; set; }
        public int schedId { get; set; }
        public int total_seats_today { get; set; }
    }

    public class reservationVModel
    {
        public string MovieName { get; set; }
        public decimal Price { get; set;  }
        public DateTime showtime { get; set; }
        public int rows { get; set; }
        public int spr { get; set; }
        public int schedId { get; set; }
        public List<seatViewModel> seats { get; set; }
    }
    public class seatViewModel
    {
        public int Id { get; set; }
        public int RowId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public bool Occupied { get; set; }
    }
    public class myreservationVModel
    {
        public int reservation_id { get; set; }
        public string moviename { get; set; }
        public string customer { get; set; }
        public int movieid { get; set; }
        public string reserved_seats { get; set; }
        public string reserved_seat_ids { get; set; }
        public DateTime show_time { get; set; }
        public DateTime reservation_date { get; set; }
        public int status { get; set; }
    }

    public class reservationListVModel
    {
        public List<myreservationVModel> myreservations { get; set; }
        public string moviename { get; set; }
        public DateTime show_time { get; set; }
        public int status { get; set; }
        public string seatnames_combined { get; set; }
    }

}