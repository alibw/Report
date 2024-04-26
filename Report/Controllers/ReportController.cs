
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Report.Models;

namespace Report.Controllers
{
    public class ReportController : Controller
    {
        public string userFullName = "علی بوالحسنی";
        public string userEmpId = "411911";
        private string systemdateformat = "mm/dd/yyyy";
        private PersianCalendar persianCalendar = new PersianCalendar();

        public ActionResult Index() => View(GetCartsFromBeginingOfMonth());

        [HttpGet]
        [Route("AddOffDays")]
        public ActionResult AddOffDays(int fromDate, int toDate = 0, string beginTime = "", string endTime = "")
        {
            if (toDate == 0)
                toDate = fromDate;
            OffDays(fromDate, toDate, beginTime, endTime);
            return View("GetCarts", CartList(fromDate, toDate));
        }

        [HttpGet]
        public ActionResult GetCarts(int fromDate, int toDate, bool showall = false) => View(CartList(fromDate, toDate, showall));

        [HttpGet]
        public string SaveCarts(
          int fromYear,
          int fromMonth,
          int fromDay,
          int toYear,
          int toMonth,
          int toDay)
        {
            string contents = JsonConvert.SerializeObject((object)GenerateCarts(fromYear, fromMonth, fromDay, toYear, toMonth, toDay));
            string path1 = AppDomain.CurrentDomain.BaseDirectory + "carts.json";
            string path2 = AppDomain.CurrentDomain.BaseDirectory + "backupcarts.json";
            System.IO.File.Delete(path1);
            System.IO.File.WriteAllText(path1, contents);
            System.IO.File.WriteAllText(path2, contents);
            return contents;
        }

        public List<NewCart> GenerateCarts(
          int fromYear,
          int fromMonth,
          int fromDay,
          int toYear,
          int toMonth,
          int toDay)
        {
            List<NewCart> carts = new List<NewCart>();
            DateTime time = persianCalendar.ToDateTime(fromYear, fromMonth, fromDay, 0, 0, 0, 0);
            DateTime dateTime = persianCalendar.ToDateTime(toYear, toMonth, toDay, 0, 0, 0, 0);
            Random random = new Random();
            for (; time.CompareTo(dateTime) != 0; time = time.AddDays(1.0))
            {
                int num1;
                string str1;
                if (persianCalendar.GetMonth(time) >= 10)
                {
                    num1 = persianCalendar.GetMonth(time);
                    str1 = num1.ToString();
                }
                else
                {
                    num1 = persianCalendar.GetMonth(time);
                    str1 = "0" + num1.ToString();
                }
                string str2 = str1;
                string str3;
                if (persianCalendar.GetDayOfMonth(time) >= 10)
                {
                    num1 = persianCalendar.GetDayOfMonth(time);
                    str3 = num1.ToString();
                }
                else
                {
                    num1 = persianCalendar.GetDayOfMonth(time);
                    str3 = "0" + num1.ToString();
                }
                string str4 = str3;
                num1 = persianCalendar.GetYear(time);
                int num2 = int.Parse(string.Format("{0}{1}{2}", num1.ToString(), str2, str4));
                int num3 = random.Next(20,59);
                int num4 = random.Next(0, 59);
                while (num4 == num3)
                    num4 = random.Next(0, 59);
                string str5 = "17";
                NewCart newCart = new NewCart()
                {
                    empId = userEmpId,
                    fullName = userFullName,
                    beginTime = string.Format("07:{0}", num3 < 10 ? "0" + num3.ToString() : (object)num3.ToString()),
                    endTime = string.Format("{0}:{1}", str5, num4 < 10 ? "0" + num4.ToString() : (object)num4.ToString()),
                    date = num2
                };
                if (persianCalendar.GetDayOfWeek(time) == DayOfWeek.Thursday || persianCalendar.GetDayOfWeek(time) == DayOfWeek.Friday)
                {
                    newCart.beginTime = "";
                    newCart.endTime = "";
                }
                carts.Add(newCart);
            }
            return carts;
        }

        public List<NewCart> CartList(int fromDate, int todate, bool showall = false)
        {
            NewCart[] cartsFromFile = GetCartsFromFile();
            IEnumerable<NewCart> source = fromDate == todate ? cartsFromFile.Where(t => t.date == fromDate) : cartsFromFile.Where(t => t.date < todate && t.date >= fromDate);
            if (!showall)
                source = source.Where(t => t.beginTime != string.Empty);
            return source.OrderBy((Func<NewCart, int>)(t => t.date)).ToList();
        }

        public List<NewCart> GetCartsFromBeginingOfMonth()
        {
            DateTime now = DateTime.Now;
            string str1 = persianCalendar.GetMonth(now) < 10 ? "0" + persianCalendar.GetMonth(now).ToString() : persianCalendar.GetMonth(now).ToString();
            string s = persianCalendar.GetDayOfMonth(now) < 10 ? "0" + persianCalendar.GetDayOfMonth(now).ToString() : persianCalendar.GetDayOfMonth(now).ToString();
            int todate = int.Parse(string.Format("{0}{1}{2}", persianCalendar.GetYear(now).ToString(), str1, s));
            DateTime time = now.AddDays(-1 * int.Parse(s) + 1);
            string str2 = persianCalendar.GetMonth(time) < 10 ? "0" + persianCalendar.GetMonth(time).ToString() : persianCalendar.GetMonth(time).ToString();
            string str3 = persianCalendar.GetDayOfMonth(time) < 10 ? "0" + persianCalendar.GetDayOfMonth(time).ToString() : persianCalendar.GetDayOfMonth(time).ToString();
            return CartList(int.Parse(string.Format("{0}{1}{2}", persianCalendar.GetYear(time).ToString(), str2, str3)), todate);
        }

        public NewCart[] GetCartsFromFile() => JsonConvert.DeserializeObject<NewCart[]>(System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "carts.json"));

        public void OffDays(int fromDate, int toDate = 0, string beginTime = "", string endTime = "")
        {
            NewCart[] cartsFromFile;
            List<NewCart> list = (cartsFromFile = GetCartsFromFile()).ToList();
            if (toDate != fromDate)
                list.RemoveAll(t => t.date < toDate && t.date >= fromDate);
            else
                list.RemoveAll(t => t.date == fromDate);
            if (!string.IsNullOrWhiteSpace(beginTime))
            {
                Random random = new Random();
                int num1 = random.Next(0, 59);
                int num2 = random.Next(0, 59);
                while (num2 == num1)
                    num2 = random.Next(0, 59);
                string str1 = num1 > 30 ? "18" : "17";
                if (beginTime.Length == 4)
                    beginTime = "0" + beginTime;
                if (endTime.Length == 4)
                    endTime = "0" + endTime;
                if (beginTime == "09:00" && endTime != "18:00" || beginTime == "08:00" && endTime != "17:00")
                {
                    string str2 = "18";
                    NewCart newCart = new NewCart()
                    {
                        empId = userEmpId,
                        fullName = userFullName,
                        beginTime = endTime,
                        endTime = string.Format("{0}:{1}", str2, num2 < 10 ? "0" + num2.ToString() : (object)num2.ToString()),
                        date = fromDate
                    };
                    list.Add(newCart);
                }
                else if (beginTime != "09:00" && endTime == "18:00" || beginTime != "08:00" && endTime == "17:00")
                {
                    NewCart newCart = new NewCart()
                    {
                        empId = userEmpId,
                        fullName = userFullName,
                        beginTime = string.Format("08:{0}", num1 < 10 ? "0" + num1.ToString() : (object)num1.ToString()),
                        endTime = beginTime,
                        date = fromDate
                    };
                    list.Add(newCart);
                }
                else if ((!(beginTime == "09:00") || !(endTime == "18:00")) && (!(beginTime == "08:00") || !(endTime == "17:00")))
                {
                    NewCart newCart1 = new NewCart()
                    {
                        empId = userEmpId,
                        fullName = userFullName,
                        beginTime = string.Format("08:{0}", num1 < 10 ? "0" + num1.ToString() : (object)num1.ToString()),
                        endTime = beginTime,
                        date = fromDate
                    };
                    NewCart newCart2 = new NewCart()
                    {
                        empId = userEmpId,
                        fullName = userFullName,
                        beginTime = endTime,
                        endTime = string.Format("{0}:{1}", str1, num2 < 10 ? "0" + num2.ToString() : (object)num2.ToString()),
                        date = fromDate
                    };
                    list.Add(newCart1);
                    list.Add(newCart2);
                }
            }
            string contents1 = JsonConvert.SerializeObject((object)list);
            string contents2 = JsonConvert.SerializeObject((object)cartsFromFile);
            string path1 = AppDomain.CurrentDomain.BaseDirectory + "carts.json";
            string path2 = AppDomain.CurrentDomain.BaseDirectory + "backupcarts.json";
            System.IO.File.Delete(path1);
            System.IO.File.Delete(path2);
            System.IO.File.WriteAllText(path1, contents1);
            System.IO.File.WriteAllText(path2, contents2);
        }
    }
}
