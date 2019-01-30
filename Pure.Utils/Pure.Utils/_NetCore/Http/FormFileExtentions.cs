using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
namespace Pure.Utils
{
    public static class FormFileExtentions
    {
        public static byte[] ReadAllBytes(this IFormFile self)
        {
            using (var reader = new BinaryReader(self.OpenReadStream()))
            {
                return reader.ReadBytes(Convert.ToInt32(self.Length));
            }
        }

        public static Task<byte[]> ReadAllBytesAsync(this IFormFile self)
        {
            return Task.Factory.StartNew<byte[]>(() =>
            {
                using (var reader = new BinaryReader(self.OpenReadStream()))
                {
                    return reader.ReadBytes(Convert.ToInt32(self.Length));
                }
            });
        }

        public static string GetFormFieldName(this IFormFile self)
        {
            try
            {
                var tmp = self.ContentDisposition.Split(';');
                foreach (var str in tmp)
                {
                    var tmp2 = str.Trim(' ');
                    var tmp3 = tmp2.Split('=');
                    if (tmp3.Count() == 2 && tmp3[0].ToLower() == "name")
                        return tmp3[1].PopFrontMatch("\"").PopBackMatch("\"");
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public static string PopBackMatch(this string self, string str)
        {
            if (str.Length > self.Length)
                return self;
            else if (self.LastIndexOf(str) == self.Length - str.Length)
                return self.Substring(0, self.Length - str.Length);
            else
                return self;
        }

        public static string PopFront(this string self, int count = 1)
        {
            if (count > self.Length || count < 0)
                throw new IndexOutOfRangeException();
            return self.Substring(count);
        }

        public static string PopBack(this string self, int count = 1)
        {
            if (count > self.Length || count < 0)
                throw new IndexOutOfRangeException();
            return self.Substring(0, self.Length - count);
        }
        public static string PopFrontMatch(this string self, string str)
        {
            if (str.Length > self.Length)
                return self;
            else if (self.IndexOf(str) == 0)
                return self.Substring(str.Length);
            else
                return self;
        }
        public static string GetFileName(this IFormFile self)
        {
            try
            {
                var tmp = self.ContentDisposition.Split(';');
                foreach (var str in tmp)
                {
                    var tmp2 = str.Trim(' ');
                    var tmp3 = tmp2.Split('=');
                    if (tmp3.Count() == 2 && tmp3[0].ToLower() == "filename")
                        return tmp3[1].PopFrontMatch("\"").PopBackMatch("\"");
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static bool SaveFile(this IFormFile file)
        {
            try
            {
                var filename = ContentDispositionHeaderValue
                               .Parse(file.ContentDisposition)
                               .FileName
                               .Trim('"');
                string newFileName = Path.GetFileNameWithoutExtension(filename) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(filename);
                filename = PathHelper.CombineWithBaseDirectory(newFileName);

                using (FileStream fs = System.IO.File.Create(filename))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }

                return true;
            }
            catch (Exception)
            {

                throw;
            }
            return false;

        }

        public static bool SaveAs(this IFormFile file, string saveFileName)
        {
            try
            {

                if (file.Length > 0)
                {
                    //path = Path.GetExtension(path).IsNullOrWhiteSpace() ? Path.Combine(path, file.FileName) : path;
                    //using (var fileStream = new FileStream(path, FileMode.Create))
                    //{
                    //    file.CopyTo(fileStream);
                    //}
                    using (FileStream fs = System.IO.File.Create(saveFileName))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                }
                

                return true;
            }
            catch (Exception)
            {

                throw;
            }
            

        }


    }
}
