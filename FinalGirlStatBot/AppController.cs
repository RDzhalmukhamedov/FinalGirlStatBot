using Microsoft.AspNetCore.Mvc;

namespace FinalGirlStatBot;

public class AppController : Controller
{
    [HttpGet]
    public string Index() => "It's Final Girl";
}
