﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminModule;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("admin/charities")]
    public class CharityController : Controller
    {
        private readonly ICharityRepository _repository;

        public CharityController(ICharityRepository repository)
        {
            _repository = repository;
        }
        public class CharityGridRow
        {
            public int Id { get; set; }
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Bank_name { get; set; }
            public string? Bank_account_no { get; set; }
            public string? Bank_bic { get; set; }
            public static CharityGridRow Create(Charity c)
                => new CharityGridRow
                {
                    Id = c.Charity_id,
                    Code = c.Charity_ext_id,
                    Name = c.Name,
                    Bank_name = c.Bank_name,
                    Bank_account_no = c.Bank_account_no,
                    Bank_bic = c.Bank_bic
                };
        }

        [HttpGet]
        public async Task<IEnumerable<CharityGridRow>> GetCharities()
        {
            var res = await _repository.GetCharities();
            return res.Select(CharityGridRow.Create);
        }

    }
}
