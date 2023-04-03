﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace ThingsEdge.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class EdgeController : Controller
{
    [HttpPost]
    [Route("")]
    public IActionResult Post()
    {
        return Ok();
    }
}
