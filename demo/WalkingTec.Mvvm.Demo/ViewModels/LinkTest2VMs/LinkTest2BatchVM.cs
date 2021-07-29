﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Core.Extensions;
using WalkingTec.Mvvm.Demo.Models;


namespace WalkingTec.Mvvm.Demo.ViewModels.LinkTest2VMs
{
    public partial class LinkTest2BatchVM : BaseBatchVM<LinkTest2, LinkTest2_BatchEdit>
    {
        public LinkTest2BatchVM()
        {
            ListVM = new LinkTest2ListVM();
            LinkedVM = new LinkTest2_BatchEdit();
        }

    }

	/// <summary>
    /// Class to define batch edit fields
    /// </summary>
    public class LinkTest2_BatchEdit : BaseVM
    {

        protected override void InitVM()
        {
        }

    }

}
