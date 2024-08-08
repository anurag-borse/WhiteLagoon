using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IBookingService _bookingService;
        private readonly IVillaService _villaService;
        private readonly IVillaNumberService _villaNumberService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymentService _paymentService;

        public BookingController(IWebHostEnvironment hostingEnvironment, IVillaService villaService, IVillaNumberService villaNumberService,
            IBookingService bookingService, UserManager<ApplicationUser> userManager, IPaymentService paymentService)
        {
            _hostingEnvironment = hostingEnvironment;
            _villaService = villaService;
            _villaNumberService = villaNumberService;
            _bookingService = bookingService;
            _userManager = userManager;
            _paymentService = paymentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser user = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult();

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _villaService.GetVillaById(villaId),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name
            };

            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _villaService.GetVillaById(booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;

            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            if (!_villaService.IsVillaAvailableByDate(villa.Id, booking.Nights, booking.CheckInDate))
            {
                TempData["error"] = "Room has been sold out!";
                //no rooms available
                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = booking.VillaId,
                    checkInDate = booking.CheckInDate,
                    nights = booking.Nights
                });
            }

            _bookingService.CreateBooking(booking);

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";

            var options = _paymentService.CreateStripeSessionOptions(booking, villa, domain);

            var session = _paymentService.CreateStripeSession(options);

            _bookingService.UpdateStripePaymentID(booking.Id, session.Id, session.PaymentIntentId);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = _bookingService.GetBookingById(bookingId);
            if (bookingFromDb.Status == SD.StatusPending)
            {
                if (string.IsNullOrEmpty(bookingFromDb.StripeSessionId))
                {
                    // Log error or handle the case where the session ID is missing
                    throw new Exception("Stripe session ID is missing.");
                }

                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);
                if (session.PaymentStatus == "paid")
                {
                    _bookingService.UpdateStatus(bookingFromDb.Id, SD.StatusApproved, 0);
                    _bookingService.UpdateStripePaymentID(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                }
            }

            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _bookingService.GetBookingById(bookingId);

            if (bookingFromDb == null)
            {
                // Handle the case where the booking is not found
                return NotFound();
            }

            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);
                bookingFromDb.VillaNumbers = _villaNumberService.GetAllVillaNumbers().Where(u => u.VillaId == bookingFromDb.VillaId
                && availableVillaNumber.Any(x => x == u.Villa_Number)).ToList();
            }

            return View(bookingFromDb);
        }

        [HttpPost]
        [Authorize]
        public IActionResult GenerateInvoice(int id, string downloadType)
        {
            string basePath = _hostingEnvironment.WebRootPath;
            WordDocument document = new WordDocument();

            //load the template document
            string dataPath = basePath + @"/exports/BookingDetails.docx";
            using FileStream fileStream = new(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            document.Open(fileStream, FormatType.Automatic);

            // update the template document
            Booking bookingFromDb = _bookingService.GetBookingById(id);

            TextSelection textSelection = document.Find("xx_customer_name", false, true);
            WTextRange wTextRange = textSelection.GetAsOneRange();
            wTextRange.Text = bookingFromDb.Name;

            textSelection = document.Find("xx_customer_phone", false, true);
            wTextRange = textSelection.GetAsOneRange();
            wTextRange.Text = bookingFromDb.Phone;

            textSelection = document.Find("xx_customer_email", false, true);
            wTextRange = textSelection.GetAsOneRange();
            wTextRange.Text = bookingFromDb.Email;

            textSelection = document.Find("XX_BOOKING_NUMBER", false, true);
            wTextRange = textSelection.GetAsOneRange();
            wTextRange.Text = "BOOKING NUMBER - " + bookingFromDb.Id;

            textSelection = document.Find("XX_BOOKING_DATE", false, true);
            wTextRange = textSelection.GetAsOneRange();
            wTextRange.Text = "BOOKING DATE - " + bookingFromDb.BookingDate.ToShortDateString();

            textSelection = document.Find("xx_payment_date", false, true);
            wTextRange = textSelection.GetAsOneRange();
            wTextRange.Text = bookingFromDb.PaymentDate.ToShortDateString();

            textSelection = document.Find("xx_checkin_date", false, true);
            wTextRange = textSelection.GetAsOneRange();
            wTextRange.Text = bookingFromDb.CheckInDate.ToShortDateString();

            textSelection = document.Find("xx_checkout_date", false, true);
            wTextRange = textSelection.GetAsOneRange();
            wTextRange.Text = bookingFromDb.CheckOutDate.ToShortDateString();

            textSelection = document.Find("xx_booking_total", false, true);
            wTextRange = textSelection.GetAsOneRange();
            wTextRange.Text = bookingFromDb.TotalCost.ToString("c");

            WTable table = new(document);
            table.TableFormat.Borders.LineWidth = 1.0f;
            table.TableFormat.Borders.Color = Color.Black;
            table.TableFormat.Paddings.Top = 7f;
            table.TableFormat.Paddings.Bottom = 7f;
            table.TableFormat.Borders.Horizontal.LineWidth = 1.0f;

            int row = bookingFromDb.VillaNumber > 0 ? 3 : 2;

            table.ResetCells(row, 4);

            WTableRow row0 = table.Rows[0];
            row0.Cells[0].AddParagraph().AppendText("NIGHTS");
            row0.Cells[0].Width = 80;
            row0.Cells[1].AddParagraph().AppendText("VILLA");
            row0.Cells[1].Width = 220;
            row0.Cells[2].AddParagraph().AppendText("PRICE PER NIGHT");
            row0.Cells[3].AddParagraph().AppendText("TOTAl");
            row0.Cells[3].Width = 80;

            WTableRow row1 = table.Rows[1];
            row1.Cells[0].AddParagraph().AppendText(bookingFromDb.Nights.ToString());
            row1.Cells[0].Width = 80;
            row1.Cells[1].AddParagraph().AppendText(bookingFromDb.Villa.Name);
            row1.Cells[1].Width = 220;
            row1.Cells[2].AddParagraph().AppendText((bookingFromDb.TotalCost / bookingFromDb.Nights).ToString("c"));
            row1.Cells[3].AddParagraph().AppendText(bookingFromDb.TotalCost.ToString("c"));
            row1.Cells[3].Width = 80;

            if (bookingFromDb.VillaNumber > 0)
            {
                WTableRow row2 = table.Rows[2];
                row2.Cells[0].Width = 80;

                row2.Cells[1].AddParagraph().AppendText("Villa Number - " + bookingFromDb.VillaNumber.ToString());
                row2.Cells[1].Width = 220;
                row2.Cells[3].Width = 80;
            }

            WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
            tableStyle.TableProperties.RowStripe = 1;
            tableStyle.TableProperties.ColumnStripe = 2;
            tableStyle.TableProperties.Paddings.Top = 2;
            tableStyle.TableProperties.Paddings.Bottom = 1;
            tableStyle.TableProperties.Paddings.Left = 5.4f;
            tableStyle.TableProperties.Paddings.Right = 5.4f;

            ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
            firstRowStyle.CharacterFormat.Bold = true;
            firstRowStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255);
            firstRowStyle.CellProperties.BackColor = Color.Black;

            table.ApplyStyle("CustomStyle");

            TextBodyPart bodyPart = new(document);
            bodyPart.BodyItems.Add(table);

            // Replace the text in the template document with the booking details
            document.Replace("<ADDTABLEHERE>", bodyPart, false, false);

            using DocIORenderer docIORenderer = new();
            MemoryStream stream = new();
            if (downloadType == "word")
            {
                document.Save(stream, FormatType.Docx);
                stream.Position = 0;

                return File(stream, "application/docx", "BookingDetails.docx");
            }
            else
            {
                PdfDocument pdfDocument = docIORenderer.ConvertToPDF(document);
                pdfDocument.Save(stream);
                stream.Position = 0;
                return File(stream, "application/pdf", "BookingDetails.pdf");
            }
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            // Ensure the booking ID is correctly set
            if (booking == null || booking.Id == 0)
            {
                // Handle the case where the booking is not valid
                TempData["Error"] = "Invalid booking details.";
                return RedirectToAction(nameof(Index));
            }

            _bookingService.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);

            TempData["Success"] = "Booking Updated Successfully";

            // Redirect to BookingDetails with the correct bookingId
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);

            TempData["Success"] = "Booking Completed Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, SD.StatusCancelled, booking.VillaNumber);

            TempData["Success"] = "Booking Canceled Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumber = new();

            var villaNumbers = _villaNumberService.GetAllVillaNumbers().Where(u => u.VillaId == villaId);

            var checkedInVilla = _bookingService.GetCheckedInVillaNumbers(villaId);

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumber.Add(villaNumber.Villa_Number);
                }
            }
            return availableVillaNumber;
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objbookings;
            string userId = "";
            if (string.IsNullOrEmpty(status))
            {
                status = "";
            }
            if (!User.IsInRole(SD.Role_Admin))
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            }

            objbookings = _bookingService.GetAllBookings(userId, status);

            if (!string.IsNullOrEmpty(status))
            {
                objbookings = objbookings.Where(u => u.Status.ToLower().Equals(status.ToLower()));
            }
            return Json(new { data = objbookings });
        }

        #endregion API CALLS
    }
}