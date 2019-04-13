using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using MvcWork.Models;

namespace MvcWork.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var t = Environment.GetEnvironmentVariable("ZOOKEEPER_HOME");

            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        //public async Task<IActionResult> Create([ModelBinder(BinderType = typeof(CustomModelBinder))] CreateVm vm)
        //public async Task<IActionResult> Create([Bind(include: "PartialType")]CreateVm vm)
        public async Task<IActionResult> Create(CreateVm vm)
        {
            //ModelState.Keys.Where(e=>e.StartsWith(nameof(CreateVm.PartialVm))).ToList().ForEach(e=>ModelState.Remove(e));

            await TryUpdateModelAsync(vm, string.Empty, e=>e.PartialVm, e=>e.Family);

            if (ModelState.IsValid)
            {

            }

            return View();
        }
    }

    public class CreateVm
    {
        [Required]
        public int PartialType { get; set; }

        public PartialVm PartialVm { get; set; }

        [BindingBehavior(BindingBehavior.Never)]
        public string Family { get; set; }
    }

    public class PartialVm
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }
    }

    public class CustomModelBinder : ComplexTypeModelBinder
    {
        public CustomModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders, ILoggerFactory loggerFactory)
        : base(propertyBinders, loggerFactory)
        {
        }
        protected override Task BindProperty(ModelBindingContext bindingContext)
        {
            if (bindingContext.FieldName == "BinderValue")
            {
                bindingContext.Result = ModelBindingResult.Success("BinderValueTest");
                return Task.CompletedTask;
            }
            else
            {
                return base.BindProperty(bindingContext);
            }
        }
        protected override void SetProperty(ModelBindingContext bindingContext, string modelName, ModelMetadata propertyMetadata, ModelBindingResult result)
        {
            base.SetProperty(bindingContext, modelName, propertyMetadata, result);
        }
    }
}
