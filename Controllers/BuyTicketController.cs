using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace BusBookingSystem.Controllers
{
    public class BuyTicketController: BaseController
    {
        private readonly string _connection;

        public BuyTicketController(IConfiguration configuration){
            _connection= configuration.GetConnectionString("DefaultConnection")!;
        }
        //GET
        public IActionResult Buy(){
            if(!IsLoggedIn())
            return RedirectToAction("Login", "LogIn");
            return View();
        }

        [HttpPost]
        //Buying a ticket in general not for a specific trip the user choose the type of ticket
        public IActionResult Buy(string duration){
            //recheck if user logged in more secure
            if(!IsLoggedIn())
            return RedirectToAction("Login", "LogIn");
            try{

            //get the userId
            int userId= GetUserId();

            string type;
            decimal price;
            DateTime expiresAt;

            if(duration == "1day"){
                type="DAY";
                price=5;
                expiresAt=DateTime.Now.AddDays(1);
            }
            else if(duration =="1week"){
                type="WEEK";
                price=20;
                expiresAt=DateTime.Now.AddDays(7);
            }
            else{
                type="MONTH";
                price=60;
                expiresAt=DateTime.Now.AddMonths(1);
            }
            using var conn=new MySqlConnection(_connection);
            conn.Open();

    string query=@"INSERT INTO Ticket
    (UserId, Type, Price, IsActive, ActivatedAt, ExpiresAt)
    VALUES(@UserId, @Type, @Price, @IsActive, NULL, @ExpiresAt)";

    using (var cmd=new MySqlCommand(query,conn)){
    
    cmd.Parameters.AddWithValue("@UserId",userId);
    cmd.Parameters.AddWithValue("@Type", type);
    cmd.Parameters.AddWithValue("@Price", price);
    cmd.Parameters.AddWithValue("@IsActive", false);
    cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);

    cmd.ExecuteNonQuery();

    }
    return RedirectToAction("Index","Home");
    }catch(Exception ex){
        Console.WriteLine(ex.Message);
        return View("Error");
    }

        
    }
}

}