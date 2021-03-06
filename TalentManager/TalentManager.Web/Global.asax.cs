﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TalentManager.Data;
using System.Data.Entity;
using TalentManager.Web.Models;
using AutoMapper;
using TalentManager.Domain;
using TalentManager.Web.App_Start;

namespace TalentManager.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Database.SetInitializer<Context>(null);
            IocConfig.RegisterDependencyResolver(GlobalConfiguration.Configuration);
            Mapper.Initialize(c => 
                {
                    c.CreateMap<EmployeeDto, Employee>();
                    c.CreateMap<Employee, EmployeeDto>();
                }
                );
        }
    }
}