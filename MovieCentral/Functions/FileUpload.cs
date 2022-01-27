using MovieCentral.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MovieCentral.Functions
{
    public class FileUpload
    {
        public void FileUploadInFolder(int id, HttpPostedFileBase file, bool isUpdate = false)
        {
            if (file != null)
            {
                string oldpath = "";
                string extension = Path.GetExtension(file.FileName);
                string fileName = "movie" + DateTime.Now.ToString("ddMMyyhhmmss") + extension;
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/movieposterimg/"), fileName);
                string imagePath = "/movieposterimg/" + fileName;
                file.SaveAs(path);
                using (dbContext _db = new dbContext())
                {
                    movieModel getMovie = _db.movies.FirstOrDefault(u => u.Id == id);
                    if (getMovie != null)
                    {
                        try
                        {
                            if (isUpdate)
                            {
                                oldpath = getMovie.ImageFullPath;
                                if (System.IO.File.Exists(oldpath))
                                {
                                    System.IO.File.Delete(oldpath);
                                }
                            }

                            getMovie.ImagePath = imagePath;
                            getMovie.ImageFullPath = path;
                            _db.SaveChanges();


                        }
                        catch (IOException ioExp)
                        {
                            Console.WriteLine(ioExp.Message);
                        }
                    }

                }
                
            }
        }
    }
}