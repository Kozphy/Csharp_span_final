﻿using GoSweet.Controllers.feature;
using GoSweet.Models;
using GoSweet.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace GoSweet.Controllers
{
    public class FirmController : Controller
    {
        private readonly ShopwebContext _context;
        private readonly ILogger<FirmController> _logger;
        private readonly HashPassword _hashPasswordBuilder = new HashPassword();

        public FirmController(ShopwebContext context, ILogger<FirmController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public class global
        {
            public static string? StartDateString { get; set; }

            public static string? EndDateString { get; set; }

            public static DateTime Now
            {
                get { return DateTime.Now; }
            }

            public static DateTime StartDate { get; set; }

            public static DateTime EndDate { get; set; }
        }

        public IActionResult Homepage()
        {
            int? id = HttpContext.Session.GetInt32("fnumber")!;
            if (id is null)
            {
                return RedirectToAction("Login", "Firm");
            }

            #region 日期參數
            DateTime MonthBegin = global.Now.AddDays(1 - global.Now.Day);
            DateTime LastMonthEnd = global.Now.AddDays(1 - global.Now.Day).AddDays(-1);
            DateTime LastMonthBegin = global.Now.AddDays(1 - global.Now.Day).AddMonths(-1);
            DateTime ThreeMonthLater = global.Now.AddMonths(-3);
            #endregion

            #region 廠商圖片
            var Firmpic = _context
                .FirmPagetables
                .Where(x => x.FNumber == id)
                .Select(x => x.FPicurl)
                .FirstOrDefault();
            if (Firmpic is null)
            {
                Firmpic = "/img/Nopic.jpg";
            }
            #endregion

            #region 當月訂單數
            string TotalOrders = (
                from someone in _context.OrderDatatables
                where
                    someone.OStart <= global.Now
                    && someone.OStart >= MonthBegin
                    && someone.FNumber == id
                select someone.ONumber
            )
                .Count()
                .ToString("N0");
            #endregion

            #region 當月出貨數
            string TotalShipped = (
                from someone in _context.OrderDatatables
                where
                    someone.OStart <= global.Now
                    && someone.OStart >= MonthBegin
                    && someone.OShipstatus.Contains("已")
                    && someone.FNumber == id
                select someone.ONumber
            )
                .Count()
                .ToString("N0");
            #endregion

            #region 當月未出貨數
            string TotalunShipped = (
                from someone in _context.OrderDatatables
                where
                    someone.OStart <= global.Now
                    && someone.OStart >= MonthBegin
                    && someone.OShipstatus.Contains("未")
                    && someone.FNumber == id
                select someone.ONumber
            )
                .Count()
                .ToString("N0");
            #endregion

            #region 當月總收入
            string TotalThisMonth = (
                from someone in _context.OrderDatatables
                where
                    someone.OStart <= global.Now
                    && someone.OStart >= MonthBegin
                    && someone.FNumber == id
                select (someone.OBuynumber * someone.OPrice)
            )
                .Sum()
                .ToString("C0");
            #endregion

            #region 上月訂單數
            string LastMonthTotalOrders = (
                from someone in _context.OrderDatatables
                where
                    someone.OStart <= LastMonthEnd
                    && someone.OStart >= LastMonthBegin
                    && someone.FNumber == id
                select someone.ONumber
            )
                .Count()
                .ToString("N0");
            #endregion

            #region 上月出貨數
            string LastMonthTotalShipped = (
                from someone in _context.OrderDatatables
                where
                    someone.OStart <= LastMonthEnd
                    && someone.OStart >= LastMonthBegin
                    && someone.OShipstatus.Contains("已")
                    && someone.FNumber == id
                select someone.ONumber
            )
                .Count()
                .ToString("N0");
            #endregion

            #region 上月未出貨數
            string LastMonthTotalunShipped = (
                from someone in _context.OrderDatatables
                where
                    someone.OStart <= LastMonthEnd
                    && someone.OStart >= LastMonthBegin
                    && someone.OShipstatus.Contains("未")
                    && someone.FNumber == id
                select someone.ONumber
            )
                .Count()
                .ToString("N0");
            #endregion

            #region 上月總收入
            string TotalLastMonth = (
                from someone in _context.OrderDatatables
                where
                    someone.OStart <= LastMonthEnd
                    && someone.OStart >= LastMonthBegin
                    && someone.FNumber == id
                select (someone.OBuynumber * someone.OPrice)
            )
                .Sum()
                .ToString("C0");
            #endregion

            #region 待處理訂單
            var Waitinglist =
                from O in _context.OrderDatatables
                join P in _context.ProductDatatables on O.PNumber equals P.PNumber
                where O.OShipstatus.Contains("未") && O.FNumber == id
                orderby O.OStart
                select new WaitingLists
                {
                    OrderDate = O.OStart.ToString(),
                    OrderID = O.ONumber,
                    Products = P.PName
                };
            #endregion 直接使用Model

            #region 熱門評論
            var Comment =
                from someone in _context.OrderAssesstables
                join something in _context.OrderDatatables
                    on someone.ONumber equals something.ONumber
                where something.FNumber == id
                orderby something.OStart descending
                select new RecentlyComment
                {
                    CommentDate = something.OStart.ToString(),
                    Content = someone.OCcomment,
                    Rating = someone.OCscore
                };
            #endregion 預計也是使用Model

            #region 熱門商品
            var Hotsale =
                from someone in _context.OrderDatatables
                join something in _context.ProductDatatables
                    on someone.PNumber equals something.PNumber
                where someone.FNumber == id && someone.OStart > ThreeMonthLater
                select new { something, someone } into tempTable
                group tempTable by tempTable.something.PName into TempTable
                select new HotSales
                {
                    ProductName = TempTable.FirstOrDefault().something.PName,
                    Quentity = TempTable.Sum(x => x.someone.OBuynumber)
                };
            Hotsale = Hotsale.OrderByDescending(x => x.Quentity);
            #endregion 預計也是使用Model

            #region 建立Model存放資料
            FirmHomepageModel HomepageModels = new FirmHomepageModel
            {
                PicturePath = Firmpic,
                //類別賦值-當月訂單
                ThisMonthOrdersTotal = TotalOrders,
                //類別賦值-當月出貨
                ThisMonthShippedTotal = TotalShipped,
                //類別賦值-當月未出貨
                ThisMonthunShippedTotal = TotalunShipped,
                //類別賦值-當月收入
                ThisMonthReveune = TotalThisMonth,
                //類別賦值-上月訂單
                LastMonthOrdersTotal = LastMonthTotalOrders,
                //類別賦值-上月出貨
                LastMonthShippedTotal = LastMonthTotalShipped,
                //類別賦值-上月未出貨
                LastMonthunShippedTotal = LastMonthTotalunShipped,
                //類別賦值-上月收入
                LastMonthReveune = TotalLastMonth,
                //類別賦值-待處理訂單-取前五
                WaitingList = Waitinglist.Take(5).ToList(),
                //類別賦值-熱門評論-取前十
                RecentlyComments = Comment.Take(10).ToList(),
                //類別賦值-熱門商品-取前十
                HotSale = Hotsale.Take(10).ToList(),

                // bell dropdown message
                //FirmBellDropDownDatas = GetBellDropdownMessage()
            };
            #endregion

            // 廠商通知到 _LayoutFirm
            GetBellDropdownMessage();

            RatingJson();
            return View(HomepageModels);
        }

        public IEnumerable<FirmBellDropDownVm>? GetBellDropdownMessage()
        {
            string firmAccount = HttpContext.Session.GetString("firmAccount")!;
            if (firmAccount == null)
            {
                return null;
            }

            #region get firm bellMessage info
            IEnumerable<FirmBellDropDownVm> firmNotifyMessageQuery = (
                from notify in _context.NotifyDatatables
                join order in _context.OrderDatatables on notify.ONumber equals order.ONumber
                join firm in _context.FirmAccounttables on notify.FNumber equals firm.FNumber
                join product in _context.ProductDatatables on order.PNumber equals product.PNumber
                where
                    (notify.OStatus == "已下單" || notify.OStatus == "已取貨")
                    && firm.FAccount == firmAccount
                    && notify.NRead == false
                select new FirmBellDropDownVm
                {
                    //OrderNumber = notify.ONumber,
                    OrderEnd = order.OEnd,
                    ProductName = product.PName,
                    OrderStatus = notify.OStatus,
                    NotifyRead = notify.NRead,
                }
            );
            #endregion

            IEnumerable<FirmBellDropDownVm> firmNotifyMessages = firmNotifyMessageQuery.ToList();
            HttpContext
                .Session
                .SetString("NotifyMessages", JsonConvert.SerializeObject(firmNotifyMessages));
            HttpContext.Session.SetInt32("NotifyMessagesCount", firmNotifyMessages.Count());

            return firmNotifyMessages;
        }

        [HttpPost]
        public IActionResult BellMessageHaveRead()
        {
            string firmAccount = HttpContext.Session.GetString("firmAccount")!;

            // 取得廠商通知
            var firmNotifyMessageQuery =
                from notify in _context.NotifyDatatables
                join order in _context.OrderDatatables on notify.ONumber equals order.ONumber
                join firm in _context.FirmAccounttables on notify.FNumber equals firm.FNumber
                join product in _context.ProductDatatables on order.PNumber equals product.PNumber
                where
                    (notify.OStatus == "已下單" || notify.OStatus == "已取貨")
                    && firm.FAccount == firmAccount
                select notify;

            foreach (var item in firmNotifyMessageQuery)
            {
                item.NRead = true;
            }

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return RedirectToAction(nameof(GetBellDropdownMessage));
        }

        public JsonResult RatingJson()
        {
            int? id = HttpContext.Session.GetInt32("fnumber")!;

            var somebody = (
                from someone in _context.OrderAssesstables
                join someelse in _context.OrderDatatables on someone.ONumber equals someelse.ONumber
                where someelse.FNumber == id
                select someone.OCscore
            ).Sum();
            var something = (
                from someone in _context.OrderAssesstables
                join someelse in _context.OrderDatatables on someone.ONumber equals someelse.ONumber
                where someelse.FNumber == id
                select someone.OCscore
            ).Count();
            ratingvalue rateValue = new ratingvalue
            {
                ratingScore = somebody,
                ratingQuentity = something
            };

            return Json(rateValue);
        }

        public IActionResult Revenue()
        {
            int? id = HttpContext.Session.GetInt32("fnumber")!;
            if (id is null)
            {
                return RedirectToAction("Login", "Firm");
            }

            #region 日期設定
            global.StartDateString = HttpContext.Request.Query["StartDate"];
            global.EndDateString = HttpContext.Request.Query["EndDate"];
            if (
                global.StartDateString is null
                || global.EndDateString is null
                || global.StartDateString == ""
                || global.EndDateString == ""
            )
            {
                global.StartDate = global.Now.AddDays(-90);
                global.EndDate = global.Now;
            }
            else
            {
                global.StartDate = Convert.ToDateTime(global.StartDateString);
                global.EndDate = Convert.ToDateTime(global.EndDateString);
            }
            ViewData["Start"] = global.StartDate.ToString("yyyy-MM-dd");
            ViewData["End"] = global.EndDate.ToString("yyyy-MM-dd");
            #endregion 日期預設跟GET接收

            #region 期間帳務
            var Period =
                from someone in _context.OrderDatatables
                where
                    someone.OStart >= global.StartDate
                    && someone.OStart <= global.EndDate
                    && someone.FNumber == id
                select someone.OPrice * someone.OBuynumber;
            //期間結餘
            ViewData["Revenue"] = Period.Sum().ToString("C0");

            // 期間收入
            ViewData["Income"] = Period.Where(Money => Money >= 0).Sum().ToString("C0");

            //期間支出
            ViewData["Expenses"] = Period.Where(Money => Money < 0).Sum().ToString("C0");
            #endregion

            JsonData();
            return View();
        }

        public JsonResult JsonData()
        {
            int? id = HttpContext.Session.GetInt32("fnumber")!;

            var somebody =
                from someone in _context.OrderDatatables
                join something in _context.ProductDatatables
                    on someone.PNumber equals something.PNumber
                where
                    someone.OStart >= global.StartDate
                    && someone.OStart < global.EndDate
                    && someone.FNumber == id
                orderby someone.OStart
                select new RevenueSearch
                {
                    orderDate = someone.OStart.ToString(),
                    name = something.PName,
                    orderType = someone.OType ? "團購" : "直購",
                    shipState = someone.OShipstatus,
                    categories = something.PCategory,
                    id = someone.ONumber,
                    quentity = someone.OBuynumber,
                    price = someone.OPrice,
                    total = decimal.Round(someone.OBuynumber * someone.OPrice, 2)
                };
            return Json(somebody.ToList<RevenueSearch>());
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(FirmAccountLoginVm firmLoginData)
        {
            if (ModelState.IsValid == false)
                return View();

            // create hashPassword with salt
            string hashPassword = _hashPasswordBuilder.CreateSha256Password(
                firmLoginData.FPassword!
            );
            firmLoginData.FPassword = hashPassword;

            // get database firm account data
            var firmAccountQuery = _context
                .FirmAccounttables
                .Where(
                    (f) =>
                        f.FAccount.Equals(firmLoginData.FAccount)
                        && f.FPassword.Equals(firmLoginData.FPassword)
                )
                .Select(
                    (f) =>
                        new
                        {
                            Account = f.FAccount,
                            AccountName = f.FNickname,
                            f_number = f.FNumber,
                        }
                );

            bool accountNotExist = firmAccountQuery.IsNullOrEmpty();

            if (accountNotExist.Equals(true))
            {
                TempData["firmAccountNotExistMessage"] = "帳號不存在或密碼錯誤";
                return RedirectToAction("Login");
            }

            var firmAccount = firmAccountQuery.First();

            HttpContext.Session.SetString("firmAccount", firmAccount.Account);
            HttpContext.Session.SetString("firmAccountName", firmAccount.AccountName);
            HttpContext.Session.SetInt32("firmNumber", firmAccount.f_number);
            HttpContext.Session.SetInt32("fnumber", firmAccount.f_number);
            HttpContext.Session.SetInt32("myfnumber", firmAccount.f_number);
            HttpContext.Session.SetString("fnickname", firmAccount.AccountName);
            TempData["firmAccountLoginSuccessMessage"] = "帳號登入成功";

            return RedirectToAction("Homepage", "Firm");
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(FirmAccountVm firmAccountData)
        {
            if (ModelState.IsValid == false)
                return View(firmAccountData);

            // create hashPassword with salt
            string hashPassword = _hashPasswordBuilder.CreateSha256Password(
                firmAccountData.FPassword!
            );
            firmAccountData.FPassword = hashPassword;

            bool accountNotExist = _context
                .FirmAccounttables
                .Where((f) => f.FAccount.Equals(firmAccountData.FAccount))
                .IsNullOrEmpty();

            if (accountNotExist.Equals(false))
            {
                TempData["firmAccountExistMessage"] = "此帳號已被註冊";
                RedirectToAction("SignUp");
                return View();
            }

            try
            {
                CreateFirmAccount(firmAccountData);
                TempData["firmSignUpSuccessMessage"] = "帳號註冊成功";
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }

            return RedirectToAction("Login");
        }

        private void CreateFirmAccount(FirmAccountVm firmAccountData)
        {
            FirmAccounttable firmAccount = new FirmAccounttable()
            {
                FNickname = firmAccountData.FNickname,
                FAccount = firmAccountData.FAccount,
                FPassword = firmAccountData.FPassword,
                FMailpass = firmAccountData.FMailpass,
            };
            _context.FirmAccounttables.Add(firmAccount);
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult SendMail(string emailAddress)
        {
            if (emailAddress == null) throw new ArgumentNullException(nameof(emailAddress));
            if (ModelState.IsValid == false)
                return View();

            if (emailAddress.IsNullOrEmpty())
            {
                return RedirectToAction("Login");
            }

            // 寄送 email 之前先檢查 email 是否存在
            var firmAccountQuery = _context
                .FirmAccounttables
                .Where(c => c.FAccount.Equals(emailAddress));

            bool firmAccountNotExist = firmAccountQuery.IsNullOrEmpty();

            if (firmAccountNotExist.Equals(true))
            {
                TempData["firmAccountNotExistMessage"] = "帳號不存在";
                return RedirectToAction("Login");
            }

            _logger.LogDebug(ControllerContext.ActionDescriptor.ControllerName);
            Mail mailHandler = new Mail(
                emailAddress,
                ControllerContext.ActionDescriptor.ControllerName
            );
            try
            {
                mailHandler.SendMail();
                TempData["sendEmailSuccessMessage"] = $"Send Email to {emailAddress} Success";
            }
            catch (Exception ex)
            {
                TempData["sendEmailFailMessage"] = $"Send Email to {emailAddress} fail";
                return StatusCode(500, ex.Message);
            }

            return RedirectToAction("Login");
        }

        public IActionResult ResetPassword(string EmailAddress)
        {
            ViewBag.EmailAddress = EmailAddress;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordVm resetPasswordData)
        {
            ViewBag.EmailAddress = resetPasswordData.EmailAddress;
            if (!ModelState.IsValid)
                return View(resetPasswordData);

            if (resetPasswordData.NewPassword != resetPasswordData.CheckNewPassword)
            {
                TempData["CheckPasswordNotEqualMessage"] = "輸入的密碼不相符";
                return View(resetPasswordData);
            }

            var accountQuery = _context
                .FirmAccounttables
                .Where((f) => f.FAccount.Equals(resetPasswordData.EmailAddress));
            var account = accountQuery.First();

            // create hashPassword with salt
            string hashPassword = _hashPasswordBuilder.CreateSha256Password(
                resetPasswordData.NewPassword!
            );
            account.FPassword = hashPassword;

            try
            {
                _context.SaveChanges();
                TempData["resetPasswordSuccessMessage"] = "密碼重置成功";
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return RedirectToAction("Login", "Firm");
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("firmAccountName");
            HttpContext.Session.Remove("firmAccount");
            HttpContext.Session.Remove("fnumber");
            HttpContext.Session.Remove("firmNumber");
            HttpContext.Session.Remove("fnickname");
            HttpContext.Session.SetInt32("NotifyMessagesCount", 0);
            TempData["logOutMessage"] = "登出成功";
            //HttpContext.Session.SetString("AccountName", String.Empty);
            return RedirectToAction("Index", "Home");
        }
    }
}
