﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace GoSweet.Models
{
    public partial class Ship_datatable
    {
        public Ship_datatable()
        {
            Order_datatables = new HashSet<Order_datatable>();
            p_numbers = new HashSet<Product_datatable>();
        }

        public int ship_number { get; set; }
        public string ship_name { get; set; }

        public virtual ICollection<Order_datatable> Order_datatables { get; set; }

        public virtual ICollection<Product_datatable> p_numbers { get; set; }
    }
}