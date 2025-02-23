﻿using GoSweet.Models;
using System.ComponentModel.DataAnnotations;

namespace GoSweet.Models.ViewModels
{
    public class HomeIndexVm
    {
        public IEnumerable<CategoryViewModel>? CategoryDatas { get; set; }
        public IEnumerable<ProductRankDataViewModel>? ProductRankDatas { get; set; }
        public IEnumerable<ProductGroupBuyData>? ProductGroupBuyDatas { get; set; }
        public FormFileVm FormFileVm { get; set; }
    }

    public class CategoryViewModel
    {
        public string? Category { get; set; }
    }

    public class ProductRankDataViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCategory { get; set; }
        public int? ProductPrice { get; set; }
        public string? ProductPicture { get; set; }
        public string? ProductDescription { get; set; }
        public int ProductTotalBuyNumber { get; set; }
    }

    public class ProductGroupBuyData
    {
        public int? GroupNumber { get; set; }
        public string? ProductName { get; set; }
        public string? ProductPicture { get; set; }
        public string? ProductDescription { get; set; }
        public int GroupMaxPeople { get; set; }
        public int GroupNowPeople { get; set; }
        public double GroupPeoplePercent { get; set; }
        public DateTime GroupEndDate { get; set; }
        public int GroupRemainDate { get; set; }
    }

    public class FormFileVm
    {
        [Required(ErrorMessage = "帳號必填")]
        public FormFile? FileForUpload { get; set; }

        [Required(ErrorMessage = "帳號必填")]
        public FormFileCollection? FilesForUpload { get; set; }
    }
}
