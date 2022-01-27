using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MovieCentral.Models
{
    [Table("Accounts")]
    public class accountModel
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public int RoleId { get; set; }
    }
    [Table("AccountRoles")]
    public class accountRoleModel
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
    }
    [Table("Customer")]
    public class customerModel
    {
        public int Id { get; set; }
        public int AccountsId { get; set; }
        public string Name { get; set; }
    }
    [Table("Movie")]
    public class movieModel
    {
        public int Id { get; set; }
        [Required, StringLength(60, MinimumLength = 3)]
        [Display(Name = "Movie Title")]
        public string Name { get; set; }
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string ImageFullPath { get; set; }
        public DateTime Date_Added { get; set; } = DateTime.Now;
    }
    [Table("MovieSchedule")]
    public class movieSchedModel
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        [Required]
        public decimal Price { get; set; }
    }
    [Table("SeatRow")]
    public class seatRowModel
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int Seats { get; set; }
        public int MovieSchedId { get; set; }
    }
    [Table("Seat")]
    public class seatModel
    {
        public int Id { get; set; }
        public int RowId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public bool Occupied { get; set; }
    }
    [Table("Reservation")]
    public class reservationModel
    {
        public int Id { get; set; }
        public string SeatIds { get; set; }
        public string SeatNames { get; set; }
        public int CustomerId { get; set; }
        public int MovieSchedId { get; set; }
        public int HasReserved { get; set; }
        public DateTime ReservationDate { get; set; }
    }
}